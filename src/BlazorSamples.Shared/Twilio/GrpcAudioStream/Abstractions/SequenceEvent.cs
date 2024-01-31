using BlazorSamples.Shared.Twilio.GrpcAudioStream.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions
{
    public abstract class SequenceEvent : StreamSidEvent
    {
        public const string SEQUENCE_NUMBER = "sequenceNumber";

        [Required]
        [JsonPropertyOrder(11)]
        [JsonPropertyName(SEQUENCE_NUMBER)]
        [JsonConverter(typeof(JsonStringToUIntConverter))]
        public required uint SequenceNumber { get; init; }
    }
}
