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
    public class InboundStartEvent : SequenceEvent, IInboundEvent
    {
        public const string EVENT_TYPE = "start";
        public const string START = EVENT_TYPE;

        [Required]
        [JsonPropertyOrder(101)]
        [JsonPropertyName(START)]
        public required InboundStart Start { get; init; }
        public InboundStartEvent()
        {
            EventType = EVENT_TYPE;
            Direction = EventDirection.Inbound;
        }
    }
}
