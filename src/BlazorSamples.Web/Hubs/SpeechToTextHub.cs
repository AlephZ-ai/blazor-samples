using BlazorSamples.Shared;
using Google.Protobuf;
using Microsoft.AspNetCore.SignalR;
using Vosk;

namespace BlazorSamples.Web.Hubs
{
    public class SpeechToTextHub(VoskRecognizer recognizer) : Hub<ISpeechToTextClient>
    {
        public async Task ProcessAudioBuffer(byte[] buffer, BufferPosition position, string mimeType, int sampleRate, int channels)
        {
            string message;
            if (recognizer.AcceptWaveform(buffer, buffer.Length))
            {
                message = recognizer.Result();
            }
            else
            {
                message = recognizer.PartialResult();
            }

            await Clients.Caller.ReceiveMessage(message);

            if (position == BufferPosition.Last)
            {
                message = recognizer.FinalResult();
                await Clients.Caller.ReceiveMessage(message);
            }
        }
    }
}
