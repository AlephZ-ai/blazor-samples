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
        private static NamedPipeServerStream dotnetWritePipe;
        private static NamedPipeClientStream ffmpegReadPipe;
        private static string pipeName = "audioInputPipe";
        private static Task ffmpegTask;

        public async Task ProcessAudioBuffer(byte[] buffer, BufferPosition position, int p, string mimeType, int sampleRate, int channelCount)
        {
            var localFileName = "Files/temp.wav";

            if (position == BufferPosition.First)
            {
                if (File.Exists(localFileName))
                    File.Delete(localFileName);

                if (!Directory.Exists("Files"))
                    Directory.CreateDirectory("Files");

                await InitializePipes();
                ffmpegTask = StartFFMpegProcess(localFileName, mimeType, sampleRate, channelCount);
            }

            await WriteToServerPipe(buffer);

            if (position == BufferPosition.Last)
            {
                await FinalizePipes();
            }

            await Clients.Caller.ReceiveMessage(p.ToString());
        }

        private async Task InitializePipes()
        {
            dotnetWritePipe = new NamedPipeServerStream(pipeName, PipeDirection.Out);
            ffmpegReadPipe = new NamedPipeClientStream(".", pipeName, PipeDirection.In);

            var dotnetWrite = dotnetWritePipe.WaitForConnectionAsync();
            await ffmpegReadPipe.ConnectAsync();
            await dotnetWrite;
        }

        private Task StartFFMpegProcess(string outputPath, string mimeType, int sampleRate, int channelCount) =>
            FFMpegArguments
                .FromPipeInput(new StreamPipeSource(ffmpegReadPipe), options => options
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
            if (dotnetWritePipe.IsConnected)
            {
                await dotnetWritePipe.WriteAsync(buffer, 0, buffer.Length);
            }
        }

        private async Task FinalizePipes()
        {
            dotnetWritePipe.Disconnect();
            await dotnetWritePipe.DisposeAsync();
            await ffmpegTask;
            await ffmpegReadPipe.DisposeAsync();
        }
    }
}
