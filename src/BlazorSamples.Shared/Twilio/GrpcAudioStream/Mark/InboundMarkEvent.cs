using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Converters;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Mark
{
    public class InboundMarkEvent : InboundEvent, ISequenceEvent
    {
        public const string EVENT_TYPE = OutboundMarkEvent.EVENT_TYPE;
        public const string MARK = EVENT_TYPE;
        
        [Required]
        [JsonPropertyName(IStreamSidEvent.STREAM_SID)]
        public required string StreamSid { get; init; }

        [Required]
        [JsonPropertyName(ISequenceEvent.SEQUENCE_NUMBER)]
        [JsonConverter(typeof(JsonStringToUIntConverter))]
        public required uint SequenceNumber { get; init; }

        [Required]
        [JsonPropertyName(MARK)]
        public required SharedMark Mark { get; init; }
        public InboundMarkEvent()
        {
            Direction = EventDirection.Inbound;
        }

        internal override T RunProcessor<T>(IInboundEventProcessor<T> processor) => processor.Handle(this);
    }
}
