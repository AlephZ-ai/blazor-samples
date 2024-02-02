using BlazorSamples.Shared.SpeechRecognition;

namespace System.Collections.Generic
{
    public static partial class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<SpeechRecognitionResult> RecognizeSpeechAsync(this IAsyncEnumerable<ReadOnlyMemory<byte>> source, ISpeechRecognizer recognizer, CancellationToken ct = default)
        {
            return recognizer.RecognizeAsync(source, ct);
        }
    }
}
