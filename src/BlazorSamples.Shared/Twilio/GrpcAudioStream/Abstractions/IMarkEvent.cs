using BlazorSamples.Shared.Twilio.GrpcAudioStream.Mark;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions
{
    public interface IMarkEvent
    {
        public const string EVENT_TYPE = "mark";
        public const string MARK = EVENT_TYPE;

        [Required]
        [JsonPropertyOrder(101)]
        [JsonPropertyName(MARK)]
        public SharedMark Mark { get; }
    }
}
