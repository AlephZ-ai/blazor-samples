namespace BlazorSamples.Shared.Old.TextToText
{
    public interface ITextToText
    {
        public IAsyncEnumerable<string> StreamingResponse(string query, CancellationToken ct = default);
    }
}
