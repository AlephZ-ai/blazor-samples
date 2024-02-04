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
        public string? Handle(InboundConnectedEvent connectedEvent) => connectedEvent.GetType().Name;
        public string? Handle(InboundStartEvent startEvent) => startEvent.GetType().Name;
        public string? Handle(InboundMediaEvent mediaEvent) => mediaEvent.GetType().Name;
        public string? Handle(InboundStopEvent stopEvent) => stopEvent.GetType().Name;
        public string? Handle(InboundMarkEvent markEvent) => markEvent.GetType().Name;
        public string? ProcessEvent(InboundEvent inboundEvent) => IInboundEventProcessor<string?>.DefaultProcessEvent(this, inboundEvent);
    }
}
