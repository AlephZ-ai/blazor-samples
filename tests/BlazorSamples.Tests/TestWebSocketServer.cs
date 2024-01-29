﻿using BlazorSamples.Ws2;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSamples.Tests
{
    public class TestWebSocketServer
    {
        public static Func<WebSocket, CancellationToken, Task> WebSocketFunction { get; set; } = Echo;
        public static int DefaultBufferSize { get; set; } = 4 * 1024;
        public static string Path { get; set; } = "/";
        public static Task? AppCompletion { get; private set; }
        public static WebApplication? App { get; private set; }
        public static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        public static async Task MainAsync(string[] args)
        {
            WebApplicationBuilder? builder;
            try
            {
                builder = WebApplication.CreateBuilder(args);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }

            App = builder.Build();
            App.UseWebSockets();
            App.MapGet(Path, async (
                HttpContext context,
                CancellationToken ct) =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    using var webSocket = await context.WebSockets.AcceptWebSocketAsync().ConfigureAwait(false);
                    await WebSocketFunction(webSocket, ct).ConfigureAwait(false);
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                }
            });

            AppCompletion = App.RunAsync();
            await AppCompletion.ConfigureAwait(false);
        }

        private static async Task Echo(WebSocket webSocket, CancellationToken ct)
        {
            var buffer = new byte[DefaultBufferSize];
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
                ct);
        }
    }
}
