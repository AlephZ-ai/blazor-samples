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
        public Task<string?> HandleAsync(InboundConnectedEvent connectedEvent) => Task.FromResult<string?>(connectedEvent.EventType);
        public Task<string?> HandleAsync(InboundStartEvent startEvent) => Task.FromResult<string?>(startEvent.EventType);
        public Task<string?> HandleAsync(InboundMediaEvent mediaEvent) => Task.FromResult<string?>(mediaEvent.EventType);
        public Task<string?> HandleAsync(InboundStopEvent stopEvent) => Task.FromResult<string?>(stopEvent.EventType);
        public Task<string?> HandleAsync(InboundMarkEvent markEvent) => Task.FromResult<string?>(markEvent.EventType);
        public Task<string?> ProcessEventAsync(IInboundEvent inboundEvent) => IInboundEventProcessor<string?>.DefaultProcessEventAsync(this, inboundEvent);
    }
}
