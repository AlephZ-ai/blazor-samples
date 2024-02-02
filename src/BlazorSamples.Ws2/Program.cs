using Microsoft.AspNetCore.HttpOverrides;
using System.Net.WebSockets;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions;
using System.Runtime.CompilerServices;
using System.Text.Json;
using BlazorSamples.Shared.AudioConverter.Ffmpeg;
using FFMpegCore.Enums;
using BlazorSamples.Shared.AudioConverter;
using BlazorSamples.Shared.SpeechRecognition;
using BlazorSamples.Shared.SpeechRecognition.Vosk;

var initialBufferSize = 4 * 1024;
var jsonOptions = JsonSerializerOptions.Default;
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

builder.AddFfmpegAudioConverter(new FfmpegAudioConverterOptions
{
    InitialBufferSize = initialBufferSize,
    InFormat = "mulaw",
    InSampleRate = 8000,
    OutFormat = "wav",
    OutSampleRate = 16000,
    OutSpeed = Speed.VeryFast,
});

builder.AddVoskSpeechRecognizer(new VoskSpeechRecognizerOptions
{
    JsonOptions = jsonOptions,
});

var app = builder.Build();
var logger = app.Services.GetRequiredService<ILogger<Program>>();
var sr = app.Services.GetRequiredService<ISpeechRecognizer>();
logger.LogInformation("Models downloading");
await sr.EnsureModelsDownloadedAsync().ConfigureAwait(false);
logger.LogInformation("Models downloaded");
app.MapDefaultEndpoints();
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseForwardedHeaders();
app.UseCors();
app.UseWebSockets();
app.MapGet("/", () => @"[""Hello World!""]");
app.MapGet("/stream", async (
    HttpContext context,
    IAudioConverter audioConverter,
    ISpeechRecognizer speechRecognizer,
    CancellationToken ct) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
        await ProcessAsync(webSocket, initialBufferSize, jsonOptions, audioConverter, speechRecognizer, ct).ConfigureAwait(false);
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
})
.WithName("Stream")
.RequireCors();




await app.RunAsync();




static async Task ProcessAsync(
    WebSocket webSocket,
    int initialBufferSize,
    JsonSerializerOptions jsonOptions,
    IAudioConverter audioConverter,
    ISpeechRecognizer speechRecognizer,
    CancellationToken ct = default)
{
    var receiveLoop = webSocket
        .ReadAllAsync(initialBufferSize, ct)
        .RecombineFragmentsAsync(initialBufferSize, ct)
        .ExcludeEmpty()
        .ConvertFromJsonAsync<IInboundEvent>(jsonOptions)
        .ExcludeNull()
        .ProcessTwilioEvent()
        .ExcludeNull()
        .Select(twilio => twilio.Payload)
        .ConvertAudioAsync(audioConverter, ct)
        .RecognizeSpeechAsync(speechRecognizer, ct);

    await webSocket.SendAllAsync(receiveLoop.ToJsonBytesAsync(jsonOptions), WebSocketMessageType.Text, ct)
        .LastAsync(cancellationToken: ct)
        .ConfigureAwait(false);

    await webSocket.CloseAsync(webSocket.CloseStatus ?? WebSocketCloseStatus.EndpointUnavailable, webSocket.CloseStatusDescription ?? "Server shutting down", ct).ConfigureAwait(false);
}
