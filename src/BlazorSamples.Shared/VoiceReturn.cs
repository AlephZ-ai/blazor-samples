using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSamples.Shared
{
    public class VoiceReturn
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string id { get; set; }
        public string name { get; set; }
        public string sample { get; set; }
        public string accent { get; set; }
        public string age { get; set; }
        public string gender { get; set; }
        public string language { get; set; }
        public string language_code { get; set; }
        public string loudness { get; set; }
        public string style { get; set; }
        public string tempo { get; set; }
        public string texture { get; set; }
        public bool is_cloned { get; set; }
        public string voice_engine { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
}
