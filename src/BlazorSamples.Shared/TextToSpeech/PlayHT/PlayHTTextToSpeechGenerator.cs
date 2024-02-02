using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using BlazorSamples.TextToSpeech.PlayHT.Protos.V1;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;

namespace BlazorSamples.Shared.TextToSpeech.PlayHT;

public sealed class PlayHTTextToSpeechGenerator(PlayHTTextToSpeechGeneratorOptions options, HttpClient authClient) : ITextToSpeechGenerator
{
    public async IAsyncEnumerable<ReadOnlyMemory<byte>> GenerateAsync(IAsyncEnumerable<string> source, [EnumeratorCancellation] CancellationToken ct = default)
    {
        // TODO: Cache channel
        var (client, channel, lease) = await InitializeClient(ct).ConfigureAwait(false);
        try
        {
            await foreach (var text in source.WithCancellation(ct).ConfigureAwait(false))
            {
                var request = new TtsRequest
                {
                    Lease = lease,
                    Params = new()
                    {
                        Text = { text },
                        Voice = options.Voice,
                        Format = options.Format,
                        SampleRate = options.SampleRate,
                        Temperature = options.Temperature,
                        Quality = options.Quality,
                        Speed = options.Speed,
                    }
                };

                using var call = client.Tts(request, null, null, ct);
                await foreach (var response in call.ResponseStream.ReadAllAsync(ct).WithCancellation(ct).ConfigureAwait(false))
                {
                    // TODO: Fix allocations
                    yield return new ReadOnlyMemory<byte>(response.Data.ToByteArray());
                }
            }
        }
        finally
        {
            channel.Dispose();
        }
    }

    private async Task<(Tts.TtsClient client, GrpcChannel channel, ByteString lease)> InitializeClient(CancellationToken ct)
    {
        var authRequest = new HttpRequestMessage(HttpMethod.Post, options.LeasesUrl)
        {
            Headers = { { "X-User-Id", options.User }, { "Authorization", $"Bearer { options.Key }" } }
        };

        var authResponse = await authClient.SendAsync(authRequest, HttpCompletionOption.ResponseHeadersRead, ct);
        authResponse.EnsureSuccessStatusCode();
        var lease = await authResponse.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);
        using var ms = new MemoryStream(lease[72..]);
        var metadata = (await JsonSerializer.DeserializeAsync<IDictionary<string, JsonElement>>(ms, options.JsonOptions, ct).ConfigureAwait(false))!;
        var inferenceAddress = metadata["inference_address"].ToString();
        var leaseByteString = ByteString.CopyFrom(lease);
        // Can't use DI Tts.TtsClient because it needs dynamic address but channels are thread safe
        var channel = GrpcChannel.ForAddress($"https://{inferenceAddress}");
        var client = new Tts.TtsClient(channel);
        return (client, channel, leaseByteString);
    }
}
