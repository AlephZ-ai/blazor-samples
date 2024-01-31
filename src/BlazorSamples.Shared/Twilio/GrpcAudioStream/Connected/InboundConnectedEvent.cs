using BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions;
using BlazorSamples.Shared.TypeChat;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Connected
{
    public record struct InboundConnectedEvent : IEvent
    {
        public const string EVENT_TYPE = "connected";
        public const string PROTOCOL = "protocol";
        public const string VERSION = "version";

        public string EventType => EVENT_TYPE;
        public EventDirection Direction => EventDirection.Inbound;

        [Required(AllowEmptyStrings = false)]
        [JsonPropertyOrder(101)]
        [JsonPropertyName(PROTOCOL)]
        public required string Protocol { get; init; }

        [Required(AllowEmptyStrings = false)]
        [JsonPropertyOrder(102)]
        [JsonPropertyName(VERSION)]
        public required string Version { get; init; }
    }
}
