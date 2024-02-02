using BlazorSamples.Shared.SpeechRecognition;
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
        public static IAsyncEnumerable<SpeechRecognitionResult> RecognizeSpeechAsync(this IAsyncEnumerable<ReadOnlyMemory<byte>> input, ISpeechRecognizer converter, CancellationToken ct = default)
        {
            return converter.RecognizeAsync(input, ct);
        }
    }
}
