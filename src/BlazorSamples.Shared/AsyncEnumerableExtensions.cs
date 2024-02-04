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

        public static async IAsyncEnumerable<string> DetectSentenceSimple(this IAsyncEnumerable<string> source, ILogger log, [EnumeratorCancellation] CancellationToken ct = default)
        {
            log.Enter();
            try
            {
                var sentence = string.Empty;
                await foreach (var partial in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    log.Loop();
                    sentence += partial ?? string.Empty;
                    if (DetectSentenceSimple(ref sentence, out var leftOver, log))
                    {
                        log.Yield();
                        yield return sentence;
                        sentence = leftOver;
                    }
                }
            }
            finally
            {
                log.Exit();
            }
        }
        
        private static bool DetectSentenceSimple(ref string sentence, out string leftOver, ILogger log)
        {
            try
            {
                if (sentence.Contains('.') || sentence.Contains('!') || sentence.Contains('?'))
                {
                    var split = sentence.Split(new[] { '.', '!', '?' }, 2);
                    sentence = split[0].Trim();
                    leftOver = split[1] ?? string.Empty;
                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Exception(ex);
            }

            leftOver = string.Empty;
            return false;
        }

        public static IAsyncEnumerable<T?> ConvertFromJsonAsync<T>(this Utf8BytesAsyncEnumerable source, ILogger log, JsonSerializerOptions? jsonOptions = null)
        {
            log.Enter();
            jsonOptions ??= JsonSerializerOptions.Default;
            return source
                .Select(json =>
                {
                    log.Loop();
                    try
                    {
                        return JsonSerializer.Deserialize<T>(json.Span, jsonOptions);
                    }
                    catch (Exception ex)
                    {
                        log.Exception(ex);
                        throw;
                    }
                })
                .Select(x => { log.Yield(); return x; })
                .Finally(() => log.Exit());
        }

        public static Utf8JsonBytesAsyncEnumerable ToJsonBytesAsync<T>(this IAsyncEnumerable<T> source, ILogger log, JsonSerializerOptions? jsonOptions = null)
        {
            log.Enter();
            //TODO: Not happy with this memory footprint
            jsonOptions ??= JsonSerializerOptions.Default;
            return source
                .Select(obj =>
                {
                    log.Loop();
                    try
                    {
                        return new ReadOnlyMemory<byte>(JsonSerializer.SerializeToUtf8Bytes(obj, jsonOptions));
                    }
                    catch (Exception ex)
                    {
                        log.Exception(ex);
                        throw;
                    }
                })
                .Select(x => { log.Yield(); return x; })
                .Finally(() => log.Exit());
        }

        public static Utf8JsonStringAsyncEnumerable ToJsonStringAsync<T>(this IAsyncEnumerable<T> source, ILogger log, JsonSerializerOptions? jsonOptions = null)
        {
            log.Enter();
            //TODO: Not happy with this memory footprint
            jsonOptions ??= JsonSerializerOptions.Default;
            return source
                .Select(obj =>
                {
                    log.Loop();
                    try
                    {
                        return JsonSerializer.Serialize(obj, jsonOptions);
                    }
                    catch (Exception ex)
                    {
                        log.Exception(ex);
                        throw;
                    }
                })
                .Select(x => { log.Yield(); return x; })
                .Finally(() => log.Exit());
        }

        public static Utf8BytesAsyncEnumerable ToBytesAsync(this Utf8StringAsyncEnumerable source, ILogger log, int initialBufferSize = DefaultBufferSize)
        {
            log.Enter();
            var pool = MemoryPool<byte>.Shared;
            var owner = pool.Rent(initialBufferSize);
            var buffer = owner.Memory;
            return source
                .Select(x => { log.Loop(); return x; })
                .Where(str => !string.IsNullOrEmpty(str))
                .Select(ReadOnlyMemory<byte> (str) =>
                {
                    ResizeBufferIfNeeded(pool, ref owner, ref buffer, 0, Encoding.UTF8.GetByteCount(str!), log);
                    var length = Encoding.UTF8.GetBytes(str, buffer.Span);
                    log.Yield();
                    return buffer[..length];
                })
                .Finally(() =>
                {
                    owner.Dispose();
                    log.Exit();
                });
        }

        public static Utf8StringAsyncEnumerable ToStringAsync(this Utf8BytesAsyncEnumerable source, ILogger log)
        {
            log.Enter();
            return source
                .Select(buffer =>
                {
                    log.Loop();
                    try
                    {
                        return Encoding.UTF8.GetString(buffer.Span);
                    }
                    catch (Exception ex)
                    {
                        log.Exception(ex);
                        throw;
                    }
                })
                .Select(x => { log.Yield(); return x; })
                .Finally(() => log.Exit());
        }

        public static IAsyncEnumerable<ReadOnlyMemory<T>> ExcludeEmpty<T>(
            this IAsyncEnumerable<ReadOnlyMemory<T>> source,
            ILogger log)
        {
            log.Enter();
            return source
                .Select(i => { log.Loop(); return i; })
                .Where(buffer => buffer.Length > 0)
                .Select(i => { log.Yield(); return i; })
                .Finally(() => log.Exit());
        }

        public static IAsyncEnumerable<T> ExcludeNull<T>(this IAsyncEnumerable<T?> source, ILogger log)
            where T : class
        {
            log.Enter();
            return source
                .Select(i => { log.Loop(); return i; })
                .Where(item => item is not null)
                .Select(i => { log.Yield(); return i; })
                .Finally(() => log.Exit())!;
        }

        internal static void ResizeBufferIfNeeded(MemoryPool<byte> pool, ref IMemoryOwner<byte> owner, ref Memory<byte> buffer, int offset, int addedLength, ILogger log)
        {
            try
            {
                var combinedSize = offset + addedLength;
                if (combinedSize > buffer.Length)
                {
                    var newSize = buffer.Length;
                    while (newSize < combinedSize) newSize *= 2;
                    log.BufferResize(buffer.Length, newSize);
                    var newBuffer = pool.Rent(newSize);
                    buffer.CopyTo(newBuffer.Memory);
                    buffer = newBuffer.Memory;
                    owner.Dispose();
                    owner = newBuffer;
                }
            }
            catch (Exception ex)
            {
                log.Exception(ex);
                throw;
            }
        }
    }
}
