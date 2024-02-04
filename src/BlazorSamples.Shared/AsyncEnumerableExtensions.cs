using Google.Protobuf;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using BlazorSamples.Shared;
using Microsoft.Extensions.Logging;
using Utf8BytesAsyncEnumerable = System.Collections.Generic.IAsyncEnumerable<System.ReadOnlyMemory<byte>>;
using Utf8JsonBytesAsyncEnumerable = System.Collections.Generic.IAsyncEnumerable<System.ReadOnlyMemory<byte>>;
using Utf8JsonStringAsyncEnumerable = System.Collections.Generic.IAsyncEnumerable<string?>;
using Utf8StringAsyncEnumerable = System.Collections.Generic.IAsyncEnumerable<string?>;

namespace System.Collections.Generic
{
    public static partial class AsyncEnumerableExtensions
    {
        public const int DefaultBufferSize = 4 * 1024;

        public static async IAsyncEnumerable<string> DetectSentenceSimple(this IAsyncEnumerable<string> source, [EnumeratorCancellation] CancellationToken ct = default)
        {
            var sentence = string.Empty;
            await foreach (var partial in source.WithCancellation(ct).ConfigureAwait(false))
            {
                sentence += partial ?? string.Empty;
                if (DetectSentenceSimple(ref sentence, out var leftOver))
                {
                    yield return sentence;
                    sentence = leftOver;
                }
            }
        }
        
        private static bool DetectSentenceSimple(ref string sentence, out string leftOver)
        {
            if (sentence.Contains('.') || sentence.Contains('!') || sentence.Contains('?'))
            {
                var split = sentence.Split(new[] { '.', '!', '?' }, 2);
                sentence = split[0].Trim();
                leftOver = split[1] ?? string.Empty;
                return true;
            }

            leftOver = string.Empty;
            return false;
        }

        public static IAsyncEnumerable<T?> ConvertFromJsonAsync<T>(this Utf8BytesAsyncEnumerable source, JsonSerializerOptions? jsonOptions = null)
        {
            jsonOptions ??= JsonSerializerOptions.Default;
            return source.Select(json => JsonSerializer.Deserialize<T>(json.Span, jsonOptions));
        }

        public static Utf8JsonBytesAsyncEnumerable ToJsonBytesAsync<T>(this IAsyncEnumerable<T> source, JsonSerializerOptions? jsonOptions = null)
        {
            //TODO: Not happy with this memory footprint
            jsonOptions ??= JsonSerializerOptions.Default;
            return source.Select(obj => new ReadOnlyMemory<byte>(JsonSerializer.SerializeToUtf8Bytes(obj, jsonOptions)));
        }

        public static Utf8JsonStringAsyncEnumerable ToJsonStringAsync<T>(this IAsyncEnumerable<T> source, JsonSerializerOptions? jsonOptions = null)
        {
            //TODO: Not happy with this memory footprint
            jsonOptions ??= JsonSerializerOptions.Default;
            return source.Select(obj => JsonSerializer.Serialize(obj, jsonOptions));
        }

        public static Utf8BytesAsyncEnumerable ToBytesAsync(this Utf8StringAsyncEnumerable source, ILogger log, int initialBufferSize = DefaultBufferSize)
        {
            log.LogEnter();
            var pool = MemoryPool<byte>.Shared;
            var owner = pool.Rent(initialBufferSize);
            var buffer = owner.Memory;
            return source
                .Where(str => !string.IsNullOrEmpty(str))
                .Select(ReadOnlyMemory<byte> (str) =>
                {
                    log.LogLoop();
                    ResizeBufferIfNeeded(pool, ref owner, ref buffer, 0, Encoding.UTF8.GetByteCount(str!), log);
                    var length = Encoding.UTF8.GetBytes(str, buffer.Span);
                    log.LogYield();
                    return buffer[..length];
                })
                .Finally(() =>
                {
                    owner.Dispose();
                    log.LogExit();
                });
        }

        public static Utf8StringAsyncEnumerable ToStringAsync(this Utf8BytesAsyncEnumerable source) =>
            source.Select(buffer => Encoding.UTF8.GetString(buffer.Span));

        public static IAsyncEnumerable<ReadOnlyMemory<T>> ExcludeEmpty<T>(
            this IAsyncEnumerable<ReadOnlyMemory<T>> source,
            ILogger? log = null)
        {
            log?.LogEnter();
            return source.Where(buffer => buffer.Length > 0)
                .Select(i => { log?.LogYield(); return i; })
                .Finally(() => log?.LogExit());
        }

        public static IAsyncEnumerable<T> ExcludeNull<T>(this IAsyncEnumerable<T?> source)
            where T : class =>
            source.Where(item => item is not null)!;

        internal static void ResizeBufferIfNeeded(MemoryPool<byte> pool, ref IMemoryOwner<byte> owner, ref Memory<byte> buffer, int offset, int addedLength, ILogger log)
        {
            try
            {
                var combinedSize = offset + addedLength;
                if (combinedSize > buffer.Length)
                {
                    var newSize = buffer.Length;
                    while (newSize < combinedSize) newSize *= 2;
                    log.LogBufferResize(buffer.Length, newSize);
                    var newBuffer = pool.Rent(newSize);
                    buffer.CopyTo(newBuffer.Memory);
                    buffer = newBuffer.Memory;
                    owner.Dispose();
                    owner = newBuffer;
                }
            }
            catch (Exception ex)
            {
                log.LogException(ex);
                throw;
            }
        }
    }
}
