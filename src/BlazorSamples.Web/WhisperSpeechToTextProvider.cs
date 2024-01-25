using System.Threading.Channels;
using Whisper.net;
using Whisper.net.Ggml;

namespace BlazorSamples.Web
{
    public sealed class WhisperSpeechToTextProvider : ISpeechToTextProvider, IDisposable
    {
        private readonly WhisperFactory _factory;
        private readonly WhisperProcessor _processor;
        private readonly Channel<byte[]> _inChannel;
        private WhisperSpeechToTextProvider(string models, GgmlType modelType)
        {
            var model = Enum.GetName(modelType);
            _inChannel = Channel.CreateUnbounded<byte[]>();
            _factory = WhisperFactory.FromPath($"{models}/{model}.bin");
            _processor = _factory.CreateBuilder()
                .WithLanguage("auto")
                .Build();
         }

        public static WhisperSpeechToTextProvider Create(string models, GgmlType modelType)
        {
            return new WhisperSpeechToTextProvider(models, modelType);
        }

        public async Task<AppendWavChunk> AppendWavChunk(byte[] buffer, int bytesRead)
        {
            var ms = new MemoryStream(buffer, 0, bytesRead);
            ms.Seek(0, SeekOrigin.Begin);
            var concat = string.Empty;
            await foreach (var result in _processor.ProcessAsync(ms))
            {
                concat += result.Text;
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
            return null;
        }

        public static async Task DownloadModelsAsync(string models, GgmlType modelType)
        {
            await DownloadModelAsync(models, modelType);
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
