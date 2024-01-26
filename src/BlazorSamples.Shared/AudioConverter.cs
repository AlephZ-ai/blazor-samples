
using BlazorSamples.Shared;
using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using System.Collections;
using System.IO.Pipes;
using static Google.Protobuf.WireFormat;

namespace BlazorSamples.Shared
{
    public class AudioConverter : IAudioConverter
    {
        private NamedPipeServerStream dotnetServerWriteOutPipe = null!;
        private NamedPipeClientStream ffmpegClientReadInFromDotnetServerWriteOutPipe = null!;

        private NamedPipeServerStream ffmpegServerWriteOutPipe = null!;
        private NamedPipeClientStream dotnetClientReadInFromFfmpegServerWriteOutPipe = null!;
        private Task ffmpegTask = null!;

        public async Task InitializationAsync()
        {
            await OpenPipes().ConfigureAwait(false);
            ffmpegTask = StartFFMpegProcess();
        }

        public async IAsyncEnumerable<byte[]> ProcessAudioBuffer(byte[] buffer)
        {
            await WriteToDotnetServerOutPipe(buffer).ConfigureAwait(false);
            await foreach (var converted in DotnetClientReadInFromFfmpegWriteServerOutPipe())
            {
                yield return converted;
            }
        }

        private async Task OpenPipes()
        {
            var audioWriteDotnetServerOutToFfmpegClientInPipe = Guid.NewGuid().ToString();
            var audioWriteFfmpegServerOutToDotnetClientInPipe = Guid.NewGuid().ToString();
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

        public async Task ClosePipes()
        {
            dotnetServerWriteOutPipe.Disconnect();
            await ffmpegTask.ConfigureAwait(false);
            await dotnetServerWriteOutPipe.DisposeAsync().ConfigureAwait(false);
            await ffmpegClientReadInFromDotnetServerWriteOutPipe.DisposeAsync().ConfigureAwait(false);
            ffmpegServerWriteOutPipe.Disconnect();
            await ffmpegServerWriteOutPipe.DisposeAsync().ConfigureAwait(false);
            await dotnetClientReadInFromFfmpegServerWriteOutPipe.DisposeAsync().ConfigureAwait(false);
        }

        private Task StartFFMpegProcess() =>
            FFMpegArguments
                .FromPipeInput(new StreamPipeSource(ffmpegClientReadInFromDotnetServerWriteOutPipe), options => options
                    .ForceFormat("mulaw"))
                .OutputToPipe(new StreamPipeSink(ffmpegServerWriteOutPipe), options => options
                    .ForceFormat("wav")
                    .WithAudioSamplingRate(16000)
                    .WithAudioBitrate(128)
                    .WithSpeedPreset(Speed.UltraFast)
                    .WithCustomArgument("-ac 1"))
                .ProcessAsynchronously();

        private async Task WriteToDotnetServerOutPipe(byte[] buffer)
        {
            await dotnetServerWriteOutPipe.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
        }

        private async IAsyncEnumerable<byte[]> DotnetClientReadInFromFfmpegWriteServerOutPipe()
        {
            int bytesRead;
            var buffer = new byte[4096];
            while ((bytesRead = await dotnetClientReadInFromFfmpegServerWriteOutPipe.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                yield return buffer[..bytesRead];
            }
        }
    }
}
