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
    public record struct InboundMarkEvent : IMarkEvent, ISequenceEvent
    {
        public const string EVENT_TYPE = IMarkEvent.EVENT_TYPE;

        public string EventType => EVENT_TYPE;
        public EventDirection Direction => EventDirection.Inbound;
        public required int SequenceNumber { get; init; }
        public required SharedMark Mark { get; init; }
    }
}
