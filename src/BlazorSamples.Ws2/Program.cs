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
app.Run();

app.MapGet("/stream", async (
    HttpContext context,
    CancellationToken ct
) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        await Echo(webSocket, ct);
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
})
.WithName("Stream")
.RequireCors();

static async Task Echo(WebSocket webSocket, CancellationToken ct = default)
{
    var receiveLoop = webSocket.ReceiveAsyncEnumerable(1024 * 4, ct).Select(r => r.Buffer);
    await webSocket.SendAsyncEnumerable(receiveLoop, WebSocketMessageType.Text, ct).LastAsync().ConfigureAwait(false);
    await webSocket.CloseAsync(webSocket.CloseStatus!.Value, webSocket.CloseStatusDescription, ct);
}
