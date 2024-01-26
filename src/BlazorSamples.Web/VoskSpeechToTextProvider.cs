using BlazorSamples.Shared;
using System;
using System.IO.Compression;
using System.Reflection;
using System.Text.Json;
using Vosk;

namespace BlazorSamples.Web
{
    public sealed class VoskSpeechToTextProvider : ISpeechToTextProvider, IDisposable
    {
        public const string Models = $"{ISpeechToTextProvider.Models}/vosk";
        //public const string defautModel = "vosk-model-en-us-0.22";
        public const string DefautModel = "vosk-model-small-en-us-0.15";
        public const string DefautSpk = "vosk-model-spk-0.4";
        static VoskSpeechToTextProvider()
        {
            Vosk.Vosk.SetLogLevel(0);
        }

        // TODO: Create a recoginzer per client in the signalr hub
        private readonly VoskRecognizer _rec;
        private readonly Model _model;
        private readonly SpkModel _spk;
        public VoskSpeechToTextProvider()
        {
            _model = new Model($"{Models}/{DefautModel}");
            _spk = new SpkModel($"{Models}/{DefautSpk}");
            _rec = new VoskRecognizer(_model, 16000.0f);
            _rec.SetSpkModel(_spk);
            _rec.SetMaxAlternatives(0);
            _rec.SetWords(true);
        }

        public Task<AppendWavChunk> AppendWavChunk(byte[] buffer, int bytesRead)
        {
            if (_rec.AcceptWaveform(buffer, bytesRead))
            {
                var result = JsonSerializer.Deserialize<RegularResult>(_rec.Result());
                return Task.FromResult(new AppendWavChunk { CompleteSentence = result?.text });
            }
            else
            {
                var result = JsonSerializer.Deserialize<PartialResult>(_rec.PartialResult());
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

        public async Task DownloadModelsAsync()
        {
            await DownloadModelAsync(Models, DefautModel);
            await DownloadModelAsync(Models, DefautSpk);
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
