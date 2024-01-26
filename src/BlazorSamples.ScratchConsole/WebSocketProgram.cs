using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSamples.ScratchConsole
{
    public class WebSocketProgram
    {
        public static void Main(string[] args)
        {
            MainAsync().Wait();
        }

        private static async Task MainAsync()
        {
            var client = new ClientWebSocket();
            try
            {
                // Adjust the URI to the appropriate WebSocket server address
                await client.ConnectAsync(new Uri("wss://localhost:7006/stream"), CancellationToken.None);
                Console.WriteLine("Connected!");

                // Send a greeting message
                await Send(client, "Hello from the client!");

                // Receive messages in a loop
                await Receive(client);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebSocket error: {ex.Message}");
            }
            finally
            {
                client?.Dispose();
            }

            Console.ReadKey();
        }

        static async Task Send(ClientWebSocket client, string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            await client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        static async Task Receive(ClientWebSocket client)
        {
            var buffer = new byte[1024 * 4];
            while (client.State == WebSocketState.Open)
            {
                var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                else
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Message from server: {message}");

                    // Echoing the message back to the server
                    await Send(client, $"Echo: {message}");
                }
            }
        }
    }
}
