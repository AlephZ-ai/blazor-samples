﻿using System;using System.Collections.Generic;using System.ComponentModel.DataAnnotations;using System.Linq;using System.Text;using System.Text.Json.Serialization;using System.Threading.Tasks;namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Start{    public class InboundStart    {        public const string ACCOUNT_SID = "accountSid";        public const string STREAM_SID = "streamSid";        public const string CALL_SID = "callSid";        public const string TRACKS = "tracks";        public const string MEDIA_FORMAT = "mediaFormat";        public const string CUSTOM_PARAMETERS = "customParameters";        [Required(AllowEmptyStrings = false)]        [JsonPropertyName(ACCOUNT_SID)]        public required string AccountSid { get; init; }        [Required(AllowEmptyStrings = false)]        [JsonPropertyName(STREAM_SID)]        public required string StreamSid { get; init; }        [Required(AllowEmptyStrings = false)]        [JsonPropertyName(CALL_SID)]        public required string CallSid { get; init; }        [Required]        [JsonPropertyName(TRACKS)]        public required string[] Tracks { get; init; }        [Required]        [JsonPropertyName(MEDIA_FORMAT)]        public InboundMediaFormat MediaFormat { get; init; }        [Required]        [JsonPropertyName(CUSTOM_PARAMETERS)]        public IDictionary<string, string> CustomParameters { get; init; }        public InboundStart()        {            MediaFormat = new InboundMediaFormat();            CustomParameters = new Dictionary<string, string>();        }    }}