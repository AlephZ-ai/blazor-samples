using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Clear;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Connected;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Mark;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Media;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Start;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Stop;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = EVENT)]
    public abstract class Event : IEvent
    {
        public const string EVENT = "event";
        public const string UNKNOWN = "unknown";

        [Required(AllowEmptyStrings = false)]
        [JsonPropertyName(EVENT)]
        public string EventType { get; init; } = UNKNOWN;

        [Required]
        [JsonIgnore]
        public EventDirection Direction { get; init; } = EventDirection.Unknown;
    }
}
