// See https://aka.ms/new-console-template for more information
using NAudio.Wave;
using NLayer.NAudioSupport;
using Whisper.net;
using Whisper.net.Ggml;

// We declare three variables which we will use later, ggmlType, modelFileName and mp3FileName
var ggmlType = GgmlType.Base;
var modelFileName = "ggml-tiny.bin";
var mp3FileName = "sample.mp3";

// This section detects whether the "ggml-base.bin" file exists in our project disk. If it doesn't, it downloads it from the internet
if (!File.Exists(modelFileName))
{
    await DownloadModel(modelFileName, ggmlType);
}

// This section creates the whisperFactory object which is used to create the processor object.
using var whisperFactory = WhisperFactory.FromPath("ggml-tiny.bin");

// This section creates the processor object which is used to process the audio file, it uses language `auto` to detect the language of the audio file.
using var processor = whisperFactory.CreateBuilder()
    .WithLanguage("auto")
    .Build();


var builder = new Mp3FileReaderBase.FrameDecompressorBuilder(wf => new Mp3FrameDecompressor(wf));
using var reader = new Mp3FileReaderBase(mp3FileName, builder);
if (!File.Exists("sample.wav"))
{
    WaveFileWriter.CreateWaveFile("sample.wav", reader);
}

using FileStream wavStream = File.OpenRead("sample.wav");
do
{
    // This section sets the wavStream to the beginning of the stream. (This is required because the wavStream was written to in the previous section)

    wavStream.Seek(0, SeekOrigin.Begin);
    // This section processes the audio file and prints the results (start time, end time and text) to the console.
    await foreach (var result in processor.ProcessAsync(wavStream))
    {
        Console.WriteLine($"{result.Start}->{result.End}: {result.Text}");
    }

    Console.WriteLine("Done");
} while (true);

static async Task DownloadModel(string fileName, GgmlType ggmlType)
{
    Console.WriteLine($"Downloading Model {fileName}");
    using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(ggmlType);
    using var fileWriter = File.OpenWrite(fileName);
    await modelStream.CopyToAsync(fileWriter);
}
