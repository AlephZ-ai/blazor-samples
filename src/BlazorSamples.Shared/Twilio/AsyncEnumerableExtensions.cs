using BlazorSamples.Shared.Twilio.GrpcAudioStream;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions;
using Google.Protobuf;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace System.Collections.Generic
{
    public static partial class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<DefaultTwilioProcessorResult?> ProcessTwilioEvent(this IAsyncEnumerable<InboundEvent> input)
        {
            var processor = new DefaultTwilioProcessor();
            return input.Select(processor.ProcessEvent);
        }
    }
}
