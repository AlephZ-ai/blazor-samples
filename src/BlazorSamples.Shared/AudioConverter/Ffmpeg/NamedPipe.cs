using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.AudioConverter.Ffmpeg;

internal sealed class NamedPipe : IAsyncDisposable, IDisposable
{
    private const string SERVER = ".";
    public string Name { get; }
    public NamedPipeServerStream Server { get; }
    public NamedPipeClientStream Client { get; }
    private readonly MemoryPool<byte> _pool = MemoryPool<byte>.Shared;
    private readonly int _initialBufferSize;
    private int _connected = 0;
    private int _disposedValue = 0;

    public NamedPipe(string? name = null, int initialBufferSize = 4 * 1024)
    {
        Name = name ?? Guid.NewGuid().ToString();
        _initialBufferSize = initialBufferSize;
        Server = new(Name, PipeDirection.Out, 1, PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous | PipeOptions.WriteThrough);
        Client = new(SERVER, Name, PipeDirection.In, PipeOptions.Asynchronous | PipeOptions.WriteThrough);
    }

    public async IAsyncEnumerable<ReadOnlyMemory<byte>> ReadAllAsync(
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await EnsureConnectedAsync(ct).ConfigureAwait(false);
        using var owner = _pool.Rent(_initialBufferSize);
        var buffer = owner.Memory;
        int read;
        while (!ct.IsCancellationRequested && (read = await Client.ReadAsync(buffer, ct).ConfigureAwait(false)) > 0)
        {
            yield return buffer[..read];
        }
    }

    public async Task WriteAllAsync(IAsyncEnumerable<ReadOnlyMemory<byte>> source, CancellationToken ct = default)
    {
        await EnsureConnectedAsync(ct).ConfigureAwait(false);
        try
        {
            await foreach (var buffer in source.WithCancellation(ct).ConfigureAwait(false))
            {
                await Server.WriteAsync(buffer, ct).ConfigureAwait(false);
                if (ct.IsCancellationRequested) break;
            }
        }
        finally
        {
            Server.Disconnect();
        }
    }

    private async Task EnsureConnectedAsync(CancellationToken ct)
    {
        if (Interlocked.CompareExchange(ref _connected, 1, 0) == 0)
        {
            var serverConnect = Server.WaitForConnectionAsync(ct);
            var clientConnect = Client.ConnectAsync(ct);
            await Task.WhenAll(serverConnect, clientConnect).ConfigureAwait(false);
        }
    }

    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _disposedValue, 1, 0) != 0) return;
        Client.Dispose();
        Server.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _disposedValue, 1, 0) != 0) return;
        await Client.DisposeAsync().ConfigureAwait(false);
        await Server.DisposeAsync().ConfigureAwait(false);
    }
}