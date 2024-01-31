using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions
{
    public enum EventDirection : byte
    {
        Unknown = 0,
        Inbound = 1,
        Outbound = 2
    }
}
