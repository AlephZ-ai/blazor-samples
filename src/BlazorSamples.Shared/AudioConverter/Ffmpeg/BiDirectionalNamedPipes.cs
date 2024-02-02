using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.AudioConverter.Ffmpeg;

internal sealed class BiDirectionalNamedPipes(
    string? inPipeName = null,
    string? outPipeName = null,
    int initialBufferSize = 4 * 1024) : IAsyncDisposable, IDisposable
{
    public NamedPipe In { get; } = new NamedPipe(inPipeName, initialBufferSize);
    public NamedPipe Out { get; } = new NamedPipe(outPipeName, initialBufferSize);
    public string InPipeName => In.Name;
    public string OutPipeName => Out.Name;
    private int _disposedValue = 0;

    public IAsyncEnumerable<ReadOnlyMemory<byte>> ProcessAllAsync(IAsyncEnumerable<ReadOnlyMemory<byte>> source,
        Task<bool> foreign, CancellationToken ct = default)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        ct = cts.Token;
        var cancel = CancelOnFailure(foreign, cts);
        var readAll = Out.ReadAllAsync(ct);
        var writeAll = DisconnectAfter(In.WriteAllAsync(source, ct), cancel);
        return readAll
            .Finally(() => writeAll);
    }

    private async Task CancelOnFailure(Task<bool> foreignTask, CancellationTokenSource cts)
    {
        try
        {
            if (!await foreignTask.ConfigureAwait(false))
                await cts.CancelAsync();
        }
        catch
        {
            await cts.CancelAsync();
            throw;
        }
    }

    private async Task DisconnectAfter(Task previous, Task foreign)
    {
        try
        {
            await Task.WhenAll(previous, foreign).ConfigureAwait(false);
        }
        finally
        {
            Out.Server.Disconnect();
        }
    }

    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _disposedValue, 1, 0) != 0) return;
        In.Dispose();
        Out.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.CompareExchange(ref _disposedValue, 1, 0) != 0) return;
        await In.DisposeAsync();
        await Out.DisposeAsync();
    }
}
