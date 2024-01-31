using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Start
{
    public record struct InboundStartEvent : ISequenceEvent, IStreamSidEvent
    {
        public const string EVENT_TYPE = "start";
        public const string START = EVENT_TYPE;

        public string EventType => EVENT_TYPE;
        public EventDirection Direction => EventDirection.Inbound;
        public required uint SequenceNumber { get; init; }
        public required string StreamSid { get; init; }

        [Required]
        [JsonPropertyOrder(101)]
        [JsonPropertyName(START)]
        public required InboundStart Start { get; init; }
    }
}
