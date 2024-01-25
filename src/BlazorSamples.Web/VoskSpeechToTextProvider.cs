using BlazorSamples.Shared;
using System;
using System.IO.Compression;
using System.Text.Json;
using Vosk;

namespace BlazorSamples.Web
{
    public sealed class VoskSpeechToTextProvider : ISpeechToTextProvider, IDisposable
    {
        static VoskSpeechToTextProvider()
        {
            Vosk.Vosk.SetLogLevel(0);
        }

        // TODO: Create a recoginzer per client in the signalr hub
        private readonly VoskRecognizer _rec;
        private readonly Model _model;
        private readonly SpkModel _spk;
        private VoskSpeechToTextProvider(string models, string model, string spk)
        {
            _model = new Model($"{models}/{model}");
            _spk = new SpkModel($"{models}/{spk}");
            _rec = new VoskRecognizer(_model, 16000.0f);
            _rec.SetSpkModel(_spk);
            _rec.SetMaxAlternatives(0);
            _rec.SetWords(true);
        }

        public static VoskSpeechToTextProvider Create(string models, string model, string spk)
        {
            return new VoskSpeechToTextProvider(models, model, spk);
        }

        public Task<AppendWavChunk> AppendWavChunk(byte[] buffer, int bytesRead)
        {
            if (_rec.AcceptWaveform(buffer, bytesRead))
            {
                var result = JsonSerializer.Deserialize<RegularResult>(_rec.Result());
                return Task.FromResult(new AppendWavChunk { SentenceFragment = result?.text });
            }
            else
            {
                var result = JsonSerializer.Deserialize<PartialResult>(_rec.Result());
                return Task.FromResult(new AppendWavChunk { SentenceFragment = result?.partial });
            }
        }

        public string? FinalResult()
        {
            var result = JsonSerializer.Deserialize<FinalResult>(_rec.FinalResult());
            return result?.text;
        }

        public void Dispose()
        {
            _rec.Dispose();
            _model.Dispose();
            _spk.Dispose();
        }

        public static async Task DownloadModelsAsync(string models, string model, string spk)
        {
            await DownloadModelAsync(models, model);
            await DownloadModelAsync(models, spk);
        }

        static async Task DownloadModelAsync(string models, string model)
        {
            var modelPath = $"{models}/{model}";
            var zipFile = $"{models}/{model}.zip";
            var url = $"https://alphacephei.com/vosk/models/{model}.zip";
            if (!Directory.Exists(modelPath))
            {
                if (!Directory.Exists(".models")) Directory.CreateDirectory(".models");
                if (!Directory.Exists(models)) Directory.CreateDirectory(models);
                Console.WriteLine($"Downloading Vosk Model {model}");
                Directory.CreateDirectory(modelPath);
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                using var client = new HttpClient(handler);
                using var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsByteArrayAsync();
                await File.WriteAllBytesAsync(zipFile, content);
                ZipFile.ExtractToDirectory(zipFile, models);
                File.Delete(zipFile);
            }
        }
    }
}
