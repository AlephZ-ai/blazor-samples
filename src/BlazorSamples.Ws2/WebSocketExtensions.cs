using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Text;
using WebSocketAsyncEnumerableReceiveMessage = (System.Net.WebSockets.ValueWebSocketReceiveResult Result, System.ReadOnlyMemory<byte> Buffer);
using WebSocketAsyncEnumerableSendMessage = (System.Net.WebSockets.WebSocketMessageFlags Flags, System.ReadOnlyMemory<byte> Buffer);

namespace BlazorSamples.Ws2
{
    public static class WebSocketExtensions
    {
        public const int DefaultBufferSize = AsyncEnumerableExtensions.DefaultBufferSize;
        public static async IAsyncEnumerable<WebSocketAsyncEnumerableReceiveMessage> ReceiveAsyncEnumerable(
            this WebSocket webSocket,
            int bufferSize = DefaultBufferSize,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            if (webSocket.State != WebSocketState.Open) yield break;
            MemoryPool<byte> pool = MemoryPool<byte>.Shared;
            using IMemoryOwner<byte> owner = pool.Rent(bufferSize);
            Memory<byte> buffer = owner.Memory;
            ValueWebSocketReceiveResult result = default;
            while (webSocket.State == WebSocketState.Open && result.MessageType != WebSocketMessageType.Close)
            {
                result = await webSocket.ReceiveAsync(buffer, ct).ConfigureAwait(false);
                yield return (result, buffer[..result.Count]);
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
                await webSocket.SendAsync(message.Buffer, messageType, message.Flags, ct).ConfigureAwait(false);
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
            CancellationToken ct = default) =>
        webSocket.SendAsyncEnumerable(enumerable.Select(buffer => (WebSocketMessageFlags.EndOfMessage, buffer)), messageType, ct);

        public static IAsyncEnumerable<Unit> SendAsyncEnumerable(
            this WebSocket webSocket,
            IAsyncEnumerable<ReadOnlyMemory<byte>> enumerable,
            CancellationToken ct = default) =>
        webSocket.SendAsyncEnumerable(enumerable, WebSocketMessageType.Text, ct);

        public static async IAsyncEnumerable<ReadOnlyMemory<byte>> RecombineFragmentsAsync(
            this IAsyncEnumerable<WebSocketAsyncEnumerableReceiveMessage> enumerable,
            int initialBufferSize = DefaultBufferSize,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            // TODO: This is a naive implementation that will rent a new even if no resize is needed.
            // Only rent a new buffer on first need, when a message is larger than the current buffer.
            // Set a maximum buffer size and throw an exception if the message is larger than the maximum to avoid a DOS attack.
            MemoryPool<byte> pool = MemoryPool<byte>.Shared;
            IMemoryOwner<byte> owner = pool.Rent(initialBufferSize);
            Memory<byte> buffer = owner.Memory;
            var offset = 0;
            try
            {
                await foreach (var message in enumerable.WithCancellation(ct).ConfigureAwait(false))
                {
                    AsyncEnumerableExtensions.ResizeBufferIfNeeded(pool, ref owner, ref buffer, offset, message.Buffer.Length);
                    message.Buffer.CopyTo(buffer[offset..]);
                    offset += message.Buffer.Length;
                    if (message.Result.EndOfMessage)
                    {
                        yield return buffer[..offset];
                        offset = 0;
                    }
                }
            }
            finally
            {
                owner.Dispose();
            }
        }
    }
}
