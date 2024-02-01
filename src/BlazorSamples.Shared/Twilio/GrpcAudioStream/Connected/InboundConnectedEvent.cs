using BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions;using System;using System.Collections.Generic;using System.ComponentModel.DataAnnotations;using System.Linq;using System.Text;using System.Text.Json.Serialization;using System.Threading.Tasks;namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Connected{    public class InboundConnectedEvent : Event, IInboundEvent    {        public const string EVENT_TYPE = "connected";        public const string PROTOCOL = "protocol";        public const string VERSION = "version";        [Required(AllowEmptyStrings = false)]        [JsonPropertyName(PROTOCOL)]        public required string Protocol { get; init; }        [Required(AllowEmptyStrings = false)]        [JsonPropertyName(VERSION)]        public required string Version { get; init; }        public InboundConnectedEvent()
        {
            EventType = EVENT_TYPE;
            Direction = EventDirection.Inbound;
        }

        Task<T> IInboundEvent.RunProcessorAsync<T>(IInboundEventProcessor<T> processor) => processor.HandleAsync(this);
    }}