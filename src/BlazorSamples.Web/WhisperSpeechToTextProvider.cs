using Nerdbank.Streams;
using System.Threading.Channels;
using Whisper.net;
using Whisper.net.Ggml;

namespace BlazorSamples.Web
{
    public sealed class WhisperSpeechToTextProvider : ISpeechToTextProvider, IDisposable
    {
        public static string Models = $"{ISpeechToTextProvider.Models}/whisper";
        //public static GgmlType DefaultModel = GgmlType.LargeV3;
        public static GgmlType DefaultModel = GgmlType.Tiny;
        private readonly WhisperFactory _factory;
        private readonly WhisperProcessor _processor;
        private static DuplexMemoryStream? _s;
        private static Task? _processing;
        private static Channel<string>? _channel;
        public WhisperSpeechToTextProvider()
        {
            var model = Enum.GetName(DefaultModel);
            _factory = WhisperFactory.FromPath($"{Models}/{model}.bin");
            _processor = _factory.CreateBuilder()
                .WithLanguage("auto")
                .Build();
         }

        public async Task Process()
        {
            try
            {
                await foreach (var result in _processor.ProcessAsync(_s!))
                {
                    if (result is null || string.IsNullOrWhiteSpace(result.Text)) continue;
                    _channel?.Writer.TryWrite(result.Text);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            var x = _s;
        }

        public async Task<AppendWavChunk> AppendWavChunk(byte[] buffer, int bytesRead)
        {
            if (_s is null)
            {
                _s = new DuplexMemoryStream();
                _channel = Channel.CreateUnbounded<string>();
            }

            await _s.WriteAsync(buffer, 0, bytesRead);
            await _s.FlushAsync();
            if (_processing is null)
            {
                _processing = Process();
            }

            var concat = string.Empty;
            while (_channel!.Reader.TryRead(out var result))
            {
                concat += result;
            }

            return new AppendWavChunk { SentenceFragment = string.IsNullOrWhiteSpace(concat) ? null : concat };
        }

        public void Dispose()
        {
            _processor.Dispose();
            _factory.Dispose();
        }

        public string? FinalResult()
        {
            _processing = null;
            _channel?.Writer.TryComplete();
            _channel = null;
            _s?.Dispose();
            _s = null;
            return null;
        }

        public async Task DownloadModelsAsync()
        {
            await DownloadModelAsync(Models, DefaultModel);
        }

        static async Task DownloadModelAsync(string models, GgmlType modelType)
        {
            var model = modelType;
            var modelPath = $"{models}/{model}.bin";
            if (!File.Exists(modelPath))
            {
                Console.WriteLine($"Downloading Whisper Model {model}");
                if (!Directory.Exists(".models")) Directory.CreateDirectory(".models");
                if (!Directory.Exists(models)) Directory.CreateDirectory(models);
                using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(model);
                using var fileWriter = File.OpenWrite(modelPath);
                await modelStream.CopyToAsync(fileWriter);
            }
        }
    }
}
