using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.AudioConverter
{
    public interface IAudioConverter
    {
        public IAsyncEnumerable<ReadOnlyMemory<byte>> ConvertAsync(IAsyncEnumerable<ReadOnlyMemory<byte>> source, CancellationToken ct = default);
    }
}
