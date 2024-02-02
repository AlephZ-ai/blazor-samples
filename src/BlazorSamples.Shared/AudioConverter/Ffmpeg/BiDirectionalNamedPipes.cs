using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.AudioConverter.Ffmpeg
{
    internal sealed class BiDirectionalNamedPipes(string? inPipeName = null, string? outPipeName = null, int initialBufferSize = 4 * 1024) : IAsyncDisposable, IDisposable
    {
        public NamedPipe In { get; } = new NamedPipe(inPipeName, initialBufferSize);
        public NamedPipe Out { get; } = new NamedPipe(outPipeName, initialBufferSize);
        public string InPipeName => In.Name;
        public string OutPipeName => Out.Name;
        private int _disposedValue = 0;

        public IAsyncEnumerable<ReadOnlyMemory<byte>> ProcessAllAsync(IAsyncEnumerable<ReadOnlyMemory<byte>> source, CancellationToken ct = default)
        {
            var readAll = Out.ReadAllAsync(ct);
            var writeAll = In.WriteAllAsync(source, ct);
            return readAll.Finally(() => writeAll.AsTask());
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposedValue, 1, 0) == 0)
            {
                In.Dispose();
                Out.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _disposedValue, 1, 0) == 0)
            {
                await In.DisposeAsync();
                await Out.DisposeAsync();
            }

            GC.SuppressFinalize(this);
        }
    }
}
