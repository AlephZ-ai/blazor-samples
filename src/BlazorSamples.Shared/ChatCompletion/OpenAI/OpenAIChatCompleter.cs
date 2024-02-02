using System.Runtime.CompilerServices;
using Azure.AI.OpenAI;

namespace BlazorSamples.Shared.ChatCompletion.OpenAI;

public sealed class OpenAIChatCompleter(OpenAIChatCompleterOptions options, OpenAIClient openAI) : IChatCompleter
{
    public IAsyncEnumerable<string> CompleteAsync(IAsyncEnumerable<string> source, CancellationToken ct = default)
    {
        return source.SelectManyAwait<string, string>(async (query) =>
        {
            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = options.Model,
                Messages =
                {
                    new ChatRequestSystemMessage(options.SystemMessage),
                    new ChatRequestUserMessage(query),
                }
            };

            return (await openAI.GetChatCompletionsStreamingAsync(chatCompletionsOptions, ct))
                .Where(_ => !ct.IsCancellationRequested)
                .Select(completion => completion.ContentUpdate);
        });
    }
}
