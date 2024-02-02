using FFMpegCore.Pipes;
using FFMpegCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFMpegCore.Enums;

namespace BlazorSamples.Shared.AudioConverter.Ffmpeg
{
    public class FfmpegAudioConverter(IOptionsSnapshot<FfmpegAudioConverterOptions> options) : IAudioConverter
    {
        public IAsyncEnumerable<ReadOnlyMemory<byte>> ConvertAsync(IAsyncEnumerable<ReadOnlyMemory<byte>> source, CancellationToken ct = default)
        {
            var pipes = new BiDirectionalNamedPipes(options.Value.InPipeName, options.Value.OutPipeName, options.Value.InitialBufferSize);
            var ffOptions = new FFOptions();
            var process = ConvertAudioAsync(pipes, ffOptions);
            return pipes.ProcessAllAsync(source, process, ct)
                .Finally(() => pipes.DisposeAsync().AsTask());
        }

        private Task<bool> ConvertAudioAsync(BiDirectionalNamedPipes pipes, FFOptions ffOptions) =>
            FFMpegArguments
                .FromPipeInput(new StreamPipeSource(pipes.In.Client), i => i
                    .ForceFormat(options.Value.InFormat)
                    .WithAudioSamplingRate(options.Value.InSampleRate)
                    .WithHardwareAcceleration()
                    .UsingMultithreading(false))
                .OutputToPipe(new StreamPipeSink(pipes.Out.Server), o => o
                    .ForceFormat(options.Value.OutFormat)
                    .WithAudioSamplingRate(options.Value.OutSampleRate)
                    .WithAudioBitrate(options.Value.OutBitrate)
                    .WithSpeedPreset(options.Value.OutSpeed)
                    .WithFastStart()
                    .UsingMultithreading(false))
                .ProcessAsynchronously(true, ffOptions);
    }
}
