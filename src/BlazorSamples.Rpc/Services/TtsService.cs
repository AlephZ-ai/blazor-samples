using System.Text.Json;
using Grpc.Core;
using BlazorSamples.TextToSpeech.PlayHT.Protos.V1;
using Google.Protobuf;
using Grpc.Net.Client;

namespace BlazorSamples.Rpc.Services
{
    public class TtsService(IConfiguration configuration, HttpClient authClient) : Tts.TtsBase
    {
        private const string LeaseUri = "https://api.play.ht/api/v2/leases";
        private readonly string _user = configuration.GetValue<string>("playHT:user")!;
        private readonly string _key = configuration.GetValue<string>("playHT:key")!;
        private static GrpcChannel? _channel;
        private static Tts.TtsClient? _client;
        private static ByteString? _lease;
        private static readonly object _lock = new object();
        public override async Task Tts(TtsRequest request, IServerStreamWriter<TtsResponse> responseStream, ServerCallContext context)
        {
            var ct = context.CancellationToken;
            await EnsureClientInitialized(ct).ConfigureAwait(false);
            request.Lease = _lease;
            using var call = _client!.Tts(request, context.RequestHeaders, context.Deadline, ct);
            await foreach (var response in call.ResponseStream.ReadAllAsync(ct).WithCancellation(ct).ConfigureAwait(false))
            {
                await responseStream.WriteAsync(response, ct).ConfigureAwait(false);
            }
        }

        public static void ShutdownChannel()
        {
            _channel?.Dispose();
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
            var authRequest = new HttpRequestMessage(HttpMethod.Post, LeaseUri)
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
    }
}
