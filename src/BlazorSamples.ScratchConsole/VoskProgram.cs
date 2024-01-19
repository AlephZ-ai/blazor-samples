// See https://aka.ms/new-console-template for more information
// Demo byte buffer
using Vosk;

namespace BlazorSamples.ScratchConsole
{
    public static class VoskProgram
    {
        public static void Main()
        {
            Vosk.Vosk.SetLogLevel(0);
            VoskRecognizer rec = new VoskRecognizer(new Model("model"), 16000.0f);
            rec.SetSpkModel(new SpkModel("model-spk"));
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
                Console.WriteLine("Done");
            } while (true);
        }
    }
}
