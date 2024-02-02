using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.SpeechRecognition.Vosk
{
    public sealed class VoskSpeechRecognizerOptions
    {
        public string Model { get; init; } = "vosk-model-en-us-0.22";
        //public string Model { get; init; } = "vosk-model-small-en-us-0.15";
        public string Spk { get; init; } = "vosk-model-spk-0.4";
        public int SampleRate { get; init; } = 16000;
        public bool Words { get; init; } = true;
        public bool PartialWords { get; init; } = true;
        public JsonSerializerOptions JsonOptions { get; init; } = JsonSerializerOptions.Default;
    }
}
