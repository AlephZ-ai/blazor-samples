using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions
{
    public interface ISequenceEvent : IEvent
    {
        public const string SEQUENCE_NUMBER = "sequenceNumber";

        [Required]
        [JsonPropertyOrder(11)]
        [JsonPropertyName(SEQUENCE_NUMBER)]
        public int SequenceNumber { get; }
    }
}
