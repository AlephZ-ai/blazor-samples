using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.SpeechRecognition
{
    public interface ISpeechRecognizer
    {
        public IAsyncEnumerable<SpeechRecognitionResult> RecognizeAsync(IAsyncEnumerable<ReadOnlyMemory<byte>> source, CancellationToken ct = default);
    }
}
