using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.Twilio
{
    public class AudioChunk
    {
        public string @event { get; } = "media";
        public required string streamSid { get; set; }
        public required Media media { get; set; }
    }

    public class Media
    {
        public required string payload { get; set; }
    }
}
