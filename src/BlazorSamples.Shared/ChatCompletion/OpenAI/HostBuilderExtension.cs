using BlazorSamples.Shared.ChatCompletion;
using BlazorSamples.Shared.ChatCompletion.OpenAI;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting
{
    public static partial class HostBuilderExtension
    {
        public static IHostApplicationBuilder AddOpenAIChatCompleter(this IHostApplicationBuilder builder, OpenAIChatCompleterOptions options)
        {
            builder.Services.AddSingleton(options);
            builder.Services.AddSingleton<IChatCompleter, OpenAIChatCompleter>();
            return builder;
        }
    }
}
