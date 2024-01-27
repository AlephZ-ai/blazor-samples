using BlazorSamples.PlayHT.Protos.V1;
using BlazorSamples.Shared;
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

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddHttpClient();
builder.Services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders = ForwardedHeaders.All);
builder.AddAzureOpenAI("openai");
builder.Services.AddScoped<IAudioConverter, AudioConverter>();
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
    var cts = CancellationTokenSource.CreateLinkedTokenSource(ct, appLifetime.ApplicationStopping);
    while (true)
    {
        receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token).ConfigureAwait(false);
        if (receiveResult.CloseStatus.HasValue || appLifetime.ApplicationStopping.IsCancellationRequested)
        {
            cts.Cancel();
            break;
        }

        await ProcessReceivedMessage(buffer, receiveResult.Count, audioConverter, recognizer, cts.Token);
    }

    await CloseWebSocketConnection(webSocket, appLifetime, receiveResult, cts.Token).ConfigureAwait(false);
}

static async Task ProcessReceivedMessage(byte[] buffer, int count, IAudioConverter audioConverter, ISpeechToTextProvider recognizer, CancellationToken ct)
{
    using var jsonDocument = JsonSerializer.Deserialize<JsonDocument>(buffer.AsSpan(0, count))!;
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
            await ProcessMediaEvent(jsonDocument, audioConverter, recognizer, ct);
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

static async Task ProcessMediaEvent(JsonDocument jsonDocument, IAudioConverter audioConverter, ISpeechToTextProvider recognizer, CancellationToken ct)
{
    var payload = jsonDocument.RootElement.GetProperty("media").GetProperty("payload").GetString()!;
    byte[] data = Convert.FromBase64String(payload);
    await audioConverter.InitializationAsync(converted =>
    {
        Console.WriteLine($"Converted audio buffer size: {converted?.Length}");
        return Task.CompletedTask;
        // var result = await recognizer.AppendWavChunk(converted, converted.Length)!;
        // if (result.CompleteSentence is not null)
        // {
        //     Console.WriteLine(result.CompleteSentence);
        // }
        // else
        // {
        //     Console.WriteLine(result.SentenceFragment);
        // }
    }, ct);

    await audioConverter.ProcessAudioBuffer(data);
    await audioConverter.ClosePipes();
}

static async Task CloseWebSocketConnection(WebSocket webSocket, IHostApplicationLifetime appLifetime, WebSocketReceiveResult receiveResult, CancellationToken ct)
{
    if (receiveResult.CloseStatus.HasValue)
    {
        await webSocket.CloseAsync(receiveResult.CloseStatus.Value, receiveResult.CloseStatusDescription, ct).ConfigureAwait(false);
    }
    else if (appLifetime.ApplicationStopping.IsCancellationRequested)
    {
        await webSocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "Server shutting down", ct).ConfigureAwait(false);
    }
}
