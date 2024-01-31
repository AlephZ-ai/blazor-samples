using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Clear
{
    public class OutboundClearEvent : StreamSidEvent, IOutboundEvent
    {
        public const string EVENT_TYPE = "clear";
        public OutboundClearEvent()
        {
            EventType = EVENT_TYPE;
            Direction = EventDirection.Outbound;
        }
    }
}
