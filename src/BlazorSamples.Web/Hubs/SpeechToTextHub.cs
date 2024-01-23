﻿using BlazorSamples.Shared;
using FFMpegCore.Pipes;
using FFMpegCore;
using Google.Protobuf;
using Microsoft.AspNetCore.SignalR;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using FFMpegCore.Enums;
using Vosk;
using System.Text;

namespace BlazorSamples.Web.Hubs
{
    // TODO: Need to fix singleton and static hacks
    // TODO: Reorder out of order buffers or don't use signalr (websockets directly? seems easier)
    public class SpeechToTextHub(VoskRecognizer rec) : Hub<ISpeechToTextClient>
    {
        private static string audioWriteDotnetServerOutToFfmpegClientInPipe = "audioWriteDotnetServerOutToFfmpegClientInPipe";
        private static NamedPipeServerStream dotnetServerWriteOutPipe;
        private static NamedPipeClientStream ffmpegClientReadInFromDotnetServerWriteOutPipe;
        private static string audioWriteFfmpegServerOutToDotnetClientInPipe = "audioWriteFfmpegServerOutToDotnetClientInPipe";
        private static NamedPipeServerStream ffmpegServerWriteOutPipe;
        private static NamedPipeClientStream dotnetClientReadInFromFfmpegServerWriteOutPipe;
        private static Task ffmpegTask;
        private static Task dotnetTask;

        public async Task ProcessAudioBuffer(byte[] buffer, BufferPosition position, string mimeType)
        {
            if (position == BufferPosition.First)
            {
                await OpenPipes();
                ffmpegTask = StartFFMpegProcess(mimeType);
                dotnetTask = DotnetClientReadInFromFfmpegWriteServerOutPipe();
            }

            await WriteToDotnetServerOutPipe(buffer);
            if (position == BufferPosition.Last)
            {
                await ClosePipes();
            }

            await Clients.Caller.ReceiveMessage(Random.Shared.NextInt64().ToString());
        }

        private async Task OpenPipes()
        {
            dotnetServerWriteOutPipe = new NamedPipeServerStream(audioWriteDotnetServerOutToFfmpegClientInPipe, PipeDirection.Out);
            ffmpegClientReadInFromDotnetServerWriteOutPipe = new NamedPipeClientStream(".", audioWriteDotnetServerOutToFfmpegClientInPipe, PipeDirection.In);
            ffmpegServerWriteOutPipe = new NamedPipeServerStream(audioWriteFfmpegServerOutToDotnetClientInPipe, PipeDirection.Out);
            dotnetClientReadInFromFfmpegServerWriteOutPipe = new NamedPipeClientStream(".", audioWriteFfmpegServerOutToDotnetClientInPipe, PipeDirection.In);
            var dotnetServerWriteOutPipeWaitForConnectionAsync = dotnetServerWriteOutPipe.WaitForConnectionAsync();
            var ffmpegServerWriteOutPipeWaitForConnectionAsync = ffmpegServerWriteOutPipe.WaitForConnectionAsync();
            await Task.WhenAll(ffmpegClientReadInFromDotnetServerWriteOutPipe.ConnectAsync(), dotnetClientReadInFromFfmpegServerWriteOutPipe.ConnectAsync());
            await Task.WhenAll(dotnetServerWriteOutPipeWaitForConnectionAsync, ffmpegServerWriteOutPipeWaitForConnectionAsync);
        }

        private Task StartFFMpegProcess(string mimeType) =>
            FFMpegArguments
                .FromPipeInput(new StreamPipeSource(ffmpegClientReadInFromDotnetServerWriteOutPipe), options => options
                    .ForceFormat(mimeType.Substring(6))
                    .UsingMultithreading(false)
                    .WithHardwareAcceleration())
                .OutputToPipe(new StreamPipeSink(ffmpegServerWriteOutPipe), options => options
                    .OverwriteExisting()
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
            await dotnetServerWriteOutPipe.WriteAsync(buffer, 0, buffer.Length);
        }

        private async Task ClosePipes()
        {
            dotnetServerWriteOutPipe.Disconnect();
            await ffmpegTask;
            await dotnetServerWriteOutPipe.DisposeAsync();
            await ffmpegClientReadInFromDotnetServerWriteOutPipe.DisposeAsync();
            ffmpegServerWriteOutPipe.Disconnect();
            await dotnetTask;
            await ffmpegServerWriteOutPipe.DisposeAsync();
            await dotnetClientReadInFromFfmpegServerWriteOutPipe.DisposeAsync();
        }

        private async Task DotnetClientReadInFromFfmpegWriteServerOutPipe()
        {
            int bytesRead;
            var buffer = new byte[4096];
            var localFileName = "Files/temp.wav";
            if (File.Exists(localFileName))
                File.Delete(localFileName);

            if (!Directory.Exists("Files"))
                Directory.CreateDirectory("Files");

            await using var fileStream = File.OpenWrite(localFileName);
            while ((bytesRead = await dotnetClientReadInFromFfmpegServerWriteOutPipe.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                fileStream.Write(buffer, 0, bytesRead);
            }

            await fileStream.FlushAsync();
        }
    }
}
