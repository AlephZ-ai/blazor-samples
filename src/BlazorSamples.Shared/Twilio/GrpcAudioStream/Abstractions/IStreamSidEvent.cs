using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions
{
    public interface IStreamSidEvent : IEvent
    {
        public const string STREAM_SID = "streamSid";

        public string StreamSid { get; }
    }
}
