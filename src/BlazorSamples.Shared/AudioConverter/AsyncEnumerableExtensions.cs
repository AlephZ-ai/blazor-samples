using BlazorSamples.Shared.AudioConverter;
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
        public static IAsyncEnumerable<ReadOnlyMemory<byte>> ConvertAudioAsync(this IAsyncEnumerable<ReadOnlyMemory<byte>> input, IAudioConverter converter, CancellationToken ct = default)
        {
            return converter.ConvertAsync(input, ct);
        }
    }
}
