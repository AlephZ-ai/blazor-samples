using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Text;
using BlazorSamples.Shared;
using Microsoft.Extensions.Logging;
using WebSocketAsyncEnumerableReceiveMessage = (System.Net.WebSockets.ValueWebSocketReceiveResult Result, System.ReadOnlyMemory<byte> Buffer);
using WebSocketAsyncEnumerableSendMessage = (System.Net.WebSockets.WebSocketMessageFlags Flags, System.ReadOnlyMemory<byte> Buffer);

namespace System.Net.WebSockets
{
    public static class WebSocketExtensions
    {
        public const int DefaultBufferSize = AsyncEnumerableExtensions.DefaultBufferSize;
        public static async IAsyncEnumerable<WebSocketAsyncEnumerableReceiveMessage> ReadAllAsync(
            this WebSocket webSocket,
            int bufferSize = DefaultBufferSize,
            ILogger? log = null,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            log?.LogEnter();
            if (webSocket.State != WebSocketState.Open) yield break;
            MemoryPool<byte> pool = MemoryPool<byte>.Shared;
            using IMemoryOwner<byte> owner = pool.Rent(bufferSize);
            Memory<byte> buffer = owner.Memory;
            ValueWebSocketReceiveResult result = default;
            while (webSocket.State == WebSocketState.Open && result.MessageType != WebSocketMessageType.Close)
            {
                log?.LogAwait();
                result = await webSocket.ReceiveAsync(buffer, ct).ConfigureAwait(false);
                log?.LogReceive();
                yield return (result, buffer[..result.Count]);
            }

            log?.LogExit();
        }

        public static IAsyncEnumerable<WebSocketAsyncEnumerableReceiveMessage> ReadAllAsync(
            this WebSocket webSocket,
            ILogger? log = null,
            CancellationToken ct = default) =>
        webSocket.ReadAllAsync(DefaultBufferSize, log, ct);

        public static async IAsyncEnumerable<Unit> SendAllAsync(
            this WebSocket webSocket,
            IAsyncEnumerable<WebSocketAsyncEnumerableSendMessage> source,
            WebSocketMessageType messageType = WebSocketMessageType.Text,
            ILogger? log = null,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            log?.LogEnter();
            await foreach (var message in source.WithCancellation(ct).ConfigureAwait(false))
            {
                if (webSocket.State != WebSocketState.Open) break;
                log?.LogAwait();
                await webSocket.SendAsync(message.Buffer, messageType, message.Flags, ct).ConfigureAwait(false);
                log?.LogReceive();
                yield return Unit.Default;
            }
            
            log?.LogExit();
        }

        public static IAsyncEnumerable<Unit> SendAllAsync(
            this WebSocket webSocket,
            IAsyncEnumerable<WebSocketAsyncEnumerableSendMessage> source,
            ILogger? log = null,
            CancellationToken ct = default) =>
        webSocket.SendAllAsync(source, WebSocketMessageType.Text, log, ct);

        public static IAsyncEnumerable<Unit> SendAllAsync(
            this WebSocket webSocket,
            IAsyncEnumerable<ReadOnlyMemory<byte>> source,
            WebSocketMessageType messageType = WebSocketMessageType.Text,
            ILogger? log = null,
            CancellationToken ct = default) =>
        webSocket.SendAllAsync(source.Select(buffer => (WebSocketMessageFlags.EndOfMessage, buffer)), messageType, log, ct);

        public static IAsyncEnumerable<Unit> SendAllAsync(
            this WebSocket webSocket,
            IAsyncEnumerable<ReadOnlyMemory<byte>> source,
            ILogger? log = null,
            CancellationToken ct = default) =>
        webSocket.SendAllAsync(source, WebSocketMessageType.Text, log, ct);

        public static async IAsyncEnumerable<ReadOnlyMemory<byte>> RecombineFragmentsAsync(
            this IAsyncEnumerable<WebSocketAsyncEnumerableReceiveMessage> source,
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
                await foreach (var message in source.WithCancellation(ct).ConfigureAwait(false))
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
