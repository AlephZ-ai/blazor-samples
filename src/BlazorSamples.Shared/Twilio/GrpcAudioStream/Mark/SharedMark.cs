using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Mark
{
    public class SharedMark
    {
        public const string NAME = "name";

        [Required(AllowEmptyStrings = false)]
        [JsonPropertyName(NAME)]
        public required string Name { get; init; }
    }
}
