using System.Text.Json;
using BlazorSamples.TextToSpeech.PlayHT.Protos.V1;

namespace BlazorSamples.Shared.TextToSpeech.PlayHT;

public sealed class PlayHTTextToSpeechGeneratorOptions
{
    public string LeasesUrl { get; init; } = "https://api.play.ht/api/v2/leases";
    public required string User { get; init; }
    public required string Key { get; init; }

    //public string Voice { get; init; } = "s3://peregrine-voices/oliver_narrative2_parrot_saad/manifest.json";
    //public string Voice { get; init; } = "s3://peregrine-voices/mel21/manifest.json";
    //public string Voice { get; init; } = "s3://voice-cloning-zero-shot/fd633950-d8be-4825-9d59-23d93c880d40/aaron2/manifest.json";
    public string Voice { get; init; } = "s3://mockingbird-prod/susan_vo_training_46ffcc60-d630-42f6-acfe-4affd003ae7a/voices/speaker/manifest.json";

    public Format Format { get; init; } = Format.Mulaw;

    public float Temperature { get; init; } = 1.2f;

    public Quality Quality { get; init; } = Quality.Draft;

    public int SampleRate { get; init; } = 8000;

    public float Speed { get; init; } = 1.2f;

    public JsonSerializerOptions JsonOptions { get; init; } = JsonSerializerOptions.Default;
}