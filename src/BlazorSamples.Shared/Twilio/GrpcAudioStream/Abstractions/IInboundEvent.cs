using BlazorSamples.Shared.Twilio.GrpcAudioStream.Connected;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Mark;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Media;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Start;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Stop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions
{
    [JsonDerivedType(typeof(InboundConnectedEvent), InboundConnectedEvent.EVENT_TYPE)]
    [JsonDerivedType(typeof(InboundStartEvent), InboundStartEvent.EVENT_TYPE)]
    [JsonDerivedType(typeof(InboundMediaEvent), InboundMediaEvent.EVENT_TYPE)]
    [JsonDerivedType(typeof(InboundStopEvent), InboundStopEvent.EVENT_TYPE)]
    [JsonDerivedType(typeof(InboundMarkEvent), InboundMarkEvent.EVENT_TYPE)]
    public interface IInboundEvent : IEvent { }
}
