using BlazorSamples.Shared.SpeechRecognition.Vosk;
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
    public class VoskSpeechRecognizerTests
    {
        private const string WAV = "files/wav.wav";
        private static TestContext _context = null!;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _context = context;
        }

        [TestMethod]
        public async Task RecognizeSpeechTest()
        {
            var ct = _context.CancellationTokenSource.Token;
            var options = new VoskSpeechRecognizerOptions
            {
                JsonOptions = JsonSerializerOptions.Default,
            };

            var recognizer = new VoskSpeechRecognizer(options);
            await recognizer.EnsureModelsDownloadedAsync(ct);
            var results = new List<string>();
            await foreach (var text in recognizer.RecognizeAsync(ReadFileAsync(WAV, ct), ct).WithCancellation(ct))
            {
                if (!string.IsNullOrEmpty(text?.Fragment) && text.IsPauseDetected)
                {
                    _context.WriteLine(text.Fragment);
                    results.Add(text.Fragment);
                }
            }

            Assert.IsTrue(results.Any());
            Assert.IsFalse(string.IsNullOrEmpty(results[0]));
            recognizer.Dispose();
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
    }
}
