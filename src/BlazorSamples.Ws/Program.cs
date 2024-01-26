using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
var app = builder.Build();

app.MapDefaultEndpoints();
app.UseWebSockets();
app.MapGet("/", () => "Hello World!");
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await Echo(webSocket);
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
    else
    {
        await next(context);
    }

});

app.Run();

static async Task Echo(WebSocket webSocket)
{
    var buffer = new byte[1024 * 4];
    int echoCount = 0;
    int maxEchoCount = 3;  // Set this to the number of echoes you want (e.g., 2 or 3)

    while (echoCount < maxEchoCount)
    {
        var receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);

        if (receiveResult.MessageType == WebSocketMessageType.Close)
        {
            break;
        }

        await webSocket.SendAsync(
            new ArraySegment<byte>(buffer, 0, receiveResult.Count),
            receiveResult.MessageType,
            receiveResult.EndOfMessage,
            CancellationToken.None);

        echoCount++;
    }

    await webSocket.CloseAsync(
        WebSocketCloseStatus.NormalClosure,
        $"Closing after {maxEchoCount} echoes",
        CancellationToken.None);
}
