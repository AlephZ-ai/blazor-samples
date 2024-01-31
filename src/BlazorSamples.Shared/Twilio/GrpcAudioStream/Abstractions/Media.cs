using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions
{
    public abstract class Media
    {
        public const string EVENT_TYPE = "media";
        public const string MEDIA = EVENT_TYPE;
        public const string PAYLOAD = "payload";

        [Required(AllowEmptyStrings = false)]
        [JsonPropertyName(PAYLOAD)]
        public required byte[] Payload { get; init; }
    }
}
