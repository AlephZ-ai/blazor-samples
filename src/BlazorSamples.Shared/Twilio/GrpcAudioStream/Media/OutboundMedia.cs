using BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Media
{
    public record struct OutboundMedia : IMedia
    {
        public required byte[] Payload { get; init; }
    }
}
