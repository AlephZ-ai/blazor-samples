﻿using BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions;using System;using System.Collections.Generic;using System.ComponentModel.DataAnnotations;using System.Linq;using System.Text;using System.Text.Json.Serialization;using System.Threading.Tasks;namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Connected{    public class InboundConnectedEvent : InboundEvent    {        public const string EVENT_TYPE = "connected";        public const string PROTOCOL = "protocol";        public const string VERSION = "version";        [Required(AllowEmptyStrings = false)]        [JsonPropertyName(PROTOCOL)]        public required string Protocol { get; init; }        [Required(AllowEmptyStrings = false)]        [JsonPropertyName(VERSION)]        public required string Version { get; init; }        public InboundConnectedEvent()        {            Direction = EventDirection.Inbound;        }        internal override T RunProcessor<T>(IInboundEventProcessor<T> processor) => processor.Handle(this);    }}