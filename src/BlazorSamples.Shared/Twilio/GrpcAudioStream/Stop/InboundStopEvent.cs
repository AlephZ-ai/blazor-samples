using BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions;
using BlazorSamples.Shared.TwilioOld;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Stop
{
    public record struct InboundStopEvent : ISequenceEvent, IStreamSidEvent
    {
        public const string EVENT_TYPE = "stop";
        public const string STOP = EVENT_TYPE;

        public string EventType => EVENT_TYPE;
        public EventDirection Direction => EventDirection.Inbound;
        public required int SequenceNumber { get; init; }
        public required string StreamSid { get; init; }

        [Required]
        [JsonPropertyOrder(101)]
        [JsonPropertyName(STOP)]
        public required InboundStop Stop { get; init; }
    }
}
