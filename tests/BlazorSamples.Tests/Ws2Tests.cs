using BlazorSamples.Ws2;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;

namespace BlazorSamples.Tests
{
    [TestClass]
    public class Ws2Tests
    {
        private static TestContext _context = null!;
        private static readonly string _assemblyName = typeof(Ws2Tests).Assembly.GetName().Name!;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _context = context;
        }

        [TestMethod]
        public async Task EchoTest()
        {
            // Arrange
            TestWebSocketServer.Path = "/echo";
            TestWebSocketServer.DefaultBufferSize = 1024 * 4;
            TestWebSocketServer.WebSocketFunction = Echo;
            var ct = _context.CancellationTokenSource.Token;
            var factory = CreateFactory();
            var client = factory.Server.CreateWebSocketClient();
            var uri = factory.Server.BaseAddress.ToWs();

            // Act
            var webSocket = await client.ConnectAsync(uri, ct).ConfigureAwait(false);
            var request = "Hello, WebSocket!";
            await SendStringAsync(webSocket,request, ct).ConfigureAwait(false);
            var response = await ReceiveStringAsync(webSocket, ct).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(request, response);

            // Act
            request = "Hello, WebSocket2!";
            await SendStringAsync(webSocket, request, ct).ConfigureAwait(false);
            response = await ReceiveStringAsync(webSocket, ct).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(request, response);

            // Act
            request = "33Hello, WebSocket!";
            await SendStringAsync(webSocket, request, ct).ConfigureAwait(false);
            response = await ReceiveStringAsync(webSocket, ct).ConfigureAwait(false);
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "OK", ct).ConfigureAwait(false);

            // Assert
            Assert.AreEqual(request, response);
        }


        static async Task Echo(WebSocket webSocket, CancellationToken ct = default)
        {
            var receiveLoop = webSocket.ReceiveAsyncEnumerable(TestWebSocketServer.DefaultBufferSize, ct).RecombineFragmentsAsync(TestWebSocketServer.DefaultBufferSize, ct);
            await webSocket.SendAsyncEnumerable(receiveLoop, WebSocketMessageType.Text, ct).LastAsync(cancellationToken: ct).ConfigureAwait(false);
            await webSocket.CloseAsync(webSocket.CloseStatus ?? WebSocketCloseStatus.EndpointUnavailable, webSocket.CloseStatusDescription ?? "Server shutting down", ct).ConfigureAwait(false);
        }

        static WebApplicationFactory<TestWebSocketServer> CreateFactory()
        {
            var factory = new WebApplicationFactory<TestWebSocketServer>()
            .WithWebHostBuilder(builder =>
            {
                builder
                .UseSolutionRelativeContentRoot($"tests/{_assemblyName}")
                .UseTestServer(options =>
                {
                    options.PreserveExecutionContext = true;
                    options.AllowSynchronousIO = false;
                });
            });

            factory.ConfigureAwait(false);
            return factory;
        }

        static ValueTask SendStringAsync(WebSocket webSocket, string message, CancellationToken ct = default)
        {
            var buffer = new Memory<byte>(Encoding.UTF8.GetBytes(message));
            return webSocket.SendAsync(buffer, WebSocketMessageType.Text, WebSocketMessageFlags.EndOfMessage, ct);
        }

        static async Task<string> ReceiveStringAsync(WebSocket webSocket, CancellationToken ct = default)
        {
            var buffer = new Memory<byte>(new byte[TestWebSocketServer.DefaultBufferSize]);
            var message = new StringBuilder();
            ValueWebSocketReceiveResult result;
            do
            {
                result = await webSocket.ReceiveAsync(buffer, ct).ConfigureAwait(false);
                if (result.Count > 0)
                    message.Append(Encoding.UTF8.GetString(buffer[..result.Count].ToArray()));
            } while (!result.EndOfMessage);

            return message.ToString();
        }
    }
}
