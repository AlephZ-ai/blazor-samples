﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions
{
    public interface IMedia
    {
        public const string EVENT_TYPE = "media";
        public const string MEDIA = EVENT_TYPE;
        public const string PAYLOAD = "payload";

        [Required(AllowEmptyStrings = false)]
        [JsonPropertyOrder(99)]
        [JsonPropertyName(PAYLOAD)]
        public byte[] Payload { get; }
    }
}
