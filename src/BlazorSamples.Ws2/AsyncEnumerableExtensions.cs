﻿using Google.Protobuf;
using System.Buffers;
using System.Text;
using System.Text.Json;

namespace BlazorSamples.Ws2
{
    public static class AsyncEnumerableExtensions
    {
        public const int DefaultBufferSize = 4 * 1024;
        public static IAsyncEnumerable<T?> ToTFromJsonAsyncEnumerable<T>(this IAsyncEnumerable<ReadOnlyMemory<byte>> source, JsonSerializerOptions? jsonOptions = null)
        {
            jsonOptions ??= JsonSerializerOptions.Default;
            return source.Select(json => JsonSerializer.Deserialize<T>(json.Span, jsonOptions));
        }

        public static IAsyncEnumerable<ReadOnlyMemory<byte>> ToJsonBytesAsyncEnumerable<T>(this IAsyncEnumerable<T> source, JsonSerializerOptions? jsonOptions = null)
        {
            //TODO: Not happy with this memory footprint
            jsonOptions ??= JsonSerializerOptions.Default;
            return source.Select(ReadOnlyMemory<byte> (obj) => new Memory<byte>(JsonSerializer.SerializeToUtf8Bytes(obj, jsonOptions)));
        }

        public static IAsyncEnumerable<string> ToJsonStringAsyncEnumerable<T>(this IAsyncEnumerable<T> source, JsonSerializerOptions? jsonOptions = null)
        {
            //TODO: Not happy with this memory footprint
            jsonOptions ??= JsonSerializerOptions.Default;
            return source.Select(obj => JsonSerializer.Serialize(obj, jsonOptions));
        }

        public static IAsyncEnumerable<ReadOnlyMemory<byte>> ToBytesAsyncEnumerable(this IAsyncEnumerable<string?> source, int initialBufferSize = DefaultBufferSize)
        {
            var pool = MemoryPool<byte>.Shared;
            var owner = pool.Rent(initialBufferSize);
            var buffer = owner.Memory;
            return source
            .Where(str => !string.IsNullOrEmpty(str))
            .Select(ReadOnlyMemory<byte> (str) =>
            {
                ResizeBufferIfNeeded(pool, ref owner, ref buffer, 0, Encoding.UTF8.GetByteCount(str!));
                var length = Encoding.UTF8.GetBytes(str, buffer.Span);
                return buffer[..length];
            })
            .Finally(() =>
            {
                owner.Dispose();
            });
        }

        public static IAsyncEnumerable<string?> ToStringAsyncEnumerable(this IAsyncEnumerable<ReadOnlyMemory<byte>> source) =>
            source.Select(buffer => Encoding.UTF8.GetString(buffer.Span));

        public static IAsyncEnumerable<ReadOnlyMemory<T>> ExcludeEmpty<T>(this IAsyncEnumerable<ReadOnlyMemory<T>> source) =>
            source.Where(buffer => buffer.Length > 0);

        internal static void ResizeBufferIfNeeded(MemoryPool<byte> pool, ref IMemoryOwner<byte> owner, ref Memory<byte> buffer, int offset, int addedLength)
        {
            var combinedSize = offset + addedLength;
            if (combinedSize > buffer.Length)
            {
                var newSize = buffer.Length;
                while (newSize < combinedSize) newSize *= 2;
                var newBuffer = pool.Rent(newSize);
                buffer.CopyTo(newBuffer.Memory);
                buffer = newBuffer.Memory;
                owner.Dispose();
                owner = newBuffer;
            }
        }
    }
}
