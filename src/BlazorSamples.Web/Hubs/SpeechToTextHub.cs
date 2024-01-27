using BlazorSamples.Shared;
using FFMpegCore.Pipes;
using FFMpegCore;
using Google.Protobuf;
using Microsoft.AspNetCore.SignalR;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using FFMpegCore.Enums;
using Vosk;
using System.Text;
using System.Text.Json;
using BlazorSamples.Shared.Clients;
using BlazorSamples.Shared.SpeechToText;

namespace BlazorSamples.Web.Hubs
{
    // TODO: Need to fix singleton and static hacks
    // TODO: Reorder out of order buffers or don't use signalr (websockets directly? seems easier)
    public class SpeechToTextHub(ISpeechToTextProvider provider) : Hub<ISpeechToTextClient>
    {
        private static string audioWriteDotnetServerOutToFfmpegClientInPipe =
            Guid.NewGuid().ToString();

        private static NamedPipeServerStream dotnetServerWriteOutPipe = null!;
        private static NamedPipeClientStream ffmpegClientReadInFromDotnetServerWriteOutPipe = null!;

        private static string audioWriteFfmpegServerOutToDotnetClientInPipe =
            Guid.NewGuid().ToString();

        private static NamedPipeServerStream ffmpegServerWriteOutPipe = null!;
        private static NamedPipeClientStream dotnetClientReadInFromFfmpegServerWriteOutPipe = null!;
        private static Task ffmpegTask = null!;
        private static Task dotnetTask = null!;

        public async Task ProcessAudioBuffer(byte[] buffer, BufferPosition position, string mimeType)
        {
            var caller = Clients.Caller;
            if (position == BufferPosition.First)
            {
                await OpenPipes().ConfigureAwait(false);
                ffmpegTask = StartFFMpegProcess(mimeType);
                dotnetTask = DotnetClientReadInFromFfmpegWriteServerOutPipe(caller);
            }

            await WriteToDotnetServerOutPipe(buffer).ConfigureAwait(false);
            if (position == BufferPosition.Last)
            {
                await ClosePipes().ConfigureAwait(false);
            }
        }

        private async Task OpenPipes()
        {
            dotnetServerWriteOutPipe =
                new NamedPipeServerStream(audioWriteDotnetServerOutToFfmpegClientInPipe, PipeDirection.Out);
            ffmpegClientReadInFromDotnetServerWriteOutPipe = new NamedPipeClientStream(".",
                audioWriteDotnetServerOutToFfmpegClientInPipe, PipeDirection.In);
            ffmpegServerWriteOutPipe =
                new NamedPipeServerStream(audioWriteFfmpegServerOutToDotnetClientInPipe, PipeDirection.Out);
            dotnetClientReadInFromFfmpegServerWriteOutPipe = new NamedPipeClientStream(".",
                audioWriteFfmpegServerOutToDotnetClientInPipe, PipeDirection.In);
            var dotnetServerWriteOutPipeWaitForConnectionAsync = dotnetServerWriteOutPipe.WaitForConnectionAsync();
            var ffmpegServerWriteOutPipeWaitForConnectionAsync = ffmpegServerWriteOutPipe.WaitForConnectionAsync();
            await Task.WhenAll(ffmpegClientReadInFromDotnetServerWriteOutPipe.ConnectAsync(),
                dotnetClientReadInFromFfmpegServerWriteOutPipe.ConnectAsync()).ConfigureAwait(false);
            await Task.WhenAll(dotnetServerWriteOutPipeWaitForConnectionAsync,
                ffmpegServerWriteOutPipeWaitForConnectionAsync).ConfigureAwait(false);
        }

        private Task StartFFMpegProcess(string mimeType) =>
            FFMpegArguments
                .FromPipeInput(new StreamPipeSource(ffmpegClientReadInFromDotnetServerWriteOutPipe), options => options
                    .ForceFormat(mimeType.Substring(6))
                    .UsingMultithreading(false)
                    .WithHardwareAcceleration())
                .OutputToPipe(new StreamPipeSink(ffmpegServerWriteOutPipe), options => options
                    .ForceFormat("wav")
                    .WithAudioSamplingRate(16000)
                    .WithAudioBitrate(256)
                    .UsingMultithreading(false)
                    .WithSpeedPreset(Speed.UltraFast)
                    .WithFastStart()
                    .WithCustomArgument("-ac 1"))
                .ProcessAsynchronously();

        private async Task WriteToDotnetServerOutPipe(byte[] buffer)
        {
            await dotnetServerWriteOutPipe.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
        }

        private async Task ClosePipes()
        {
            dotnetServerWriteOutPipe.Disconnect();
            await ffmpegTask.ConfigureAwait(false);
            await dotnetServerWriteOutPipe.DisposeAsync().ConfigureAwait(false);
            await ffmpegClientReadInFromDotnetServerWriteOutPipe.DisposeAsync().ConfigureAwait(false);
            ffmpegServerWriteOutPipe.Disconnect();
            await dotnetTask.ConfigureAwait(false);
            await ffmpegServerWriteOutPipe.DisposeAsync().ConfigureAwait(false);
            await dotnetClientReadInFromFfmpegServerWriteOutPipe.DisposeAsync().ConfigureAwait(false);
        }

        private async Task DotnetClientReadInFromFfmpegWriteServerOutPipe(ISpeechToTextClient caller)
        {
            int bytesRead;
            var buffer = new byte[4096];
            var localFileName = "Files/temp.wav";
            if (File.Exists(localFileName))
                File.Delete(localFileName);

            if (!Directory.Exists("Files"))
                Directory.CreateDirectory("Files");

            while ((bytesRead = await dotnetClientReadInFromFfmpegServerWriteOutPipe.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0)
            {
                var result = await provider.AppendWavChunk(buffer, bytesRead).ConfigureAwait(false);
                if (!string.IsNullOrWhiteSpace(result.CompleteSentence))
                {
                    await caller.ReceiveResult(new RegularResult { text = result.CompleteSentence }).ConfigureAwait(false);
                }
                else if (!string.IsNullOrWhiteSpace(result.SentenceFragment))
                {
                    await caller.ReceivePartialResult(new PartialResult { partial = result.SentenceFragment }).ConfigureAwait(false);
                }
            }

            var finalResult = provider.FinalResult();
            if (!string.IsNullOrWhiteSpace(finalResult))
            {
                await caller.ReceiveFinalResult(new FinalResult { text = finalResult }).ConfigureAwait(false);
            }
        }
    }
}
