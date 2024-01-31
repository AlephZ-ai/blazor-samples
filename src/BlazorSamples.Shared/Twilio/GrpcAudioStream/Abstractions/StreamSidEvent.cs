﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions
{
    public abstract class StreamSidEvent : Event
    {
        public const string STREAM_SID = "streamSid";

        [Required]
        [JsonPropertyName(STREAM_SID)]
        public required string StreamSid { get; init; }
    }
}
