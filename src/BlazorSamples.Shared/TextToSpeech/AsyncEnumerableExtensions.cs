using BlazorSamples.Shared.TextToSpeech;

namespace System.Collections.Generic;

public static partial class AsyncEnumerableExtensions
{
    public static IAsyncEnumerable<ReadOnlyMemory<byte>> GenerateSpeechAsync(
        this IAsyncEnumerable<string> source, ITextToSpeechGenerator generator, CancellationToken ct = default)
    {
        return generator.GenerateAsync(source, ct);
    }
}
