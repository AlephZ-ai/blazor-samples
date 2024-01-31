using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Clear
{
    public record struct OutboundClearEvent : IStreamSidEvent
    {
        public const string EVENT_TYPE = "clear";

        public string EventType => EVENT_TYPE;
        public EventDirection Direction => EventDirection.Outbound;
        public required string StreamSid { get; init; }

    }
}
