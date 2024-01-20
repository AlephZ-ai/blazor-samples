using BlazorSamples.Shared;
using Google.Protobuf;
using Microsoft.AspNetCore.SignalR;
using Vosk;

namespace BlazorSamples.Web.Hubs
{
    // TODO: Need to fix singleton and static hacks
    // TODO: Reorder out of order buffers or don't use signalr (websockets directly? seems easier)
    public class SpeechToTextHub(VoskRecognizer recognizer) : Hub<ISpeechToTextClient>
    {
        static FileStream stream = default!;
        public async Task ProcessAudioBuffer(byte[] buffer, BufferPosition position, int p, string mimeType, int sampleRate, int channelCount)
        {
            // string message;
            // if (recognizer.AcceptWaveform(buffer, buffer.Length))
            // {
            //     message = recognizer.Result();
            // }
            // else
            // {
            //     message = recognizer.PartialResult();
            // }

            // await Clients.Caller.ReceiveMessage(message);

            // if (position == BufferPosition.Last)
            // {
            //     message = recognizer.FinalResult();
            //     await Clients.Caller.ReceiveMessage(message);
            // }
            var localFileName = $"Files/temp.{mimeType.Substring(6)}";

            // Delete file if it exists and this the first buffer
            if (position == BufferPosition.First)
            {
                if (File.Exists(localFileName))
                    File.Delete(localFileName);

                if (!Directory.Exists("Files"))
                    Directory.CreateDirectory("Files");

                stream = File.OpenWrite(localFileName);
            }

            stream.Write(buffer);
            if (position == BufferPosition.Last)
            {
                await stream.FlushAsync();
                stream.Close();
                stream.Dispose();
            }

            await Clients.Caller.ReceiveMessage(p.ToString());
        }
    }
}
