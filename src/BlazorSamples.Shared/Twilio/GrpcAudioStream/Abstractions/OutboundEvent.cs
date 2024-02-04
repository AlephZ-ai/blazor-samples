using BlazorSamples.Shared.Twilio.GrpcAudioStream.Clear;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Connected;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Mark;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions
{
    [JsonDerivedType(typeof(OutboundMediaEvent), OutboundMediaEvent.EVENT_TYPE)]
    [JsonDerivedType(typeof(OutboundMarkEvent), OutboundMarkEvent.EVENT_TYPE)]
    [JsonDerivedType(typeof(OutboundClearEvent), OutboundClearEvent.EVENT_TYPE)]
    [JsonPolymorphic(TypeDiscriminatorPropertyName = EVENT)]
    public abstract class OutboundEvent : Event { }
}
