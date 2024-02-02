using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.SpeechRecognition
{
    public sealed class SpeechRecognitionResult
    {
        public bool IsPauseDetected { get; init; } = false;
        public required string? Fragment { get; init; }
    }
}
