using Google.Protobuf.WellKnownTypes;
using System.Buffers;
using System.Net.WebSockets;
using System.Reactive;
using System.Runtime.CompilerServices;
using WebSocketAsyncEnumerableReceiveMessage = (System.Net.WebSockets.ValueWebSocketReceiveResult Result, System.ReadOnlyMemory<byte> Buffer);
using WebSocketAsyncEnumerableSendMessage = (System.Net.WebSockets.WebSocketMessageFlags Flags, System.ReadOnlyMemory<byte> Buffer);

namespace BlazorSamples.Ws2
{
    public static class Extensions
    {
        public const int DefaultBufferSize = 4 * 1024;
        public static async IAsyncEnumerable<WebSocketAsyncEnumerableReceiveMessage> ReceiveAsyncEnumerable(
            this WebSocket webSocket,
            int bufferSize,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            if (webSocket.State != WebSocketState.Open) yield break;
            MemoryPool<byte> pool = MemoryPool<byte>.Shared;
            IMemoryOwner<byte> owner = pool.Rent(bufferSize);
            Memory<byte> buffer = owner.Memory;
            try
            {
                ValueWebSocketReceiveResult result;
                while (webSocket.State == WebSocketState.Open)
                {
                    result = await webSocket.ReceiveAsync(buffer, ct).ConfigureAwait(false);
                    yield return (result, buffer[..result.Count]);
                    if (result.MessageType == WebSocketMessageType.Close) break;
                }
            }
            finally
            {
                owner.Dispose();
            }
        }

        public static IAsyncEnumerable<WebSocketAsyncEnumerableReceiveMessage> ReceiveAsyncEnumerable(
            this WebSocket webSocket,
            CancellationToken ct = default) =>
        webSocket.ReceiveAsyncEnumerable(DefaultBufferSize, ct);

        public static async IAsyncEnumerable<Unit> SendAsyncEnumerable(
            this WebSocket webSocket,
            IAsyncEnumerable<WebSocketAsyncEnumerableSendMessage> enumerable,
            WebSocketMessageType messageType = WebSocketMessageType.Text,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            await foreach (var message in enumerable.WithCancellation(ct).ConfigureAwait(false))
            {
                if (webSocket.State != WebSocketState.Open) break;
                await webSocket.SendAsync(message.Buffer, messageType, message.Flags, ct);
                yield return Unit.Default;
            }
        }

        public static IAsyncEnumerable<Unit> SendAsyncEnumerable(
            this WebSocket webSocket,
            IAsyncEnumerable<WebSocketAsyncEnumerableSendMessage> enumerable,
            CancellationToken ct = default) =>
        webSocket.SendAsyncEnumerable(enumerable, WebSocketMessageType.Text, ct);

        public static IAsyncEnumerable<Unit> SendAsyncEnumerable(
            this WebSocket webSocket,
            IAsyncEnumerable<ReadOnlyMemory<byte>> enumerable,
            WebSocketMessageType messageType = WebSocketMessageType.Text,
            CancellationToken ct = default)
        {
            return SendAsyncEnumerable(webSocket, enumerable.Select(buffer => (WebSocketMessageFlags.EndOfMessage, buffer)), messageType, ct);
        }

        public static IAsyncEnumerable<Unit> SendAsyncEnumerable(
            this WebSocket webSocket,
            IAsyncEnumerable<ReadOnlyMemory<byte>> enumerable,
            CancellationToken ct = default) =>
        webSocket.SendAsyncEnumerable(enumerable, WebSocketMessageType.Text, ct);
    }
}
