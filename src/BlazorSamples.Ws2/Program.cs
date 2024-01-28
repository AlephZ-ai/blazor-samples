using BlazorSamples.Ws2;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;

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
app.MapGet("/", () => "Hello World!");
app.MapGet("/stream", async (
    HttpContext context,
    CancellationToken ct
) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        await Echo2(webSocket, ct);
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
})
.WithName("Stream")
.RequireCors();




app.Run();




static async Task Echo(WebSocket webSocket, CancellationToken ct = default)
{
    var receiveLoop = webSocket.ReceiveAsyncEnumerable(1024 * 4, ct).Select(r => r.Buffer);
    await webSocket.SendAsyncEnumerable(receiveLoop, WebSocketMessageType.Text, ct).LastAsync().ConfigureAwait(false);
    await webSocket.CloseAsync(webSocket.CloseStatus!.Value, webSocket.CloseStatusDescription, ct);
}

static async Task Echo2(WebSocket webSocket, CancellationToken ct)
{
    var buffer = new byte[1024 * 4];
    var receiveResult = await webSocket.ReceiveAsync(
        new ArraySegment<byte>(buffer), ct);

    while (!receiveResult.CloseStatus.HasValue)
    {
        await webSocket.SendAsync(
            new ArraySegment<byte>(buffer, 0, receiveResult.Count),
            receiveResult.MessageType,
            receiveResult.EndOfMessage,
            ct);

        receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), ct);
    }

    await webSocket.CloseAsync(
        receiveResult.CloseStatus.Value,
        receiveResult.CloseStatusDescription,
        CancellationToken.None);
}
