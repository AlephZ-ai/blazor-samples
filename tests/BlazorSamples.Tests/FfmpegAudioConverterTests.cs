using BlazorSamples.Shared.AudioConverter.Ffmpeg;
using FFMpegCore.Enums;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorSamples.Tests
{
    [TestClass]
    public class FfmpegAudioConverterTests
    {
        private const string Mulaw = "files/mulaw.wav";
        private const string Out = "files/out.wav";
        private static bool _write = false;
        private static TestContext _context = null!;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _context = context;
        }

        [TestMethod]
        public async Task ConvertMulawAudioTest()
        {
            var ct = _context.CancellationTokenSource.Token;
            var options = new FfmpegAudioConverterOptions
            {
                InFormat = "mulaw",
                InSampleRate = 8000,
                OutFormat = "wav",
                OutSampleRate = 16000,
                OutSpeed = Speed.VerySlow,
            };

            var converter = new FfmpegAudioConverter(options);
            await WriteFileAsync(Out, converter.ConvertAsync(ReadFileAsync(Mulaw, ct), ct), ct);
        }

        private static IAsyncEnumerable<ReadOnlyMemory<byte>> ReadFileAsync(string path, CancellationToken ct)
        {
            var pool = MemoryPool<byte>.Shared;
            var owner = pool.Rent(4 * 1024);
            var buffer = owner.Memory;
            return ReadFileInternalAsync(path, buffer, ct).Finally(() => owner.Dispose());
        }

        private static async IAsyncEnumerable<ReadOnlyMemory<byte>> ReadFileInternalAsync(string path, Memory<byte> buffer, [EnumeratorCancellation] CancellationToken ct)
        {
            await using var file = File.OpenRead(path);
            int read;
            while ((read = await file.ReadAsync(buffer, ct)) > 0)
            {
                yield return buffer[..read];
            }
        }

        private static async Task WriteFileAsync(string path, IAsyncEnumerable<ReadOnlyMemory<byte>> source, CancellationToken ct)
        {
            if (_write)
            {
                await using var file = File.Create(path);
                await foreach (var buffer in source.WithCancellation(ct))
                {
                    await file.WriteAsync(buffer, ct);
                }
            }
            else
            {
                await foreach (var _ in source.WithCancellation(ct)) { }
            }
        }
    }
}
