using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSamples.Shared
{
    public interface IAudioConverter
    {
        public Task InitializationAsync(Func<byte[], Task> processConvertedAudioBuffer);
        public Task ProcessAudioBuffer(byte[] buffer);
        public Task ClosePipes();
    }
}
