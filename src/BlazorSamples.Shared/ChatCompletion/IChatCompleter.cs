namespace BlazorSamples.Shared.ChatCompletion;

public interface IChatCompleter
{
    public IAsyncEnumerable<string> CompleteAsync(IAsyncEnumerable<string> source, CancellationToken ct = default);
}
