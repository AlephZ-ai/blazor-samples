using BlazorSamples.Shared.AudioConverter;
using BlazorSamples.Shared.AudioConverter.Ffmpeg;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Hosting
{
    public static partial class HostBuilderExtension
    {
        public static IHostApplicationBuilder AddFfmpegAudioConverter(this IHostApplicationBuilder builder, FfmpegAudioConverterOptions options)
        {
            builder.Services.AddSingleton(options);
            builder.Services.AddSingleton<IAudioConverter, FfmpegAudioConverter>();
            return builder;
        }
    }
}
