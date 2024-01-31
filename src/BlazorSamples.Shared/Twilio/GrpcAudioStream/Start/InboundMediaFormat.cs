using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Start
{
    public class InboundMediaFormat
    {
        public const string ENCODING = "encoding";
        public const string SAMPLE_RATE = "sampleRate";
        public const string CHANNELS = "channels";

        public const string DEFAULT_ENCODING = "audio/x-mulaw";
        public const int DEFAULT_SAMPLE_RATE = 8000;
        public const int DEFAULT_CHANNELS = 1;

        [Required(AllowEmptyStrings = false)]
        [JsonPropertyName(ENCODING)]
        public string Encoding { get; init; }

        [Required]
        [JsonPropertyName(SAMPLE_RATE)]
        public ushort SampleRate { get; init; }

        [Required]
        [JsonPropertyName(CHANNELS)]
        public byte Channels { get; init; }

        public InboundMediaFormat()
        {
            Encoding = DEFAULT_ENCODING;
            SampleRate = DEFAULT_SAMPLE_RATE;
            Channels = DEFAULT_CHANNELS;
        }
    }
}
