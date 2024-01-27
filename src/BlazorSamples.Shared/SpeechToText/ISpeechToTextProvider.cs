namespace BlazorSamples.Shared.SpeechToText
{
    public interface ISpeechToTextProvider
    {
        public const string Models = ".models";
        Task<AppendWavChunk> AppendWavChunk(byte[] buffer, int bytesRead);
        string? FinalResult();
    }
}
