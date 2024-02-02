using BlazorSamples.Shared.ChatCompletion;

namespace System.Collections.Generic;

public static partial class AsyncEnumerableExtensions
{
    public static IAsyncEnumerable<string> CompleteChatAsync(
        this IAsyncEnumerable<string> source, IChatCompleter completer, CancellationToken ct = default)
    {
        return completer.CompleteAsync(source, ct);
    }
}
