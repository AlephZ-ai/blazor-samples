using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net.WebSockets;
using System.Text;

namespace BlazorSamples.Tests
{
    [TestClass]
    public class Ws2Tests
    {
        private static TestContext _context = null!;
        private static WebApplicationFactory<Program> _factory = null!;
        private static WebSocketClient _client = null!;
        private static Uri _uri = null!;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _context = context;
            _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseTestServer(options =>
                {
                    options.PreserveExecutionContext = true;
                    options.AllowSynchronousIO = false;
                });
            });

            _factory.ConfigureAwait(false);
            _client = _factory.Server.CreateWebSocketClient();
            var uriBuilder = new UriBuilder(_factory.Server.BaseAddress);
            uriBuilder.Scheme = uriBuilder.Scheme == "https" ? "wss" : "ws";
            uriBuilder.Path = "/stream";
            _uri = uriBuilder.Uri;
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            _factory.Dispose();
        }

        [TestMethod]
        public async Task Ws2Test()
        {
            var bufferSize = 1024 * 4;
            var request = "Hello, WebSocket!";
            var requestBuffer = new Memory<byte>(Encoding.UTF8.GetBytes(request));
            var responseBuffer = new Memory<byte>(new byte[bufferSize]);
            using var webSocket = await _client.ConnectAsync(_uri, _context.CancellationTokenSource.Token);
            await webSocket.SendAsync(requestBuffer, WebSocketMessageType.Text, WebSocketMessageFlags.None, _context.CancellationTokenSource.Token);
            var receiveResult = await webSocket.ReceiveAsync(responseBuffer, _context.CancellationTokenSource.Token);
            var response = Encoding.UTF8.GetString(responseBuffer[..receiveResult.Count].ToArray());
            Assert.AreEqual(request, response);

            request = "Hello, WebSocket2!";
            requestBuffer = new Memory<byte>(Encoding.UTF8.GetBytes(request));
            responseBuffer = new Memory<byte>(new byte[bufferSize]);
            await webSocket.SendAsync(requestBuffer, WebSocketMessageType.Text, WebSocketMessageFlags.None, _context.CancellationTokenSource.Token);
            receiveResult = await webSocket.ReceiveAsync(responseBuffer, _context.CancellationTokenSource.Token);
            response = Encoding.UTF8.GetString(responseBuffer[..receiveResult.Count].ToArray());
            Assert.AreEqual(request, response);

            request = "Hello, WebSocket3!";
            requestBuffer = new Memory<byte>(Encoding.UTF8.GetBytes(request));
            responseBuffer = new Memory<byte>(new byte[bufferSize]);
            await webSocket.SendAsync(requestBuffer, WebSocketMessageType.Text, WebSocketMessageFlags.None, _context.CancellationTokenSource.Token);
            receiveResult = await webSocket.ReceiveAsync(responseBuffer, _context.CancellationTokenSource.Token);
            response = Encoding.UTF8.GetString(responseBuffer[..receiveResult.Count].ToArray());
            Assert.AreEqual(request, response);

            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "OK", _context.CancellationTokenSource.Token);
        }
    }
}
