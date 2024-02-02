namespace BlazorSamples.Shared.Old.TextToSpeech
{
    public interface ITextToSpeech
    {
        IAsyncEnumerable<byte[]> Voice(string text, CancellationToken ct = default);
    }
}
