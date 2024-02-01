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

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream
{
    public class DefaultTwilioProcessor : IInboundEventProcessor<DefaultTwilioProcessorResult?>
    {
        private DefaultTwilioProcessorResult? _previous;
        public DefaultTwilioProcessorResult? ProcessEvent(IInboundEvent inboundEvent) => IInboundEventProcessor<DefaultTwilioProcessorResult?>.DefaultProcessEvent(this, inboundEvent);
        DefaultTwilioProcessorResult? IInboundEventProcessor<DefaultTwilioProcessorResult?>.Handle(InboundConnectedEvent connectedEvent) => DefaultTwilioProcessorResult.From(connectedEvent);
        DefaultTwilioProcessorResult? IInboundEventProcessor<DefaultTwilioProcessorResult?>.Handle(InboundStartEvent startEvent) => _previous = DefaultTwilioProcessorResult.From(startEvent);
        DefaultTwilioProcessorResult? IInboundEventProcessor<DefaultTwilioProcessorResult?>.Handle(InboundMediaEvent mediaEvent) => DefaultTwilioProcessorResult.From(_previous!, mediaEvent);
        DefaultTwilioProcessorResult? IInboundEventProcessor<DefaultTwilioProcessorResult?>.Handle(InboundStopEvent stopEvent) => DefaultTwilioProcessorResult.From(stopEvent);
        DefaultTwilioProcessorResult? IInboundEventProcessor<DefaultTwilioProcessorResult?>.Handle(InboundMarkEvent markEvent) => DefaultTwilioProcessorResult.From(markEvent);
    }
}
