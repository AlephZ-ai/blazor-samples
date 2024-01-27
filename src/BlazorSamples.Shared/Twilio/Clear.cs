using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.Twilio
{
    public class Clear
    {
        public string @event { get; } = "clear";
        public required string streamSid { get; set; }
    }
}
