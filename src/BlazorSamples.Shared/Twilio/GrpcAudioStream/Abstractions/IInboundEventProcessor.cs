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
    public interface IInboundEventProcessor<T>
    {
        internal protected Task<T> HandleAsync(InboundConnectedEvent connectedEvent);
        internal protected Task<T> HandleAsync(InboundStartEvent startEvent);
        internal protected Task<T> HandleAsync(InboundMediaEvent mediaEvent);
        internal protected Task<T> HandleAsync(InboundStopEvent stopEvent);
        internal protected Task<T> HandleAsync(InboundMarkEvent markEvent);
        protected static Task<T> DefaultProcessEventAsync(IInboundEventProcessor<T> processor, IInboundEvent inboundEvent) => inboundEvent.RunProcessorAsync(processor);
        public Task<T> ProcessEventAsync(IInboundEvent inboundEvent) => IInboundEventProcessor<T>.DefaultProcessEventAsync(this, inboundEvent);
    }
}
