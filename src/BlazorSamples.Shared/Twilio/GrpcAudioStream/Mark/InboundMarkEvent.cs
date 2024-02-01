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
    public class InboundMarkEvent : SequenceEvent, IInboundEvent
    {
        public const string EVENT_TYPE = OutboundMarkEvent.EVENT_TYPE;
        public const string MARK = EVENT_TYPE;


        [Required]
        [JsonPropertyName(MARK)]
        public required SharedMark Mark { get; init; }
        public InboundMarkEvent()
        {
            EventType = EVENT_TYPE;
            Direction = EventDirection.Inbound;
        }

        T IInboundEvent.RunProcessor<T>(IInboundEventProcessor<T> processor) => processor.Handle(this);
    }
}
