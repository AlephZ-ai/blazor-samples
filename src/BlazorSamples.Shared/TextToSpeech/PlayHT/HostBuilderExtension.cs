using BlazorSamples.Shared.TextToSpeech;
using BlazorSamples.Shared.TextToSpeech.PlayHT;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

public static partial class HostBuilderExtension
{
    public static IHostApplicationBuilder AddPlayHTTextToSpeechGenerator(this IHostApplicationBuilder builder, Func<IConfiguration, PlayHTTextToSpeechGeneratorOptions> configureOptions)
    {
        builder.Services.AddTransient(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var options = configureOptions(configuration);
            return options;
        });

        builder.Services.AddTransient<ITextToSpeechGenerator, PlayHTTextToSpeechGenerator>();
        return builder;
    }
}
