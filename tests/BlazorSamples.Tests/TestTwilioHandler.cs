using BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Connected;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Mark;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Media;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Start;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Stop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSamples.Tests
{
    internal sealed class TestTwilioHandler : IInboundEventProcessor<string?>
    {
        public string? Handle(InboundConnectedEvent connectedEvent) => connectedEvent.EventType;
        public string? Handle(InboundStartEvent startEvent) => startEvent.EventType;
        public string? Handle(InboundMediaEvent mediaEvent) => mediaEvent.EventType;
        public string? Handle(InboundStopEvent stopEvent) => stopEvent.EventType;
        public string? Handle(InboundMarkEvent markEvent) => markEvent.EventType;
        public string? ProcessEvent(IInboundEvent inboundEvent) => IInboundEventProcessor<string?>.DefaultProcessEvent(this, inboundEvent);
    }
}
