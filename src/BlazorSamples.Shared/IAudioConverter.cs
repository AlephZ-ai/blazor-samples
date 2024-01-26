using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSamples.Shared
{
    public interface IAudioConverter
    {
        public Task InitializationAsync();
        public IAsyncEnumerable<byte[]> ProcessAudioBuffer(byte[] buffer);
        public Task ClosePipes();
    }
}
