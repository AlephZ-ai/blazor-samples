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
using BlazorSamples.Shared.TextToSpeech;
using BlazorSamples.Shared.TextToText;
using System.Diagnostics;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Media;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Clear;
using BlazorSamples.Shared.Old.SpeechToText;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddHttpClient();
builder.Services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders = ForwardedHeaders.All);
builder.AddAzureOpenAI("openai");
builder.Services.AddScoped<IAudioConverter, FfmpegPipeAudioConverter>();
builder.Services.AddScoped<ITextToSpeech, PlayHTTextToSpeech>();
builder.Services.AddScoped<ITextToText, OpenAITextToText>();
bool isVosk = true;
if (isVosk)
{
    await VoskSpeechToTextProvider.DownloadModelsAsync();
    builder.Services.AddScoped<ISpeechToTextProvider, VoskSpeechToTextProvider>();
}
else
{
    await WhisperSpeechToTextProvider.DownloadModelsAsync();
    builder.Services.AddScoped<ISpeechToTextProvider, WhisperSpeechToTextProvider>();
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
app.Lifetime.ApplicationStopping.Register(OnShutdown);
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
    ITextToText textToText,
    ITextToSpeech textToSpeech,
    CancellationToken ct
) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
        await ProcessTwilioInputAudio(webSocket, appLifetime, audioConverter, recognizer, textToText, textToSpeech, ct).ConfigureAwait(false);
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
});

app.Run();

void OnShutdown()
{
    PlayHTTextToSpeech.ShutdownChannel();
}

static async Task ProcessTwilioInputAudio(
    WebSocket webSocket,
    IHostApplicationLifetime appLifetime,
    IAudioConverter audioConverter,
    ISpeechToTextProvider recognizer,
    ITextToText textToText,
    ITextToSpeech textToSpeech,
    CancellationToken ct
)
{
    var buffer = new byte[1024 * 4];
    WebSocketReceiveResult receiveResult;
    var cts = new CancellationTokenSource();
    string? streamSid = null;
    await audioConverter.InitializationAsync(async converted =>
    {
        if (converted is not null && converted.Length > 0)
        {
            var result = await recognizer.AppendWavChunk(converted, converted.Length).ConfigureAwait(false)!;
            if (!string.IsNullOrEmpty(result.CompleteSentence))
            {
                await foreach (var responseSentence in textToText.StreamingResponse(result.CompleteSentence, ct).WithCancellation(ct).ConfigureAwait(false))
                {
                    try
                    {
                        await foreach (var responseAudioChunk in textToSpeech.Voice(responseSentence, ct).WithCancellation(ct).ConfigureAwait(false))
                        {
                            var audio = new OutboundMediaEvent
                            {
                                StreamSid = streamSid!,
                                Media = new()
                                {
                                    Payload = responseAudioChunk,
                                },
                            };

                            var json = JsonSerializer.SerializeToUtf8Bytes(audio);
                            await webSocket.SendAsync(json, WebSocketMessageType.Text, true, ct).ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
            else if (!string.IsNullOrEmpty(result.SentenceFragment))
            {
                var clear = new OutboundClearEvent { StreamSid = streamSid! };
                var json = JsonSerializer.SerializeToUtf8Bytes(clear);
                await webSocket.SendAsync(json, WebSocketMessageType.Text, true, ct).ConfigureAwait(false);
            }
        }
    }, cts.Token);

    while (true)
    {
        receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), ct).ConfigureAwait(false);
        if (receiveResult.CloseStatus.HasValue || appLifetime.ApplicationStopping.IsCancellationRequested)
            break;

        streamSid = await ProcessReceivedMessage(buffer[..receiveResult.Count], audioConverter, streamSid);
    }

    await audioConverter.ClosePipes().ConfigureAwait(false);
    await CloseWebSocketConnection(webSocket, appLifetime, receiveResult).ConfigureAwait(false);
    cts.Cancel();
}

static async Task<string> ProcessReceivedMessage(byte[] buffer, IAudioConverter audioConverter, string? streamSid)
{
    using var jsonDocument = JsonSerializer.Deserialize<JsonDocument>(buffer)!;
    var eventMessage = jsonDocument.RootElement.GetProperty("event").GetString();
    switch (eventMessage)
    {
        case "connected":
            Console.WriteLine("Event: connected");
            break;
        case "start":
            streamSid = HandleStartEvent(jsonDocument);
            break;
        case "media":
            await ProcessMediaEvent(jsonDocument, audioConverter);
            break;
        case "stop":
            Console.WriteLine("Event: stop");
            break;
    }

    return streamSid!;
}

static string HandleStartEvent(JsonDocument jsonDocument)
{
    var streamSid = jsonDocument.RootElement.GetProperty("streamSid").GetString()!;
    return streamSid;
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
