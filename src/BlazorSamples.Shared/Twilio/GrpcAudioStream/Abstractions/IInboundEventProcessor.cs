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
        internal protected T Handle(InboundConnectedEvent connectedEvent);
        internal protected T Handle(InboundStartEvent startEvent);
        internal protected T Handle(InboundMediaEvent mediaEvent);
        internal protected T Handle(InboundStopEvent stopEvent);
        internal protected T Handle(InboundMarkEvent markEvent);
        protected static T DefaultProcessEvent(IInboundEventProcessor<T> processor, IInboundEvent inboundEvent) => inboundEvent.RunProcessor(processor);
        public T ProcessEvent(IInboundEvent inboundEvent) => IInboundEventProcessor<T>.DefaultProcessEvent(this, inboundEvent);
    }
}
