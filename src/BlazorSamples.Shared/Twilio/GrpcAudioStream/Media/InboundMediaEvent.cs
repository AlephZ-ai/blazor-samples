using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Converters;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Media
{
    public class InboundMediaEvent : InboundEvent, ISequenceEvent
    {
        public const string EVENT_TYPE = Abstractions.Media.EVENT_TYPE;
        public const string MEDIA = EVENT_TYPE;

        [Required]
        [JsonPropertyName(IStreamSidEvent.STREAM_SID)]
        public required string StreamSid { get; init; }

        [Required]
        [JsonPropertyName(ISequenceEvent.SEQUENCE_NUMBER)]
        [JsonConverter(typeof(JsonStringToUIntConverter))]
        public required uint SequenceNumber { get; init; }

        [Required]
        [JsonPropertyName(MEDIA)]
        public required InboundMedia Media { get; init; }
        public InboundMediaEvent()
        {
            Direction = EventDirection.Inbound;
        }

        internal override T RunProcessor<T>(IInboundEventProcessor<T> processor) => processor.Handle(this);
    }
}
