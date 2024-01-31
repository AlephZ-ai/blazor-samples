using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Stop
{
    public class InboundStop
    {
        public const string ACCOUNT_SID = "accountSid";
        public const string CALL_SID = "callSid";

        [Required(AllowEmptyStrings = false)]
        [JsonPropertyName(ACCOUNT_SID)]
        public required string AccountSid { get; init; }

        [Required(AllowEmptyStrings = false)]
        [JsonPropertyName(CALL_SID)]
        public required string CallSid { get; init; }
    }
}
