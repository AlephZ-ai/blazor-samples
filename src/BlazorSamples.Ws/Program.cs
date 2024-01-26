using BlazorSamples.PlayHT.Protos.V1;
using BlazorSamples.Ws;
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

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseCors();
app.UseWebSockets();
app.MapGet("/", () => "Hello World!");
app.MapGet("/stream", async (HttpContext context, CancellationToken ct) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
        await ProcessTwilioInputAudio(webSocket, context.RequestServices, ct).ConfigureAwait(false);
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
});

app.Run();

static async Task Echo(WebSocket ws)
{
    var buffer = new byte[1024 * 4];
    int echoCount = 0;
    int maxEchoCount = 3;  // Set this to the number of echoes you want (e.g., 2 or 3)

    while (echoCount < maxEchoCount)
    {
        var receiveResult = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        if (receiveResult.MessageType == WebSocketMessageType.Close)
        {
            break;
        }

        await ws.SendAsync(
            new ArraySegment<byte>(buffer, 0, receiveResult.Count),
            receiveResult.MessageType,
            receiveResult.EndOfMessage,
            CancellationToken.None);

        echoCount++;
    }

    await ws.CloseAsync(
        WebSocketCloseStatus.NormalClosure,
        $"Closing after {maxEchoCount} echoes",
        CancellationToken.None);
}

static async Task ProcessAudio(WebSocket ws, string message, IConfiguration configuration, HttpClient authClient, CancellationToken ct)
{
    await foreach(var chunk in GetAudioAsync(message, configuration, authClient, ct))
    {

    }
}

static async IAsyncEnumerable<ReadOnlyMemory<byte>> GetAudioAsync(string message, IConfiguration configuration, HttpClient authClient, [EnumeratorCancellation] CancellationToken ct)
{
    var authRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.play.ht/api/v2/leases");
    var user = configuration.GetValue<string>("playHT:user");
    var apiKey = configuration.GetValue<string>("playHT:key");
    authRequest.Headers.Add("X-User-Id", user);
    authRequest.Headers.Add("Authorization", $"Bearer {apiKey}");
    var authResponse = await authClient.SendAsync(authRequest, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
    authResponse.EnsureSuccessStatusCode();
    var lease = await authResponse.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);
    var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(1519257480).ToLocalTime();
    var created = epoch.AddSeconds(BinaryPrimitives.ReadUInt32BigEndian(lease.AsSpan(64, 4)));
    var duration = TimeSpan.FromSeconds(BinaryPrimitives.ReadUInt32BigEndian(lease.AsSpan(68, 4)));
    var expires = created.Add(duration);
    using var ms = new MemoryStream(lease[72..]);
    var metadata = (await JsonSerializer.DeserializeAsync<IDictionary<string, JsonElement>>(ms, JsonSerializerOptions.Default, ct).ConfigureAwait(false))!;
    var inferenceAddress = metadata["inference_address"].ToString();
    var headers = new Metadata
    {
        { "Content-Type", "audio/mpeg" },
    };

    var request = new TtsRequest
    {
        Lease = ByteString.CopyFrom(lease),
        Params = new()
        {
            Text = { message },
            Voice = "s3://peregrine-voices/oliver_narrative2_parrot_saad/manifest.json",
            Format = Format.Mp3,
            Temperature = 1.5f,
            Quality = Quality.Premium,
            Speed = 0.8f,
            SpeechAttributes = 9,
            StyleGuidance = 16,
        }
    };


    using var channel = GrpcChannel.ForAddress($"https://{inferenceAddress}");
    var client = new Tts.TtsClient(channel);
    AsyncServerStreamingCall<TtsResponse> response;
    try
    {
        response = client.Tts(request, headers);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        throw;
    }

    await foreach (var item in response.ResponseStream.ReadAllAsync())
    {
        yield return item.Data.Memory;
    }
}

async Task ProcessTwilioInputAudio(
    WebSocket webSocket,
    IServiceProvider serviceProvider,
    CancellationToken ct
)
{
    var appLifetime = serviceProvider.GetRequiredService<IHostApplicationLifetime>();
    var audioConverter = serviceProvider.GetRequiredService<AudioConverter>();
    var recognizer = serviceProvider.GetRequiredService<VoskRecognizer>();

    var buffer = new byte[1024 * 4];
    WebSocketReceiveResult receiveResult;
    while (true)
    {
        receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), ct).ConfigureAwait(false);

        if (receiveResult.CloseStatus.HasValue || appLifetime.ApplicationStopping.IsCancellationRequested)
            break;

        ProcessReceivedMessage(buffer, receiveResult.Count, audioConverter, recognizer);
    }

    await CloseWebSocketConnection(webSocket, appLifetime, receiveResult, ct).ConfigureAwait(false);
}

void ProcessReceivedMessage(byte[] buffer, int count, AudioConverter audioConverter, VoskRecognizer recognizer)
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
            ProcessMediaEvent(jsonDocument, audioConverter, recognizer);
            break;
        case "stop":
            Console.WriteLine("Event: stop");
            break;
    }
}

void HandleStartEvent(JsonDocument jsonDocument)
{
    var streamSid = jsonDocument.RootElement.GetProperty("streamSid").GetString();
    Console.WriteLine($"StreamId: {streamSid}");
}

void ProcessMediaEvent(JsonDocument jsonDocument, AudioConverter audioConverter, VoskRecognizer recognizer)
{
    var payload = jsonDocument.RootElement.GetProperty("media").GetProperty("payload").GetString()!;
    byte[] data = Convert.FromBase64String(payload);
    var (converted, convertedLength) = audioConverter.ConvertBuffer(data);
    if (recognizer.AcceptWaveform(converted, convertedLength))
    {
        var json = recognizer.Result();
        var jsonDoc = JsonSerializer.Deserialize<JsonDocument>(json)!;
        Console.WriteLine(jsonDoc.RootElement.GetProperty("text").GetString());
    }
    else
    {
        var json = recognizer.PartialResult();
        var jsonDoc = JsonSerializer.Deserialize<JsonDocument>(recognizer.PartialResult())!;
        Console.WriteLine(jsonDoc.RootElement.GetProperty("partial").GetString());
    }
}

async Task CloseWebSocketConnection(WebSocket webSocket, IHostApplicationLifetime appLifetime, WebSocketReceiveResult receiveResult, CancellationToken ct)
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
