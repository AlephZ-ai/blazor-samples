using BlazorSamples.Shared.SpeechRecognition;
using BlazorSamples.Shared.SpeechRecognition.Vosk;
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
        public static IHostApplicationBuilder AddVoskSpeechRecognizer(this IHostApplicationBuilder builder, VoskSpeechRecognizerOptions options)
        {
            builder.Services.AddSingleton(options);
            builder.Services.AddSingleton<ISpeechRecognizer, VoskSpeechRecognizer>();
            return builder;
        }
    }
}
