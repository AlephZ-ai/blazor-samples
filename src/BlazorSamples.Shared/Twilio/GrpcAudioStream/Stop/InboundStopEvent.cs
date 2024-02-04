using BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Converters;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Stop
{
    public class InboundStopEvent : InboundEvent, ISequenceEvent
    {
        public const string EVENT_TYPE = "stop";
        public const string STOP = EVENT_TYPE;

        [Required]
        [JsonPropertyName(IStreamSidEvent.STREAM_SID)]
        public required string StreamSid { get; init; }

        [Required]
        [JsonPropertyName(ISequenceEvent.SEQUENCE_NUMBER)]
        [JsonConverter(typeof(JsonStringToUIntConverter))]
        public required uint SequenceNumber { get; init; }

        [Required]
        [JsonPropertyName(STOP)]
        public required InboundStop Stop { get; init; }
        public InboundStopEvent()
        {
            Direction = EventDirection.Inbound;
        }

        internal override T RunProcessor<T>(IInboundEventProcessor<T> processor) => processor.Handle(this);
    }
}
