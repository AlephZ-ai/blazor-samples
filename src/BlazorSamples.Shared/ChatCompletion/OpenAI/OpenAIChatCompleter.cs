namespace BlazorSamples.Shared.ChatCompletion.OpenAI;

public sealed class OpenAIChatCompleter(OpenAIChatCompleterOptions options) : IChatCompleter
{
    public IAsyncEnumerable<string> CompleteAsync(IAsyncEnumerable<string> source, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
