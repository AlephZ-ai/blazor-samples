using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Mark
{
    public record struct SharedMark
    {
        public const string NAME = "name";

        [Required(AllowEmptyStrings = false)]
        [JsonPropertyOrder(1)]
        [JsonPropertyName(NAME)]
        public required string Name { get; init; }
    }
}
