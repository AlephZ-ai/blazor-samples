using System.Runtime.CompilerServices;
using System.Text.Json;
using BlazorSamples.TextToSpeech.PlayHT.Protos.V1;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;

namespace BlazorSamples.Shared.Old.TextToSpeech
{
    public class PlayHTTextToSpeech(IConfiguration configuration, HttpClient authClient) : ITextToSpeech
    {
        public const string LeasesUrl = "https://api.play.ht/api/v2/leases";
        private readonly string _user = configuration.GetValue<string>("playHT:user")!;
        private readonly string _key = configuration.GetValue<string>("playHT:key")!;
        private static readonly object _lock = new object();
        private static GrpcChannel? _channel;
        private static Tts.TtsClient? _client;
        private static ByteString? _lease;
        public async IAsyncEnumerable<byte[]> Voice(string text, [EnumeratorCancellation] CancellationToken ct)
        {
            await EnsureClientInitialized(ct).ConfigureAwait(false);
            var request = new TtsRequest
            {
                Lease = _lease,
                Params = new()
                {
                    Text = { text },
                    //Voice = "s3://peregrine-voices/oliver_narrative2_parrot_saad/manifest.json",
                    //Voice = "s3://peregrine-voices/mel21/manifest.json",
                    //Voice = "s3://voice-cloning-zero-shot/fd633950-d8be-4825-9d59-23d93c880d40/aaron2/manifest.json",
                    Voice = "s3://mockingbird-prod/susan_vo_training_46ffcc60-d630-42f6-acfe-4affd003ae7a/voices/speaker/manifest.json",
                    Format = Format.Mulaw,
                    SampleRate = 8000,
                    Temperature = 1.5f,
                    Quality = Quality.Medium,
                    Speed = 1.0f
                }
            };

            using var call = _client!.Tts(request, null, null, ct);
            await foreach (var response in call.ResponseStream.ReadAllAsync(ct).WithCancellation(ct).ConfigureAwait(false))
            {
                yield return response.Data.ToByteArray();
            }
        }

        private async Task EnsureClientInitialized(CancellationToken ct)
        {
            if (_client is null)
            {
                var (client, channel, lease) = await InitializeClient(ct).ConfigureAwait(false);
                lock (_lock)
                {
                    if (_client is null)
                    {
                        _client = client;
                        _channel = channel;
                        _lease = lease;
                    }
                }
            }
        }

        private async Task<(Tts.TtsClient client, GrpcChannel channel, ByteString lease)> InitializeClient(CancellationToken ct)
        {
            var authRequest = new HttpRequestMessage(HttpMethod.Post, LeasesUrl)
            {
                Headers = { { "X-User-Id", _user }, { "Authorization", $"Bearer {_key}" } }
            };

            var authResponse = await authClient.SendAsync(authRequest, HttpCompletionOption.ResponseHeadersRead, ct);
            authResponse.EnsureSuccessStatusCode();
            var lease = await authResponse.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);
            using var ms = new MemoryStream(lease[72..]);
            var metadata = (await JsonSerializer.DeserializeAsync<IDictionary<string, JsonElement>>(ms, JsonSerializerOptions.Default, ct).ConfigureAwait(false))!;
            var inferenceAddress = metadata["inference_address"].ToString();
            var leaseByteString = ByteString.CopyFrom(lease);
            // Can't use DI Tts.TtsClient because it needs dynamic address but channels are thread safe
            var channel = GrpcChannel.ForAddress($"https://{inferenceAddress}");
            var client = new Tts.TtsClient(channel);
            return (client, channel, leaseByteString);
        }

        public static void ShutdownChannel()
        {
            _channel?.Dispose();
        }
    }
}
