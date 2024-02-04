using System.Buffers;
using System.Reactive;
using System.Runtime.CompilerServices;
using BlazorSamples.Shared;
using Microsoft.Extensions.Logging;
using WebSocketAsyncEnumerableReceiveMessage = (System.Net.WebSockets.ValueWebSocketReceiveResult Result, System.ReadOnlyMemory<byte> Buffer);
using WebSocketAsyncEnumerableSendMessage = (System.Net.WebSockets.WebSocketMessageFlags Flags, System.ReadOnlyMemory<byte> Buffer);

namespace System.Net.WebSockets;

public static class WebSocketExtensions
{
    public const int DefaultBufferSize = AsyncEnumerableExtensions.DefaultBufferSize;

    public static async IAsyncEnumerable<WebSocketAsyncEnumerableReceiveMessage> ReadAllAsync(
        this WebSocket webSocket,
        ILogger log,
        int bufferSize = DefaultBufferSize,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        log.LogEnter();
        try
        {
            if (webSocket.State != WebSocketState.Open) yield break;
            MemoryPool<byte> pool = MemoryPool<byte>.Shared;
            using IMemoryOwner<byte> owner = pool.Rent(bufferSize);
            Memory<byte> buffer = owner.Memory;
            ValueWebSocketReceiveResult result = default;
            while (webSocket.State == WebSocketState.Open && result.MessageType != WebSocketMessageType.Close)
            {
                log.LogLoop();
                try
                {
                    log.LogAwait();
                    result = await webSocket.ReceiveAsync(buffer, ct).ConfigureAwait(false);
                    log.LogReceived();
                }
                catch (Exception ex)
                {
                    log.LogException(ex);
                    throw;
                }

                log.LogYield();
                yield return (result, buffer[..result.Count]);
            }
        }
        finally
        {
            log.LogExit();
        }
    }

    public static IAsyncEnumerable<WebSocketAsyncEnumerableReceiveMessage> ReadAllAsync(
        this WebSocket webSocket,
        ILogger log,
        CancellationToken ct = default) =>
        webSocket.ReadAllAsync(log, DefaultBufferSize, ct);

    public static async IAsyncEnumerable<Unit> SendAllAsync(
        this WebSocket webSocket,
        IAsyncEnumerable<WebSocketAsyncEnumerableSendMessage> source,
        ILogger log,
        WebSocketMessageType messageType = WebSocketMessageType.Text,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        log.LogEnter();
        try
        {
            await foreach (var message in source.WithCancellation(ct).ConfigureAwait(false))
            {
                try
                {
                    log.LogLoop();
                    if (webSocket.State != WebSocketState.Open) break;
                    log.LogAwait();
                    await webSocket.SendAsync(message.Buffer, messageType, message.Flags, ct).ConfigureAwait(false);
                    log.LogSent();
                }
                catch (Exception ex)
                {
                    log.LogException(ex);
                    throw;
                }

                log.LogYield();
                yield return Unit.Default;
            }
        }
        finally
        {
            log.LogExit();
        }
    }

    public static IAsyncEnumerable<Unit> SendAllAsync(
        this WebSocket webSocket,
        IAsyncEnumerable<WebSocketAsyncEnumerableSendMessage> source,
        ILogger log,
        CancellationToken ct = default) =>
        webSocket.SendAllAsync(source, log, WebSocketMessageType.Text, ct);

    public static IAsyncEnumerable<Unit> SendAllAsync(
        this WebSocket webSocket,
        IAsyncEnumerable<ReadOnlyMemory<byte>> source,
        ILogger log,
        WebSocketMessageType messageType = WebSocketMessageType.Text,
        CancellationToken ct = default) =>
        webSocket.SendAllAsync(source.Select(buffer => (WebSocketMessageFlags.EndOfMessage, buffer)), log, messageType, ct);

    public static IAsyncEnumerable<Unit> SendAllAsync(
        this WebSocket webSocket,
        IAsyncEnumerable<ReadOnlyMemory<byte>> source,
        ILogger log,
        CancellationToken ct = default) =>
        webSocket.SendAllAsync(source, log, WebSocketMessageType.Text, ct);

    public static async IAsyncEnumerable<ReadOnlyMemory<byte>> RecombineFragmentsAsync(
        this IAsyncEnumerable<WebSocketAsyncEnumerableReceiveMessage> source,
        ILogger log,
        int initialBufferSize = DefaultBufferSize,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        log.LogEnter();
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
                log.LogLoop();
                AsyncEnumerableExtensions.ResizeBufferIfNeeded(pool, ref owner, ref buffer, offset, message.Buffer.Length, log);
                message.Buffer.CopyTo(buffer[offset..]);
                offset += message.Buffer.Length;
                if (message.Result.EndOfMessage)
                {
                    log.LogYield();
                    yield return buffer[..offset];
                    offset = 0;
                }
            }
        }
        finally
        {
            owner.Dispose();
            log.LogExit();
        }
    }
}
