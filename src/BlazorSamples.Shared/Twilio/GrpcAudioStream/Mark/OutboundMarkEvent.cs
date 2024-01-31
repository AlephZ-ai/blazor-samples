using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Mark
{
    public class OutboundMarkEvent : StreamSidEvent, IOutboundEvent
    {
        public const string EVENT_TYPE = "mark";
        public const string MARK = EVENT_TYPE;


        [Required]
        [JsonPropertyName(MARK)]
        public required SharedMark Mark { get; init; }
        public OutboundMarkEvent()
        {
            EventType = EVENT_TYPE;
            Direction = EventDirection.Outbound;
        }
    }
}
