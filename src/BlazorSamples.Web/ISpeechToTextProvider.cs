namespace BlazorSamples.Web
{
    public interface ISpeechToTextProvider
    {
        Task<AppendWavChunk> AppendWavChunk(byte[] buffer, int bytesRead);
        string? FinalResult();
    }
}
