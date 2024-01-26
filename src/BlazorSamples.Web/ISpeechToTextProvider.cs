﻿namespace BlazorSamples.Web
{
    public interface ISpeechToTextProvider
    {
        public const string Models = ".models";
        Task<AppendWavChunk> AppendWavChunk(byte[] buffer, int bytesRead);
        string? FinalResult();
        Task DownloadModelsAsync();
    }
}