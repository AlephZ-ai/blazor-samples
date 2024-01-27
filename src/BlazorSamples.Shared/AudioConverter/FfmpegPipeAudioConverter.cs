using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using System.Collections;
using System.IO.Pipes;
using static Google.Protobuf.WireFormat;

namespace BlazorSamples.Shared.AudioConverter
{
    public class FfmpegPipeAudioConverter : IAudioConverter
    {
        private NamedPipeServerStream dotnetServerWriteOutPipe = null!;
        private NamedPipeClientStream ffmpegClientReadInFromDotnetServerWriteOutPipe = null!;

        private NamedPipeServerStream ffmpegServerWriteOutPipe = null!;
        private NamedPipeClientStream dotnetClientReadInFromFfmpegServerWriteOutPipe = null!;
        private Task ffmpegTask = null!;
        private Task dotnetTask = null!;
        private CancellationToken ct = default;

        public async Task InitializationAsync(Func<byte[], Task> processConvertedAudioBuffer, CancellationToken ct)
        {
            this.ct = ct;
            await OpenPipes().ConfigureAwait(false);
            ffmpegTask = StartFFMpegProcess();
            dotnetTask = DotnetClientReadInFromFfmpegWriteServerOutPipe(processConvertedAudioBuffer);
        }

        public Task ProcessAudioBuffer(byte[] buffer)
        {
            return WriteToDotnetServerOutPipe(buffer);
        }

        private async Task OpenPipes()
        {
            var audioWriteDotnetServerOutToFfmpegClientInPipe = Guid.NewGuid().ToString();
            var audioWriteFfmpegServerOutToDotnetClientInPipe = Guid.NewGuid().ToString();
            dotnetServerWriteOutPipe =
                new NamedPipeServerStream(audioWriteDotnetServerOutToFfmpegClientInPipe, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.WriteThrough);
            ffmpegClientReadInFromDotnetServerWriteOutPipe = new NamedPipeClientStream(".",
                audioWriteDotnetServerOutToFfmpegClientInPipe, PipeDirection.In, PipeOptions.Asynchronous | PipeOptions.WriteThrough);
            ffmpegServerWriteOutPipe =
                new NamedPipeServerStream(audioWriteFfmpegServerOutToDotnetClientInPipe, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.WriteThrough);
            dotnetClientReadInFromFfmpegServerWriteOutPipe = new NamedPipeClientStream(".",
                audioWriteFfmpegServerOutToDotnetClientInPipe, PipeDirection.In, PipeOptions.Asynchronous | PipeOptions.WriteThrough);
            var dotnetServerWriteOutPipeWaitForConnectionAsync = dotnetServerWriteOutPipe.WaitForConnectionAsync(ct);
            var ffmpegServerWriteOutPipeWaitForConnectionAsync = ffmpegServerWriteOutPipe.WaitForConnectionAsync(ct);
            await Task.WhenAll(ffmpegClientReadInFromDotnetServerWriteOutPipe.ConnectAsync(ct),
                dotnetClientReadInFromFfmpegServerWriteOutPipe.ConnectAsync(ct)).ConfigureAwait(false);
            await Task.WhenAll(dotnetServerWriteOutPipeWaitForConnectionAsync,
                ffmpegServerWriteOutPipeWaitForConnectionAsync).ConfigureAwait(false);
        }

        public async Task ClosePipes()
        {
            dotnetServerWriteOutPipe.Disconnect();
            await ffmpegTask.ConfigureAwait(false);
            ffmpegTask = null!;
            await dotnetServerWriteOutPipe.DisposeAsync().ConfigureAwait(false);
            await ffmpegClientReadInFromDotnetServerWriteOutPipe.DisposeAsync().ConfigureAwait(false);
            ffmpegServerWriteOutPipe.Disconnect();
            await dotnetTask.ConfigureAwait(false);
            dotnetTask = null!;
            await ffmpegServerWriteOutPipe.DisposeAsync().ConfigureAwait(false);
            await dotnetClientReadInFromFfmpegServerWriteOutPipe.DisposeAsync().ConfigureAwait(false);
        }

        private Task StartFFMpegProcess() =>
            FFMpegArguments
                .FromPipeInput(new StreamPipeSource(ffmpegClientReadInFromDotnetServerWriteOutPipe), options => options
                    .ForceFormat("mulaw")
                    .WithAudioSamplingRate(8000)
                    .WithHardwareAcceleration())
                .OutputToPipe(new StreamPipeSink(ffmpegServerWriteOutPipe), options => options
                    .ForceFormat("wav")
                    .WithAudioSamplingRate(16000)
                    .WithAudioBitrate(64)
                    .WithSpeedPreset(Speed.UltraFast))
                .ProcessAsynchronously(true);

        private async Task WriteToDotnetServerOutPipe(byte[] buffer)
        {
            await dotnetServerWriteOutPipe.WriteAsync(buffer, 0, buffer.Length, ct).ConfigureAwait(false);
        }

        private async Task DotnetClientReadInFromFfmpegWriteServerOutPipe(Func<byte[], Task> processConvertedAudioBuffer)
        {
            int bytesRead;
            var buffer = new byte[32 * 1024];
            while ((bytesRead = await dotnetClientReadInFromFfmpegServerWriteOutPipe.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
            {
                await processConvertedAudioBuffer(buffer[..bytesRead]).ConfigureAwait(false);
            }
        }
    }
}
