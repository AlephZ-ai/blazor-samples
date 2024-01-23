using BlazorSamples.Shared;
using FFMpegCore.Pipes;
using FFMpegCore;
using Google.Protobuf;
using Microsoft.AspNetCore.SignalR;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using FFMpegCore.Enums;
using Vosk;

namespace BlazorSamples.Web.Hubs
{
    // TODO: Need to fix singleton and static hacks
    // TODO: Reorder out of order buffers or don't use signalr (websockets directly? seems easier)
    public class SpeechToTextHub(VoskRecognizer rec) : Hub<ISpeechToTextClient>
    {
        private static string inputPipeName = "audioWriteToFFMPEGInputPipe";
        private static NamedPipeServerStream dotnetServerWriteOutPipe;
        private static NamedPipeClientStream ffmpegClientReadInFromDotnetServerWriteOutPipe;
        private static Task ffmpegTask;

        public async Task ProcessAudioBuffer(byte[] buffer, BufferPosition position, string mimeType)
        {
            var localFileName = "Files/temp.wav";

            if (position == BufferPosition.First)
            {
                if (File.Exists(localFileName))
                    File.Delete(localFileName);

                if (!Directory.Exists("Files"))
                    Directory.CreateDirectory("Files");

                await InitializePipes();
                ffmpegTask = StartFFMpegProcess(localFileName, mimeType);
            }

            await WriteToServerPipe(buffer);

            if (position == BufferPosition.Last)
            {
                await FinalizePipes();
            }

            await Clients.Caller.ReceiveMessage(Random.Shared.NextInt64().ToString());
        }

        private async Task InitializePipes()
        {
            dotnetServerWriteOutPipe = new NamedPipeServerStream(inputPipeName, PipeDirection.Out);
            ffmpegClientReadInFromDotnetServerWriteOutPipe = new NamedPipeClientStream(".", inputPipeName, PipeDirection.In);

            var dotnetServerWriteOutPipeWaitForConnectionAsync = dotnetServerWriteOutPipe.WaitForConnectionAsync();
            await ffmpegClientReadInFromDotnetServerWriteOutPipe.ConnectAsync();
            await dotnetServerWriteOutPipeWaitForConnectionAsync;
        }

        private Task StartFFMpegProcess(string outputPath, string mimeType) =>
            FFMpegArguments
                .FromPipeInput(new StreamPipeSource(ffmpegClientReadInFromDotnetServerWriteOutPipe), options => options
                    .ForceFormat(mimeType.Substring(6))
                    .UsingMultithreading(false)
                    .WithHardwareAcceleration())
                .OutputToFile(outputPath, true, options => options
                    .OverwriteExisting()
                    .ForceFormat("wav")
                    .WithAudioSamplingRate(16000)
                    .WithAudioBitrate(256)
                    .UsingMultithreading(false)
                    .WithSpeedPreset(Speed.UltraFast)
                    .WithFastStart()
                    .WithCustomArgument("-ac 1"))
                .ProcessAsynchronously();

        private async Task WriteToServerPipe(byte[] buffer)
        {
            await dotnetServerWriteOutPipe.WriteAsync(buffer, 0, buffer.Length);
        }

        private async Task FinalizePipes()
        {
            dotnetServerWriteOutPipe.Disconnect();
            await dotnetServerWriteOutPipe.DisposeAsync();
            await ffmpegTask;
            await ffmpegClientReadInFromDotnetServerWriteOutPipe.DisposeAsync();
        }
    }
}
