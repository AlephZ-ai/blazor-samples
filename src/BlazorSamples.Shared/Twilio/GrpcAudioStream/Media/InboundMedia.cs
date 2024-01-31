using BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Media
{
    public class InboundMedia : Abstractions.Media
    {
        public const string TRACK = "track";
        public const string CHUNK = "chunk";
        public const string TIMESTAMP = "timestamp";

        [Required(AllowEmptyStrings = false)]
        [JsonPropertyOrder(1)]
        [JsonPropertyName(TRACK)]
        public required string Track { get; init; }

        [Required(AllowEmptyStrings = false)]
        [JsonPropertyOrder(2)]
        [JsonPropertyName(CHUNK)]
        [JsonConverter(typeof(JsonStringToUIntConverter))]
        public required uint Chunk { get; init; }

        [Required(AllowEmptyStrings = false)]
        [JsonPropertyOrder(3)]
        [JsonPropertyName(TIMESTAMP)]
        [JsonConverter(typeof(JsonStringToTimeSpanConverter))]
        public required TimeSpan Timestamp { get; init; }
    }
}
