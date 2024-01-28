using System.Buffers;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using WebSocketAsyncEnumerableReceiveResult = (System.Net.WebSockets.ValueWebSocketReceiveResult Result, System.Memory<byte> Buffer);

namespace BlazorSamples.Ws2
{
    public static class Extensions
    {
        public const int DefaultBufferSize = 4 * 1024;
        public static async IAsyncEnumerable<WebSocketAsyncEnumerableReceiveResult> ReceiveAsyncEnumerable(
            this WebSocket webSocket,
            int bufferSize,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
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

        public static IAsyncEnumerable<WebSocketAsyncEnumerableReceiveResult> ReceiveAsyncEnumerable(
            this WebSocket webSocket,
            CancellationToken ct = default) =>
        webSocket.ReceiveAsyncEnumerable(DefaultBufferSize, ct);

        public static async IAsyncEnumerable<ValueTask> SendAsyncEnumerable(
            this WebSocket webSocket,
            IAsyncEnumerable<Memory<byte>> enumerable,
            WebSocketMessageType messageType = WebSocketMessageType.Text,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            await foreach (var buffer in enumerable.WithCancellation(ct).ConfigureAwait(false))
            {
                if (webSocket.State != WebSocketState.Open) break;
                yield return webSocket.SendAsync(buffer, messageType, false, ct);
            }
        }

        public static IAsyncEnumerable<ValueTask> SendAsyncEnumerable(
            this WebSocket webSocket,
            IAsyncEnumerable<Memory<byte>> enumerable,
            CancellationToken ct = default) =>
        webSocket.SendAsyncEnumerable(enumerable, WebSocketMessageType.Text, ct);
    }
}
