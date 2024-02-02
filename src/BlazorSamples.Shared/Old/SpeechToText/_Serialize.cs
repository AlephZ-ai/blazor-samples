#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
namespace BlazorSamples.Shared.Old.SpeechToText
{
    public class TimingResult
    {
        public double conf { get; set; }
        public double end { get; set; }
        public double start { get; set; }
        public string word { get; set; }
    }

    public class RegularResult
    {
        public List<TimingResult> result { get; set; }
        public List<double> spk { get; set; }
        public int spk_frames { get; set; }
        public string text { get; set; }
    }

    public class PartialResult
    {
        public string? partial { get; set; }
    }

    public class FinalResult
    {
        public string text { get; set; }
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
