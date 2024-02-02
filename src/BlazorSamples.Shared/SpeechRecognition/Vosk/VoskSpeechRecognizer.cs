using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Vosk;

namespace BlazorSamples.Shared.SpeechRecognition.Vosk
{
    public sealed class VoskSpeechRecognizer(VoskSpeechRecognizerOptions options) : ISpeechRecognizer, IDisposable
    {
        public const string MODELS = $"{ISpeechRecognizer.MODELS}/vosk";
        private Model? _model;
        private SpkModel? _spk;
        private int _downloaded = 0;
        private int _disposedValue = 0;

        public async IAsyncEnumerable<SpeechRecognitionResult> RecognizeAsync(IAsyncEnumerable<ReadOnlyMemory<byte>> source, [EnumeratorCancellation] CancellationToken ct = default)
        {
            await EnsureModelsDownloadedAsync(ct).ConfigureAwait(false);
            var rec = new VoskRecognizer(_model, options.SampleRate, _spk);
            try
            {
                rec.SetMaxAlternatives(0);
                rec.SetWords(options.Words);
                rec.SetPartialWords(options.PartialWords);
                await foreach (var buffer in source.WithCancellation(ct).ConfigureAwait(false))
                {
                    if (rec.AcceptWaveform(buffer.ToArray(), buffer.Length))
                    {
                        var result = JsonSerializer.Deserialize<RegularResult>(rec.Result(), options.JsonOptions);
                        yield return new SpeechRecognitionResult { Fragment = result?.text, IsPauseDetected = !string.IsNullOrEmpty(result?.text) };
                    }
                    else
                    {
                        var result = JsonSerializer.Deserialize<PartialResult>(rec.PartialResult(), options.JsonOptions);
                        yield return new SpeechRecognitionResult { Fragment = result?.partial };
                    }
                }

                var final = JsonSerializer.Deserialize<FinalResult>(rec.FinalResult(), options.JsonOptions);
                yield return new SpeechRecognitionResult { Fragment = final?.text, IsPauseDetected = !string.IsNullOrEmpty(final?.text) };
            }
            finally
            {
                rec.Dispose();
            }
        }

        public async ValueTask EnsureModelsDownloadedAsync(CancellationToken ct = default)
        {
            if (Interlocked.CompareExchange(ref _downloaded, 1, 0) == 0)
            {
                await DownloadModelAsync(MODELS, options.Model, ct).ConfigureAwait(false);
                await DownloadModelAsync(MODELS, options.Spk, ct).ConfigureAwait(false);
                _model = new Model($"{MODELS}/{options.Model}");
                _spk = new SpkModel($"{MODELS}/{options.Spk}");
            }
        }

        static async ValueTask DownloadModelAsync(string models, string model, CancellationToken ct)
        {
            var modelPath = $"{models}/{model}";
            var zipFile = $"{models}/{model}.zip";
            var url = $"https://alphacephei.com/vosk/models/{model}.zip";
            if (!Directory.Exists(modelPath))
            {
                if (!Directory.Exists(".models")) Directory.CreateDirectory(".models");
                if (!Directory.Exists(models)) Directory.CreateDirectory(models);
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                using var client = new HttpClient(handler);
                using var response = await client.GetAsync(url, ct).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                using var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
                using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
                // TODO: Use async methods to extract files
                archive.ExtractToDirectory(models, overwriteFiles: true);
            }
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposedValue, 1, 0) == 0)
            {
                _model?.Dispose();
                _spk?.Dispose();
            }
        }
    }
}
