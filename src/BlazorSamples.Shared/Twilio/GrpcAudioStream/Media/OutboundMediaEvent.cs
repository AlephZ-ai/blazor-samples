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
    public record struct OutboundMediaEvent : IOutboundEvent, IMediaEvent<OutboundMedia>
    {
        public const string EVENT_TYPE = IMedia.EVENT_TYPE;

        public string EventType => EVENT_TYPE;
        public EventDirection Direction => EventDirection.Outbound;
        public required string StreamSid { get; init; }
        public required OutboundMedia Media { get; init; }
    }
}
