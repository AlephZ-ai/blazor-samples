namespace BlazorSamples.Shared.TextToSpeech;

public interface ITextToSpeechGenerator
{
    public IAsyncEnumerable<ReadOnlyMemory<byte>> GenerateAsync(IAsyncEnumerable<string> source, CancellationToken ct = default);
}
