using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Clear;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Connected;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Mark;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Media;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Start;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Stop;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = EVENT)]
    [JsonDerivedType(typeof(InboundConnectedEvent), InboundConnectedEvent.EVENT_TYPE)]
    [JsonDerivedType(typeof(InboundStartEvent), InboundStartEvent.EVENT_TYPE)]
    [JsonDerivedType(typeof(InboundMediaEvent), InboundMediaEvent.EVENT_TYPE)]
    [JsonDerivedType(typeof(InboundStopEvent), InboundStopEvent.EVENT_TYPE)]
    [JsonDerivedType(typeof(InboundMarkEvent), InboundMarkEvent.EVENT_TYPE)]
    [JsonDerivedType(typeof(OutboundMediaEvent), OutboundMediaEvent.EVENT_TYPE)]
    [JsonDerivedType(typeof(OutboundMarkEvent), OutboundMarkEvent.EVENT_TYPE)]
    [JsonDerivedType(typeof(OutboundClearEvent), OutboundClearEvent.EVENT_TYPE)]
    public interface IEvent
    {
        public const string EVENT = "event";

        [Required(AllowEmptyStrings = false)]
        [JsonPropertyOrder(1)]
        [JsonPropertyName(EVENT)]
        public string EventType { get; }

        [Required]
        [JsonIgnore]
        public EventDirection Direction { get; }
    }
}
