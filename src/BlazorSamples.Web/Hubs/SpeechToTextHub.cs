using BlazorSamples.Shared;
using Google.Protobuf;
using Microsoft.AspNetCore.SignalR;

namespace BlazorSamples.Web.Hubs
{
    public class SpeechToTextHub : Hub<ISpeechToTextClient>
    {
        public async Task ProcessAudioBuffer(byte[] buffer, BufferPosition position, string mimeType, int sampleRate, int channels)
        {
            await Clients.All.ReceiveMessage(DateTime.Now);
        }
    }
}
