using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Clear
{
    public class OutboundClearEvent : OutboundEvent, IStreamSidEvent
    {
        public const string EVENT_TYPE = "clear";

        [Required]
        [JsonPropertyName(IStreamSidEvent.STREAM_SID)]
        public required string StreamSid { get; init; }

        public OutboundClearEvent()
        {
            Direction = EventDirection.Outbound;
        }
    }
}
