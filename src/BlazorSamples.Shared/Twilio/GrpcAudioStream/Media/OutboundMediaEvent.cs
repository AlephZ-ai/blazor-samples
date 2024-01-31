using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Media
{
    public class OutboundMediaEvent : StreamSidEvent, IOutboundEvent
    {
        public const string EVENT_TYPE = Abstractions.Media.EVENT_TYPE;
        public const string MEDIA = EVENT_TYPE;

        [Required]
        [JsonPropertyName(MEDIA)]
        public required OutboundMedia Media { get; init; }
        public OutboundMediaEvent()
        {
            EventType = EVENT_TYPE;
            Direction = EventDirection.Outbound;
        }
    }
}
