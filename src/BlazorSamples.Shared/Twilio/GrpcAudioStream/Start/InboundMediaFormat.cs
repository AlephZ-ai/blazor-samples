using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Start
{
    public record struct InboundMediaFormat
    {
        public const string ENCODING = "encoding";
        public const string SAMPLE_RATE = "sampleRate";
        public const string CHANNELS = "channels";

        public const string DEFAULT_ENCODING = "audio/x-mulaw";
        public const int DEFAULT_SAMPLE_RATE = 8000;
        public const int DEFAULT_CHANNELS = 1;

        [Required(AllowEmptyStrings = false)]
        [JsonPropertyOrder(1)]
        [JsonPropertyName(ENCODING)]
        public string Encoding { get; init; }

        [Required]
        [JsonPropertyOrder(2)]
        [JsonPropertyName(SAMPLE_RATE)]
        public int SampleRate { get; init; }

        [Required]
        [JsonPropertyOrder(3)]
        [JsonPropertyName(CHANNELS)]
        public int Channels { get; init; }

        public InboundMediaFormat()
        {
            Encoding = DEFAULT_ENCODING;
            SampleRate = DEFAULT_SAMPLE_RATE;
            Channels = DEFAULT_CHANNELS;
        }
    }
}
