using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BlazorSamples.Shared.PlayHT;
using BlazorSamples.PlayHT.Protos.V1;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;

namespace BlazorSamples.ScratchConsole
{
    public class PlayHTProgram
    {
        public static void Main()
        {
            MainAsync().Wait();
        }

        public static async Task MainAsync()
        {
            var mp3Path = "Files/test.mp3";
            if (File.Exists(mp3Path))
                File.Delete(mp3Path);

            if (!Directory.Exists("Files"))
                Directory.CreateDirectory("Files");

            var builder = new ConfigurationBuilder()
                .AddUserSecrets<PlayHTProgram>();

            var configuration = builder.Build();
            using var authClient = new HttpClient();
            var authRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.play.ht/api/v2/leases");
            var user = configuration.GetValue<string>("playHT:user");
            var apiKey = configuration.GetValue<string>("playHT:key");
            authRequest.Headers.Add("X-User-Id", user);
            authRequest.Headers.Add("Authorization", $"Bearer {apiKey}");
            var authResponse = await authClient.SendAsync(authRequest, HttpCompletionOption.ResponseHeadersRead);
            authResponse.EnsureSuccessStatusCode();
            var lease = await authResponse.Content.ReadAsByteArrayAsync();
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(1519257480).ToLocalTime();
            var created = epoch.AddSeconds(BinaryPrimitives.ReadUInt32BigEndian(lease.AsSpan(64, 4)));
            var duration = TimeSpan.FromSeconds(BinaryPrimitives.ReadUInt32BigEndian(lease.AsSpan(68, 4)));
            var expires = created.Add(duration);
            using var ms = new MemoryStream(lease[72..]);
            var metadata = (await JsonSerializer.DeserializeAsync<IDictionary<string, JsonElement>>(ms))!;
            var inferenceAddress = metadata["inference_address"].ToString();
            var headers = new Metadata
            {
                { "Content-Type", "audio/mpeg" },
            };

            var request = new TtsRequest
            {
                Lease = ByteString.CopyFrom(lease),
                Params = new()
                {
                    Text   = { "This is a test of the automatic broadcast system.  This is only a test." },
                    Voice = "s3://peregrine-voices/oliver_narrative2_parrot_saad/manifest.json",
                    Format = Format.Mp3,
                    Temperature = 1.5f,
                    Quality = Quality.Premium,
                    Speed = 0.8f,
                    SpeechAttributes = 9,
                    StyleGuidance = 16,
                }
            };


            using var channel = GrpcChannel.ForAddress($"https://{inferenceAddress}");
            await using var stream = File.OpenWrite(mp3Path);
            var client = new Tts.TtsClient(channel);
            var response = client.Tts(request, headers);
            await foreach (var item in response.ResponseStream.ReadAllAsync())
            {
                await stream.WriteAsync(item.Data.Memory);
            }

            await stream.FlushAsync();
        }
    }
}
