using BlazorSamples.Shared;
using FFMpegCore.Pipes;
using FFMpegCore;
using Google.Protobuf;
using Microsoft.AspNetCore.SignalR;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using Vosk;

namespace BlazorSamples.Web.Hubs
{
    // TODO: Need to fix singleton and static hacks
    // TODO: Reorder out of order buffers or don't use signalr (websockets directly? seems easier)
    public class SpeechToTextHub : Hub<ISpeechToTextClient>
    {
        private static NamedPipeServerStream serverPipe;
        private static NamedPipeClientStream clientPipe;
        private static string pipeName = "audioPipe";
        private static Task ffmpegTask;

        public async Task ProcessAudioBuffer(byte[] buffer, BufferPosition position, int p, string mimeType, int sampleRate, int channelCount)
        {
            var localFileName = "Files/temp.wav";

            if (position == BufferPosition.First)
            {
                if (!Directory.Exists("Files"))
                    Directory.CreateDirectory("Files");

                if (File.Exists(localFileName))
                    File.Delete(localFileName);
                
                InitializePipes();
                StartFFMpegProcess(localFileName, sampleRate, channelCount);
            }

            WriteToServerPipe(buffer);

            if (position == BufferPosition.Last)
            {
                FinalizePipes();
                await ffmpegTask;
            }

            await Clients.Caller.ReceiveMessage(p.ToString());
        }

        private void InitializePipes()
        {
            serverPipe = new NamedPipeServerStream(pipeName, PipeDirection.Out);
            clientPipe = new NamedPipeClientStream(".", pipeName, PipeDirection.In);

            serverPipe.WaitForConnectionAsync();
            clientPipe.Connect();
        }

        private void StartFFMpegProcess(string outputPath, int sampleRate, int channelCount)
        {
            ffmpegTask = FFMpegArguments
                .FromPipeInput(new StreamPipeSource(clientPipe), options => options
                    .ForceFormat("webm"))
                .OutputToFile(outputPath, false, options => options
                    .ForceFormat("wav")
                    .WithAudioSamplingRate(sampleRate)
                    .WithCustomArgument($"-ac {channelCount}"))
                .ProcessAsynchronously();
        }

        private void WriteToServerPipe(byte[] buffer)
        {
            if (serverPipe.IsConnected)
            {
                serverPipe.Write(buffer, 0, buffer.Length);
            }
        }

        private void FinalizePipes()
        {
            serverPipe.Disconnect();
            serverPipe.Close();
            clientPipe.Close();
        }
    }
}
