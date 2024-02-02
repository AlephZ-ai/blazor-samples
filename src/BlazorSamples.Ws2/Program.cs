using Microsoft.AspNetCore.HttpOverrides;
using System.Net.WebSockets;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions;
using System.Runtime.CompilerServices;
using System.Text.Json;
using BlazorSamples.Shared.AudioConverter.Ffmpeg;
using FFMpegCore.Enums;
using BlazorSamples.Shared.AudioConverter;
using BlazorSamples.Shared.ChatCompletion;
using BlazorSamples.Shared.ChatCompletion.OpenAI;
using BlazorSamples.Shared.SpeechRecognition;
using BlazorSamples.Shared.SpeechRecognition.Vosk;

var initialBufferSize = 4 * 1024;
var outSampleRate = 16000;
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

builder.AddFfmpegAudioConverter(new()
{
    InitialBufferSize = initialBufferSize,
    InFormat = "mulaw",
    InSampleRate = 8000,
    OutFormat = "wav",
    OutSampleRate = outSampleRate,
    OutSpeed = Speed.VeryFast,
});

builder.AddVoskSpeechRecognizer(new()
{
    JsonOptions = jsonOptions,
    SampleRate = outSampleRate,
    Words = true,
    PartialWords = false,
});

var openAIOptions = new OpenAIChatCompleterOptions
{
    SystemMessage = "You are a helpful AI audio voice assistance who answers the phone."
};

builder.AddAzureOpenAI(openAIOptions.ConnectionName);
builder.AddOpenAIChatCompleter(openAIOptions);

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
    IChatCompleter chatCompleter,
    CancellationToken ct) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
        await ProcessAsync(webSocket, initialBufferSize, jsonOptions, audioConverter, speechRecognizer, chatCompleter, ct).ConfigureAwait(false);
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
    IChatCompleter chatCompleter,
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
        .RecognizeSpeechAsync(speechRecognizer, ct)
        .Where(text => text.IsPauseDetected && !string.IsNullOrEmpty(text.Fragment))
        .Select(text => text.Fragment!)
        .CompleteChatAsync(chatCompleter, ct);

    await webSocket.SendAllAsync(receiveLoop.ToBytesAsync(initialBufferSize), WebSocketMessageType.Text, ct)
        .LastAsync(cancellationToken: ct)
        .ConfigureAwait(false);

    await webSocket.CloseAsync(webSocket.CloseStatus ?? WebSocketCloseStatus.EndpointUnavailable, webSocket.CloseStatusDescription ?? "Server shutting down", ct).ConfigureAwait(false);
}
