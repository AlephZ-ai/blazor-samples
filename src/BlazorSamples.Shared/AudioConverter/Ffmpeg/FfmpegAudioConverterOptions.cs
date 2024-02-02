using FFMpegCore.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.AudioConverter.Ffmpeg
{
    public class FfmpegAudioConverterOptions
    {
        private const int DEFAULT_INITIAL_BUFFER_SIZE = 4 * 1024;
        private const string DEFAULT_IN_FORMAT = "mulaw";
        private const int DEFAULT_IN_SAMPLE_RATE = 8000;
        private const bool DEFAULT_IN_MULTITHREADING = false;
        private const string DEFAULT_OUT_FORMAT = "wav";
        private const Speed DEFAULT_OUT_SPEED = Speed.UltraFast;
        private const int DEFAULT_OUT_SAMPLE_RATE = 16000;
        private const int DEFAULT_OUT_BITRATE = 256;
        private const bool DEFAULT_OUT_MULTITHREADING = false;
        private static readonly FfmpegAudioConverterOptions _default = new FfmpegAudioConverterOptions();
        public string InPipeName { get; init; } = Guid.NewGuid().ToString();
        public string InFormat { get; init; } = DEFAULT_IN_FORMAT;
        public int InSampleRate { get; init; } = DEFAULT_IN_SAMPLE_RATE;
        public bool InMultithreading { get; init; } = DEFAULT_IN_MULTITHREADING;
        public string OutPipeName { get; init; } = Guid.NewGuid().ToString();
        public string OutFormat { get; init; } = DEFAULT_OUT_FORMAT;
        public Speed OutSpeed { get; init; } = DEFAULT_OUT_SPEED;
        public int OutSampleRate { get; init; } = DEFAULT_OUT_SAMPLE_RATE;
        public int OutBitrate { get; init; } = DEFAULT_OUT_BITRATE;
        public bool OutMultithreading { get; init; } = DEFAULT_OUT_MULTITHREADING;
        public int InitialBufferSize { get; init; } = DEFAULT_INITIAL_BUFFER_SIZE;
        public static FfmpegAudioConverterOptions Default => _default;
    }
}
