using System.Text.Json;
using Grpc.Core;
using BlazorSamples.Rpc.PlayHT.Protos.V1;
using Google.Protobuf;

namespace BlazorSamples.Rpc.Services
{
    public class TtsService(Tts.TtsClient client, IConfiguration configuration, HttpClient authClient) : Tts.TtsBase
    {
        private const string LeaseUri = "https://api.play.ht/api/v2/leases";
        private readonly string _user = configuration.GetValue<string>("playHT:user")!;
        private readonly string _key = configuration.GetValue<string>("playHT:key")!;
        public override async Task Tts(TtsRequest request, IServerStreamWriter<TtsResponse> responseStream, ServerCallContext context)
        {
            var authRequest = new HttpRequestMessage(HttpMethod.Post, LeaseUri);
            authRequest.Headers.Add("X-User-Id", _user);
            authRequest.Headers.Add("Authorization", $"Bearer {_key}");
            var authResponse = await authClient.SendAsync(authRequest, HttpCompletionOption.ResponseHeadersRead);
            authResponse.EnsureSuccessStatusCode();
            var lease = await authResponse.Content.ReadAsByteArrayAsync();
            using var ms = new MemoryStream(lease[72..]);
            var metadata = JsonSerializer.DeserializeAsync<IDictionary<string, string>>(ms);
            request.Lease = ByteString.CopyFrom(lease);
            using var call = client.Tts(request, context.RequestHeaders, context.Deadline, context.CancellationToken);
            await foreach (var response in call.ResponseStream.ReadAllAsync())
            {
                await responseStream.WriteAsync(response);
            }
        }
    }
}
