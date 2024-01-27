using Azure.AI.OpenAI;
using BlazorSamples.Shared.Clients;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Vosk;

namespace BlazorSamples.Shared.TextToText
{
    public class OpenAITextToText(OpenAIClient openAI) : ITextToText
    {
        public const string AIModel = "gpt-4-turbo-preview";
        //public const string AIModel = "gpt-3.5-turbo-1106";
        public async IAsyncEnumerable<string> StreamingResponse(string query, [EnumeratorCancellation] CancellationToken ct = default)
        {
            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = AIModel,
                Messages =
                {
                    new ChatRequestSystemMessage("Respond with about 2 to 3 sentences and you are a helpful audio assistant that talks on the phone now, I've added speech capabilities to you.  If you really need more sentence it's fine but try not to use less than 2 unless asked.  Always end your sentence with either . ? ! and no other symbols.  I'm using that to detect when you've started a new sentence in streaming mode.  Please refrain from using . ? ! anywhere else in your output you can omit it from things like Dr. and just put Dr without any character that would make me think end of sentence."),
                    new ChatRequestUserMessage(query),
                }
            };

            var sentence = string.Empty;
            var responseStream = await openAI.GetChatCompletionsStreamingAsync(chatCompletionsOptions, ct).ConfigureAwait(false);
            await foreach (var response in responseStream.WithCancellation(ct).ConfigureAwait(false))
            {
                sentence += response?.ContentUpdate ?? string.Empty;
                if (DetectSentence(ref sentence, out var leftOver))
                {
                    yield return sentence;
                    sentence = leftOver;
                }
            }
        }

        private bool DetectSentence(ref string sentence, out string leftOver)
        {
            if (sentence.Contains(".") || sentence.Contains("!") || sentence.Contains("?"))
            {
                var split = sentence.Split(new[] { '.', '!', '?' }, 2);
                sentence = split[0];
                leftOver = split[1] ?? string.Empty;
                return true;
            }

            leftOver = string.Empty;
            return false;
        }
    }
}
