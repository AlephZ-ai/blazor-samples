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
    public class FfmpegAudioConverter(FfmpegAudioConverterOptions options) : IAudioConverter
    {
        public IAsyncEnumerable<ReadOnlyMemory<byte>> ConvertAsync(IAsyncEnumerable<ReadOnlyMemory<byte>> source, CancellationToken ct = default)
        {
            var pipes = new BiDirectionalNamedPipes(options.InPipeName, options.OutPipeName, options.InitialBufferSize);
            var ffOptions = new FFOptions();
            var process = ConvertAudioAsync(pipes, ffOptions);
            return pipes.ProcessAllAsync(source, process, ct)
                .Where(_ => !ct.IsCancellationRequested)
                .Finally(() => pipes.DisposeAsync().AsTask());
        }

        private Task<bool> ConvertAudioAsync(BiDirectionalNamedPipes pipes, FFOptions ffOptions) =>
            FFMpegArguments
                .FromPipeInput(new StreamPipeSource(pipes.In.Client), i => i
                    .ForceFormat(options.InFormat)
                    .WithAudioSamplingRate(options.InSampleRate)
                    .WithHardwareAcceleration()
                    .UsingMultithreading(options.InMultithreading))
                .OutputToPipe(new StreamPipeSink(pipes.Out.Server), o => o
                    .ForceFormat(options.OutFormat)
                    .WithAudioSamplingRate(options.OutSampleRate)
                    .WithAudioBitrate(options.OutBitrate)
                    .WithSpeedPreset(options.OutSpeed)
                    .WithFastStart()
                    .UsingMultithreading(options.OutMultithreading))
                .ProcessAsynchronously(true, ffOptions);
    }
}
