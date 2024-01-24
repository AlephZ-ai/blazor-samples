// See https://aka.ms/new-console-template for more information
// Demo byte buffer
using System.IO.Compression;
using Vosk;

namespace BlazorSamples.ScratchConsole
{
    public class VoskProgram
    {
        static string models = ".models";
        static string voskModels = $"{models}/vosk";
        public static void Main()
        {
            MainAsync().Wait();
        }

        public static async Task MainAsync()
        {
            var model = "vosk-model-en-us-0.22";
            var modelSpk = "vosk-model-spk-0.4";
            await DownloadVoskModelAsync(model);
            await DownloadVoskModelAsync(modelSpk);
            Vosk.Vosk.SetLogLevel(0);
            VoskRecognizer rec = new VoskRecognizer(new Model($"{voskModels}/{model}"), 16000.0f);
            rec.SetSpkModel(new SpkModel($"{voskModels}/{modelSpk}"));
            rec.SetMaxAlternatives(0);
            rec.SetWords(true);
            using Stream source = File.OpenRead("sample.wav");
            byte[] buffer = new byte[4096];
            int bytesRead;
            do
            {
                source.Seek(0, SeekOrigin.Begin);
                while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (rec.AcceptWaveform(buffer, bytesRead))
                    {
                        Console.WriteLine(rec.Result());
                    }
                    else
                    {
                        Console.WriteLine(rec.PartialResult());
                    }
                }

                Console.WriteLine(rec.FinalResult());
                Console.WriteLine("----------------------Done----------------------");
            } while (true);
        }

        static async Task DownloadVoskModelAsync(string model)
        {
            var modelPath = $"{voskModels}/{model}";
            var zipFile = $"{voskModels}/{model}.zip";
            var url = $"https://alphacephei.com/vosk/models/{model}.zip";
            if (!Directory.Exists(modelPath))
            {
                Console.WriteLine($"Downloading Model {model}");
                if (!Directory.Exists(models)) Directory.CreateDirectory(models);
                if (!Directory.Exists(voskModels)) Directory.CreateDirectory(voskModels);
                Directory.CreateDirectory(modelPath);
                using var client = new HttpClient();
                using var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsByteArrayAsync();
                await File.WriteAllBytesAsync(zipFile, content);
                ZipFile.ExtractToDirectory(zipFile, voskModels);
                File.Delete(zipFile);
            }
        }
    }
}
