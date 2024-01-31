using BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Stop
{
    public class InboundStopEvent : SequenceEvent, IInboundEvent
    {
        public const string EVENT_TYPE = "stop";
        public const string STOP = EVENT_TYPE;

        [Required]
        [JsonPropertyOrder(101)]
        [JsonPropertyName(STOP)]
        public required InboundStop Stop { get; init; }
        public InboundStopEvent()
        {
            EventType = EVENT_TYPE;
            Direction = EventDirection.Inbound;
        }
    }
}
