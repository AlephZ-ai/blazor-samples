using BlazorSamples.Shared.Twilio.GrpcAudioStream.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions
{
    public interface ISequenceEvent : IStreamSidEvent
    {
        public const string SEQUENCE_NUMBER = "sequenceNumber";

        public uint SequenceNumber { get; init; }
    }
}
