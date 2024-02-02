using BlazorSamples.Ws2;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net.WebSockets;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions;
using System.Runtime.CompilerServices;
using System.Text.Json;

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
    CancellationToken ct) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
        await EchoJson(webSocket, ct).ConfigureAwait(false);
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
})
.WithName("Stream")
.RequireCors();




await app.RunAsync();




static async Task EchoJson(WebSocket webSocket, CancellationToken ct = default)
{
    var initialBufferSize = 4 * 1024;
    var jsonOptions = new JsonSerializerOptions();

    var receiveLoop = webSocket
        .ReceiveAsyncEnumerable(1024 * 4, ct)
        .RecombineFragmentsAsync(initialBufferSize, ct)
        .ExcludeEmpty()
        .ToTFromJsonAsyncEnumerable<IInboundEvent>(jsonOptions)
        .ExcludeNull()
        .ProcessEvent()
        .ExcludeNull();

    var receiveLoopEchoWrapper = receiveLoop.ToJsonBytesAsyncEnumerable(jsonOptions);
    await webSocket.SendAsyncEnumerable(receiveLoopEchoWrapper, WebSocketMessageType.Text, ct)
        .LastAsync(cancellationToken: ct)
        .ConfigureAwait(false);

    await webSocket.CloseAsync(webSocket.CloseStatus ?? WebSocketCloseStatus.EndpointUnavailable, webSocket.CloseStatusDescription ?? "Server shutting down", ct).ConfigureAwait(false);
}
