using BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Media
{
    public record struct InboundMedia : IMedia
    {
        public const string TRACK = "track";
        public const string CHUNK = "chunk";
        public const string TIMESTAMP = "timestamp";

        public required byte[] Payload { get; init; }

        [Required(AllowEmptyStrings = false)]
        [JsonPropertyOrder(1)]
        [JsonPropertyName(TRACK)]
        public required string Track { get; init; }

        [Required(AllowEmptyStrings = false)]
        [JsonPropertyOrder(2)]
        [JsonPropertyName(CHUNK)]
        public required uint Chunk { get; init; }

        [Required(AllowEmptyStrings = false)]
        [JsonPropertyOrder(3)]
        [JsonPropertyName(TIMESTAMP)]
        public required ulong Timestamp { get; init; }
    }
}
