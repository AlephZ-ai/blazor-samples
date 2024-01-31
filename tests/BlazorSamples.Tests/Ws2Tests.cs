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
using System.Text.Json;

namespace BlazorSamples.Tests
{
    [TestClass]
    public class Ws2Tests
    {
        private static TestContext _context = null!;
        private const string _chars = @"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        private static readonly Random _random = new Random(69);
        private static readonly string _assemblyName = typeof(Ws2Tests).Assembly.GetName().Name!;
        private int _serverReceiveCount = 0;
        private int _serverSendCount = 0;
        private int _serverRecombinedReceiveCount = 0;
        private int _serverCloseCount = 0;
        private Task? _serverClosed;
        private int _clientReceiveCount = 0;
        private int _clientSendCount = 0;
        private int _clientRecombinedReceiveCount = 0;
        private int _clientCloseCount = 0;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _context = context;
        }

        [TestMethod]
        public async Task _EchoTest()
        {
            // Arrange
            TestWebSocketServer.Path = "/echo";
            TestWebSocketServer.DefaultBufferSize = 1024 * 4;
            TestWebSocketServer.WebSocketFunction = Echo;
            var ct = _context.CancellationTokenSource.Token;
            var factory = CreateFactory();
            var client = factory.Server.CreateWebSocketClient();
            var uri = factory.Server.BaseAddress.ToWs();
            _serverReceiveCount = 0;
            _serverSendCount = 0;
            _serverClosed = null;
            _clientReceiveCount = 0;
            _clientSendCount = 0;

            // Act and Assert
            var expectedCount = 1;
            var webSocket = await client.ConnectAsync(uri, ct).ConfigureAwait(false);
            var request = "Hello, WebSocket!";
            await SendStringAsync(webSocket,request, ct).ConfigureAwait(false);
            Assert.AreEqual(expectedCount, _clientSendCount);
            var response = await ReceiveStringAsync(webSocket, ct).ConfigureAwait(false);
            Assert.AreEqual(expectedCount, _serverReceiveCount);
            Assert.IsTrue(await IsTrueAsync(() => expectedCount == _serverSendCount, ct: ct).ConfigureAwait(false));
            Assert.AreEqual(expectedCount, _clientReceiveCount);
            Assert.AreEqual(request, response);

            // Act and Assert
            expectedCount++;
            request = "Hello, WebSocket2!";
            await SendStringAsync(webSocket, request, ct).ConfigureAwait(false);
            Assert.AreEqual(expectedCount, _clientSendCount);
            response = await ReceiveStringAsync(webSocket, ct).ConfigureAwait(false);
            Assert.AreEqual(expectedCount, _serverReceiveCount);
            Assert.IsTrue(await IsTrueAsync(() => expectedCount == _serverSendCount, ct: ct).ConfigureAwait(false));
            Assert.AreEqual(expectedCount, _clientReceiveCount);
            Assert.AreEqual(request, response);

            // Act and Assert
            expectedCount++;
            request = "33Hello, WebSocket!";
            await SendStringAsync(webSocket, request, ct).ConfigureAwait(false);
            Assert.AreEqual(expectedCount, _clientSendCount);
            response = await ReceiveStringAsync(webSocket, ct).ConfigureAwait(false);
            Assert.AreEqual(expectedCount, _serverReceiveCount);
            Assert.IsTrue(await IsTrueAsync(() => expectedCount == _serverSendCount, ct: ct).ConfigureAwait(false));
            Assert.AreEqual(expectedCount, _clientReceiveCount);
            Assert.AreEqual(request, response);
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "OK", ct).ConfigureAwait(false);
            Assert.IsTrue(await IsTrueAsync(() => _serverClosed != null, ct: ct));
            await _serverClosed!;
        }

        [TestMethod]
        public async Task EchoLongTest()
        {
            // Arrange
            TestWebSocketServer.Path = "/echo-large";
            TestWebSocketServer.DefaultBufferSize = 1024 * 4;
            TestWebSocketServer.WebSocketFunction = EchoLarge;
            var ct = _context.CancellationTokenSource.Token;
            var factory = CreateFactory();
            var client = factory.Server.CreateWebSocketClient();
            var uri = factory.Server.BaseAddress.ToWs();
            _serverReceiveCount = 0;
            _serverSendCount = 0;
            _serverRecombinedReceiveCount = 0;
            _serverClosed = null;
            _clientReceiveCount = 0;
            _clientSendCount = 0;
            _clientRecombinedReceiveCount = 0;

            // Act and Assert
            var expectedCount = 1;
            var webSocket = await client.ConnectAsync(uri, ct).ConfigureAwait(false);
            var request = GenerateRandomString(5000);
            await SendStringAsync(webSocket, request, ct).ConfigureAwait(false);
            Assert.AreEqual(expectedCount, _clientSendCount);
            var response = await ReceiveStringAsync(webSocket, ct).ConfigureAwait(false);
            Assert.AreEqual(expectedCount + 1, _serverReceiveCount);
            Assert.AreEqual(expectedCount, _serverRecombinedReceiveCount);
            Assert.IsTrue(await IsTrueAsync(() => expectedCount == _serverSendCount, ct: ct).ConfigureAwait(false));
            Assert.AreEqual(expectedCount + 1, _clientReceiveCount);
            Assert.AreEqual(expectedCount, _clientRecombinedReceiveCount);
            Assert.AreEqual(request, response);
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "OK", ct).ConfigureAwait(false);
            Assert.IsTrue(await IsTrueAsync(() => _serverClosed != null, ct: ct));
            await _serverClosed!;
        }

        [TestMethod]
        public async Task JsonTest()
        {
            // Arrange
            TestWebSocketServer.Path = "/json";
            TestWebSocketServer.DefaultBufferSize = 1024 * 4;
            TestWebSocketServer.WebSocketFunction = Json;
            var ct = _context.CancellationTokenSource.Token;
            var factory = CreateFactory();
            var client = factory.Server.CreateWebSocketClient();
            var uri = factory.Server.BaseAddress.ToWs();
            _serverReceiveCount = 0;
            _serverSendCount = 0;
            _serverRecombinedReceiveCount = 0;
            _serverClosed = null;
            _clientReceiveCount = 0;
            _clientSendCount = 0;
            _clientRecombinedReceiveCount = 0;

            // Act and Assert
            var expectedCount = 1;
            var webSocket = await client.ConnectAsync(uri, ct).ConfigureAwait(false);
            var request = new JsonTest { Message = GenerateRandomString(5000) };
            await SendJsonAsync(webSocket, request, ct).ConfigureAwait(false);
            Assert.AreEqual(expectedCount, _clientSendCount);
            var response = await ReceiveJsonAsync<JsonTest>(webSocket, ct).ConfigureAwait(false);
            Assert.AreEqual(expectedCount + 1, _serverReceiveCount);
            Assert.AreEqual(expectedCount, _serverRecombinedReceiveCount);
            Assert.IsTrue(await IsTrueAsync(() => expectedCount == _serverSendCount, ct: ct).ConfigureAwait(false));
            Assert.AreEqual(expectedCount + 1, _clientReceiveCount);
            Assert.AreEqual(expectedCount, _clientRecombinedReceiveCount);
            Assert.AreEqual(request.Message, response!.Message);
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "OK", ct).ConfigureAwait(false);
            Assert.IsTrue(await IsTrueAsync(() => _serverClosed != null, ct: ct));
            await _serverClosed!;
        }

        [TestMethod]
        public async Task MultipleClientsJsonTest()
        {
            // Arrange
            TestWebSocketServer.Path = "/multiple-clients";
            TestWebSocketServer.DefaultBufferSize = 1024 * 4;
            TestWebSocketServer.WebSocketFunction = Json;
            var numClients = 11;
            var connects = 22;
            var loops = 55;
            var ct = _context.CancellationTokenSource.Token;
            var factory = CreateFactory();
            var clients = Enumerable.Range(1, numClients).Select(_ => factory.Server.CreateWebSocketClient());
            var uri = factory.Server.BaseAddress.ToWs();
            _serverReceiveCount = 0;
            _serverSendCount = 0;
            _serverRecombinedReceiveCount = 0;
            _serverClosed = null;
            _clientReceiveCount = 0;
            _clientSendCount = 0;
            _clientRecombinedReceiveCount = 0;

            // Act and Assert
            await clients.ForEachParallelAsync(async (client, t) =>
            {
                await Enumerable.Range(1, connects).ForEachAsync(async (_, t) =>
                {
                    var webSocket = await client.ConnectAsync(uri, ct).ConfigureAwait(false);
                    await Enumerable.Range(1, loops).ForEachAsync(async (_, t) =>
                    {
                        var request = new JsonTest { Message = GenerateRandomString(_random.Next(1000, 3000)) };
                        await SendJsonAsync(webSocket, request, ct).ConfigureAwait(false);
                        var response = (await ReceiveJsonAsync<JsonTest>(webSocket, ct).ConfigureAwait(false))!;
                        Assert.AreEqual(request.Message, response.Message);
                    }, ct).ConfigureAwait(false);
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "OK", ct).ConfigureAwait(false);
                    Interlocked.Increment(ref _clientCloseCount);
                }, ct).ConfigureAwait(false);
            }, ct).ConfigureAwait(false);

            var expectedCount = numClients * connects * loops;
            Assert.AreEqual(expectedCount, _clientSendCount);
            Assert.AreEqual(expectedCount, _serverReceiveCount);
            Assert.AreEqual(expectedCount, _serverRecombinedReceiveCount);
            Assert.AreEqual(expectedCount, _serverSendCount);
            Assert.AreEqual(expectedCount, _clientReceiveCount);
            Assert.AreEqual(expectedCount, _clientRecombinedReceiveCount);
            Assert.AreEqual(numClients * connects, _clientCloseCount);
            Assert.AreEqual(numClients * connects, _serverCloseCount);
        }

        async Task Echo(WebSocket webSocket, CancellationToken ct = default)
        {
            var receiveLoop = webSocket
                .ReceiveAsyncEnumerable(TestWebSocketServer.DefaultBufferSize, ct)
                .Where(r => r.Result.MessageType != WebSocketMessageType.Close)
                .Select(r => { Interlocked.Increment(ref _serverReceiveCount); return r; })
                .Select(r => r.Buffer);

            await webSocket
                .SendAsyncEnumerable(receiveLoop, WebSocketMessageType.Text, ct)
                .Select(s => { Interlocked.Increment(ref _serverSendCount); return s; })
                .LastAsync(cancellationToken: ct)
                .ConfigureAwait(false);
 
            _serverClosed = webSocket.CloseAsync(webSocket.CloseStatus ?? WebSocketCloseStatus.EndpointUnavailable, webSocket.CloseStatusDescription ?? "Server shutting down", ct);
            await _serverClosed.ConfigureAwait(false);
            Interlocked.Increment(ref _serverCloseCount);
        }

        async Task EchoLarge(WebSocket webSocket, CancellationToken ct = default)
        {
            var receiveLoop = webSocket
                .ReceiveAsyncEnumerable(TestWebSocketServer.DefaultBufferSize, ct)
                .Where(r => r.Result.MessageType != WebSocketMessageType.Close)
                .Select(r => { Interlocked.Increment(ref _serverReceiveCount); return r; })
                .RecombineFragmentsAsync(TestWebSocketServer.DefaultBufferSize, ct)
                .Select(r => { Interlocked.Increment(ref _serverRecombinedReceiveCount); return r; });

            await webSocket
                .SendAsyncEnumerable(receiveLoop, WebSocketMessageType.Text, ct)
                .Select(s => { Interlocked.Increment(ref _serverSendCount); return s; })
                .LastAsync(cancellationToken: ct)
                .ConfigureAwait(false);

            _serverClosed = webSocket.CloseAsync(webSocket.CloseStatus ?? WebSocketCloseStatus.EndpointUnavailable, webSocket.CloseStatusDescription ?? "Server shutting down", ct);
            await _serverClosed.ConfigureAwait(false);
            Interlocked.Increment(ref _serverCloseCount);
        }

        async Task Json(WebSocket webSocket, CancellationToken ct = default)
        {
            var receiveLoop = webSocket
                .ReceiveAsyncEnumerable(TestWebSocketServer.DefaultBufferSize, ct)
                .Where(r => r.Result.MessageType != WebSocketMessageType.Close)
                .Select(r => { Interlocked.Increment(ref _serverReceiveCount); return r; })
                .RecombineFragmentsAsync(TestWebSocketServer.DefaultBufferSize, ct)
                .ExcludeEmpty()
                .ToTFromJsonAsyncEnumerable<JsonTest>()
                .ToJsonBytesAsyncEnumerable()
                .ToStringAsyncEnumerable()
                .ToBytesAsyncEnumerable()
                .ToTFromJsonAsyncEnumerable<JsonTest>()
                .ToJsonStringAsyncEnumerable()
                .Select(s => { Interlocked.Increment(ref _serverRecombinedReceiveCount); return s; })
                .ToBytesAsyncEnumerable();

            await webSocket
                .SendAsyncEnumerable(receiveLoop, WebSocketMessageType.Text, ct)
                .Select(s => { Interlocked.Increment(ref _serverSendCount); return s; })
                .LastAsync(cancellationToken: ct)
                .ConfigureAwait(false);
 
            _serverClosed = webSocket.CloseAsync(webSocket.CloseStatus ?? WebSocketCloseStatus.EndpointUnavailable, webSocket.CloseStatusDescription ?? "Server shutting down", ct);
            await _serverClosed.ConfigureAwait(false);
            Interlocked.Increment(ref _serverCloseCount);
        }

        async ValueTask SendStringAsync(WebSocket webSocket, string message, CancellationToken ct = default)
        {
            var buffer = new Memory<byte>(Encoding.UTF8.GetBytes(message));
            await webSocket.SendAsync(buffer, WebSocketMessageType.Text, WebSocketMessageFlags.EndOfMessage, ct);
            Interlocked.Increment(ref _clientSendCount);
        }

        async ValueTask SendJsonAsync<T>(WebSocket webSocket, T message, CancellationToken ct = default)
        {
            var buffer = new Memory<byte>(JsonSerializer.SerializeToUtf8Bytes(message));
            await webSocket.SendAsync(buffer, WebSocketMessageType.Text, WebSocketMessageFlags.EndOfMessage, ct);
            Interlocked.Increment(ref _clientSendCount);
        }

        async Task<string> ReceiveStringAsync(WebSocket webSocket, CancellationToken ct = default)
        {
            var buffer = new Memory<byte>(new byte[TestWebSocketServer.DefaultBufferSize]);
            var message = new StringBuilder();
            ValueWebSocketReceiveResult result;
            do
            {
                result = await webSocket.ReceiveAsync(buffer, ct).ConfigureAwait(false);
                Interlocked.Increment(ref _clientReceiveCount);
                if (result.Count > 0)
                    message.Append(Encoding.UTF8.GetString(buffer[..result.Count].ToArray()));
            } while (!result.EndOfMessage);

            Interlocked.Increment(ref _clientRecombinedReceiveCount);
            return message.ToString();
        }

        async Task<T?> ReceiveJsonAsync<T>(WebSocket webSocket, CancellationToken ct = default)
        {
            var buffer = new Memory<byte>(new byte[TestWebSocketServer.DefaultBufferSize]);
            var message = new StringBuilder();
            ValueWebSocketReceiveResult result;
            do
            {
                result = await webSocket.ReceiveAsync(buffer, ct).ConfigureAwait(false);
                Interlocked.Increment(ref _clientReceiveCount);
                if (result.Count > 0)
                    message.Append(Encoding.UTF8.GetString(buffer[..result.Count].ToArray()));
            } while (!result.EndOfMessage);

            Interlocked.Increment(ref _clientRecombinedReceiveCount);
            return JsonSerializer.Deserialize<T>(message.ToString());
        }

        private static WebApplicationFactory<TestWebSocketServer> CreateFactory()
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

        private static string GenerateRandomString(int size)
        {
            var stringBuilder = new StringBuilder(size);

            for (int i = 0; i < size; i++)
            {
                // Get a random character from the Chars string
                char randomChar = _chars[_random.Next(_chars.Length)];
                stringBuilder.Append(randomChar);
            }

            return stringBuilder.ToString();
        }

        // TODO: This is HORRIBLE and should be removed IMMEDIATELY!
        // This WILL lead to flakey tests and should be replaced with an alternative way of doing things.
        private static async Task<bool> IsTrueAsync(Func<bool> func, int timeout = 101, CancellationToken ct = default)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromMilliseconds(timeout));
            while (!cts.IsCancellationRequested)
            {
                if (func()) return true;
                await Task.Delay(9).ConfigureAwait(false);
            }

            //cts.Token.ThrowIfCancellationRequested();
            return false;
        }   
    }

    public class JsonTest
    {
        public required string Message { get; set; }
    }
}
