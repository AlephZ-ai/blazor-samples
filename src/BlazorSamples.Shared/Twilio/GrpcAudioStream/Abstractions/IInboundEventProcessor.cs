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

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions
{
    public interface IInboundEventProcessor
    {
        internal protected Task HandleAsync(InboundConnectedEvent connectedEvent);
        internal protected Task HandleAsync(InboundStartEvent startEvent);
        internal protected Task HandleAsync(InboundMediaEvent mediaEvent);
        internal protected Task HandleAsync(InboundStopEvent stopEvent);
        internal protected Task HandleAsync(InboundMarkEvent markEvent);
        internal protected Task HandleAsync(IEvent? unknownEvent);
        public Task ProcessEventAsync(IInboundEvent inboundEvent) => inboundEvent.RunProcessorAsync(this);
    }
}
