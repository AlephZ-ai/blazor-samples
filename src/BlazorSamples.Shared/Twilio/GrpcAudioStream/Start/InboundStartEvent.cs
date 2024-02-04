using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Converters;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Start
{
    public class InboundStartEvent : InboundEvent, ISequenceEvent
    {
        public const string EVENT_TYPE = "start";
        public const string START = EVENT_TYPE;

        [Required]
        [JsonPropertyName(IStreamSidEvent.STREAM_SID)]
        public required string StreamSid { get; init; }

        [Required]
        [JsonPropertyName(ISequenceEvent.SEQUENCE_NUMBER)]
        [JsonConverter(typeof(JsonStringToUIntConverter))]
        public required uint SequenceNumber { get; init; }

        [Required]
        [JsonPropertyName(START)]
        public required InboundStart Start { get; init; }
        public InboundStartEvent()
        {
            Direction = EventDirection.Inbound;
        }

        internal override T RunProcessor<T>(IInboundEventProcessor<T> processor) => processor.Handle(this);
    }
}
