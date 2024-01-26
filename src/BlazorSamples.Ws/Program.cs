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

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddHttpClient();
builder.Services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders = ForwardedHeaders.All);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddSingleton<IAudioConverter, AudioConverter>();
builder.Services.AddSingleton<ISpeechToTextProvider, VoskSpeechToTextProvider>();
var app = builder.Build();
var stt = app.Services.GetRequiredService<ISpeechToTextProvider>();
await stt.DownloadModelsAsync();
app.MapDefaultEndpoints();
app.UseCors();
app.UseWebSockets();
app.MapGet("/", () => "Hello World!");
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
    while (true)
    {
        receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), ct).ConfigureAwait(false);

        if (receiveResult.CloseStatus.HasValue || appLifetime.ApplicationStopping.IsCancellationRequested)
            break;

        await ProcessReceivedMessage(buffer, receiveResult.Count, audioConverter, recognizer);
    }

    await CloseWebSocketConnection(webSocket, appLifetime, receiveResult, ct).ConfigureAwait(false);
}

static async Task ProcessReceivedMessage(byte[] buffer, int count, IAudioConverter audioConverter, ISpeechToTextProvider recognizer)
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
            await ProcessMediaEvent(jsonDocument, audioConverter, recognizer);
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

static async Task ProcessMediaEvent(JsonDocument jsonDocument, IAudioConverter audioConverter, ISpeechToTextProvider recognizer)
{
    var payload = jsonDocument.RootElement.GetProperty("media").GetProperty("payload").GetString()!;
    byte[] data = Convert.FromBase64String(payload);
    await audioConverter.InitializationAsync();
    await foreach (var converted in audioConverter.ProcessAudioBuffer(data))
    {
        var result = await recognizer.AppendWavChunk(converted, converted.Length)!;
        if (result.CompleteSentence is not null)
        {
            Console.WriteLine(result.CompleteSentence);
        }
        else
        {
            Console.WriteLine(result.SentenceFragment);
        }
    }

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
