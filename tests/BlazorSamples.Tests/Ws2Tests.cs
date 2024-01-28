using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestPlatform.TestHost;

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
            var webSocket = await _client.ConnectAsync(_uri, _context.CancellationTokenSource.Token);
        }
    }
}
