using BlazorSamples.PlayHT.Protos.V1;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.StaticFiles;
using System.Buffers.Binary;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Vosk;
using Azure.AI.OpenAI;
using Twilio.TwiML;
using Twilio.AspNet.Common;
using Twilio.AspNet.Core;
using System.Text;
using BlazorSamples.Shared.AudioConverter;
using BlazorSamples.Shared.SpeechToText;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddHttpClient();
builder.Services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders = ForwardedHeaders.All);
builder.AddAzureOpenAI("openai");
builder.Services.AddScoped<IAudioConverter, FfmpegPipeAudioConverter>();
bool isVosk = true;
if (isVosk)
{
    await VoskSpeechToTextProvider.DownloadModelsAsync();
    builder.Services.AddSingleton<ISpeechToTextProvider, VoskSpeechToTextProvider>();
}
else
{
    await WhisperSpeechToTextProvider.DownloadModelsAsync();
    builder.Services.AddSingleton<ISpeechToTextProvider, WhisperSpeechToTextProvider>();
}

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
var warmup = app.Services.GetRequiredService<ISpeechToTextProvider>();
app.MapDefaultEndpoints();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseForwardedHeaders();
app.UseCors();
app.UseWebSockets();
app.MapGet("/", () => "Hello World!");
app.MapPost("/voice", TwiMLResult (HttpRequest request) =>
{
    var response = new VoiceResponse();
    var connect = new Twilio.TwiML.Voice.Connect();
    connect.Stream(url: $"wss://{request.Host}/stream");
    response.Append(connect);
    return response.ToTwiMLResult();
});

app.MapGet("/stream", async (
    HttpContext context,
    IHostApplicationLifetime appLifetime,
    IAudioConverter audioConverter,
    ISpeechToTextProvider recognizer,
    CancellationToken ct
) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
        await ProcessTwilioInputAudio(webSocket, appLifetime, audioConverter, recognizer, ct).ConfigureAwait(false);
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
});

app.Run();

static async Task ProcessTwilioInputAudio(
    WebSocket webSocket,
    IHostApplicationLifetime appLifetime,
    IAudioConverter audioConverter,
    ISpeechToTextProvider recognizer,
    CancellationToken ct
)
{
    var buffer = new byte[1024 * 4];
    WebSocketReceiveResult receiveResult;
    var cts = new CancellationTokenSource();
    await audioConverter.InitializationAsync(async converted =>
    {
        if (converted is not null && converted.Length > 0)
        {
            var result = await recognizer.AppendWavChunk(converted, converted.Length)!;
            if (result.CompleteSentence is not null)
            {
                if (!string.IsNullOrEmpty(result.CompleteSentence))
                    Console.WriteLine(result.CompleteSentence);
            }
            else
            {
                if (!string.IsNullOrEmpty(result.SentenceFragment))
                    Console.WriteLine(result.SentenceFragment);
            }
        }
    }, cts.Token);

    while (true)
    {
        receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), ct).ConfigureAwait(false);
        if (receiveResult.CloseStatus.HasValue || appLifetime.ApplicationStopping.IsCancellationRequested)
            break;

        await ProcessReceivedMessage(buffer[..receiveResult.Count], audioConverter);
    }

    await audioConverter.ClosePipes().ConfigureAwait(false);
    await CloseWebSocketConnection(webSocket, appLifetime, receiveResult).ConfigureAwait(false);
    cts.Cancel();
}

static async Task ProcessReceivedMessage(byte[] buffer, IAudioConverter audioConverter)
{
    using var jsonDocument = JsonSerializer.Deserialize<JsonDocument>(buffer)!;
    var eventMessage = jsonDocument.RootElement.GetProperty("event").GetString();
    switch (eventMessage)
    {
        case "connected":
            Console.WriteLine("Event: connected");
            break;
        case "start":
            HandleStartEvent(jsonDocument);
            break;
        case "media":
            await ProcessMediaEvent(jsonDocument, audioConverter);
            break;
        case "stop":
            Console.WriteLine("Event: stop");
            break;
    }
}

static void HandleStartEvent(JsonDocument jsonDocument)
{
    var streamSid = jsonDocument.RootElement.GetProperty("streamSid").GetString();
    Console.WriteLine($"StreamId: {streamSid}");
}

static async Task ProcessMediaEvent(JsonDocument jsonDocument, IAudioConverter audioConverter)
{
    var payload = jsonDocument.RootElement.GetProperty("media").GetProperty("payload").GetString()!;
    byte[] data = Convert.FromBase64String(payload);
    await audioConverter.ProcessAudioBuffer(data);
}

static async Task CloseWebSocketConnection(WebSocket webSocket, IHostApplicationLifetime appLifetime, WebSocketReceiveResult receiveResult)
{
    if (receiveResult.CloseStatus.HasValue)
    {
        await webSocket.CloseAsync(receiveResult.CloseStatus.Value, receiveResult.CloseStatusDescription, default).ConfigureAwait(false);
    }
    else if (appLifetime.ApplicationStopping.IsCancellationRequested)
    {
        await webSocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "Server shutting down", default).ConfigureAwait(false);
    }
}
