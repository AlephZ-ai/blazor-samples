using Azure.Core;
using BlazorSamples.PlayHT.Protos.V1;
using FFMpegCore.Pipes;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.TextToSpeech
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
                    Voice = "s3://peregrine-voices/oliver_narrative2_parrot_saad/manifest.json",
                    Format = Format.Mulaw,
                    SampleRate = 8000,
                    Temperature = 1.5f,
                    Quality = Quality.Draft,
                    Speed = 1.2f
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
