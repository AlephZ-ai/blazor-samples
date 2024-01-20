using BlazorSamples.Shared;
using FFmpeg.AutoGen;
using Google.Protobuf;
using Microsoft.AspNetCore.SignalR;
using System.Runtime.InteropServices;
using Vosk;

namespace BlazorSamples.Web.Hubs
{
    // TODO: Need to fix singleton and static hacks
    // TODO: Reorder out of order buffers or don't use signalr (websockets directly? seems easier)
    public class SpeechToTextHub(VoskRecognizer recognizer) : Hub<ISpeechToTextClient>
    {
        static FileStream fileStream = default!;
        static int totalAudioLength = 0;
        unsafe AVCodecContext* codecContext = null;
        public async Task ProcessAudioBuffer(byte[] buffer, BufferPosition position, int p, string mimeType, int sampleRate, int channelCount)
        {
            totalAudioLength += buffer.Length;
            var localFileName = "Files/temp.wav";

            if (position == BufferPosition.First)
            {
                if (File.Exists(localFileName))
                    File.Delete(localFileName);

                if (!Directory.Exists("Files"))
                    Directory.CreateDirectory("Files");

                fileStream = File.OpenWrite(localFileName);
                // Write initial WAV header with the placeholders for total audio length
                WriteWavHeader(fileStream, sampleRate, channelCount, totalAudioLength);
                InitializeCodec(mimeType);
            }

            DecodeAndWriteBuffer(buffer, fileStream);
            if (position == BufferPosition.Last)
            {
                FinalizeFile(fileStream, totalAudioLength, sampleRate, channelCount);
                CleanAfterConversion();
                await fileStream.FlushAsync();
                fileStream.Close();
                fileStream.Dispose();
                totalAudioLength = 0;
            }

            await Clients.Caller.ReceiveMessage(p.ToString());
        }

        private unsafe void CleanAfterConversion()
        {
            ffmpeg.avcodec_close(codecContext);
            fixed (AVCodecContext** ptr = &codecContext)
            {
                ffmpeg.avcodec_free_context(ptr);
            }

            codecContext = null;
        }

        private AVCodecID MimeTypeToCodecId(string mimeType)
        {
            if (mimeType.Contains(';'))
            {
                mimeType = mimeType.Substring(0, mimeType.IndexOf(';'));
            }

            switch (mimeType.ToLower())
            {
                case "audio/webm":
                case "audio/opus": // Opus codec commonly used in WebM
                    return AVCodecID.AV_CODEC_ID_OPUS;

                case "audio/aac":
                    return AVCodecID.AV_CODEC_ID_AAC;

                case "audio/mp3":
                case "audio/mpeg":
                    return AVCodecID.AV_CODEC_ID_MP3;

                case "audio/wav":
                    // WAV is a container format, typically containing PCM audio
                    return AVCodecID.AV_CODEC_ID_PCM_S16LE;

                case "audio/ogg":
                    return AVCodecID.AV_CODEC_ID_VORBIS;

                // Add more cases as needed for other MIME types

                default:
                    throw new NotSupportedException($"Unsupported MIME type: {mimeType}");
            }
        }

        private unsafe void InitializeCodec(string mimeType)
        {
            // Find the decoder for the audio stream
            AVCodec* codec = ffmpeg.avcodec_find_decoder(MimeTypeToCodecId(mimeType));
            if (codec == null)
                throw new ApplicationException("Unsupported codec!");

            // Allocate a codec context for the decoder
            codecContext = ffmpeg.avcodec_alloc_context3(codec);
            if (codecContext == null)
                throw new ApplicationException("Could not allocate audio codec context.");

            // Initialize the codec context to use the given codec
            if (ffmpeg.avcodec_open2(codecContext, codec, null) < 0)
                throw new ApplicationException("Could not open codec.");
        }

        private unsafe void DecodeAndWriteBuffer(byte[] buffer, FileStream fileStream)
        {
            // Allocating an AVPacket for the incoming buffer
            var packet = ffmpeg.av_packet_alloc();
            fixed (byte* bufferPtr = buffer)
            {
                packet->data = bufferPtr;
                packet->size = buffer.Length;

                // Send the packet to the decoder
                if (ffmpeg.avcodec_send_packet(codecContext, packet) < 0)
                    throw new ApplicationException("Error sending packet to decoder.");

                // Allocating an AVFrame to receive decoded data
                var frame = ffmpeg.av_frame_alloc();

                // Receive frame from the decoder
                while (ffmpeg.avcodec_receive_frame(codecContext, frame) >= 0)
                {
                    WriteDecodedFrameToFile(frame, fileStream);
                }

                // Freeing the frame and packet
                ffmpeg.av_frame_free(&frame);
                ffmpeg.av_packet_unref(packet);
            }

            ffmpeg.av_packet_free(&packet);
        }

        private unsafe void WriteDecodedFrameToFile(AVFrame* frame, FileStream fileStream)
        {
            // Calculating the size of the decoded data
            int dataSize = ffmpeg.av_get_bytes_per_sample(codecContext->sample_fmt) * frame->nb_samples * codecContext->ch_layout.nb_channels;

            byte* data = frame->data[0];

            // Allocate a managed array to hold the data
            byte[] managedArray = new byte[dataSize];

            // Copy data from unmanaged memory to the managed array
            Marshal.Copy((IntPtr)data, managedArray, 0, dataSize);

            // Write the entire chunk to the file
            fileStream.Write(managedArray, 0, dataSize);
        }

        private void FinalizeFile(FileStream fileStream, int totalAudioLength, int sampleRate, int channelCount)
        {
            // Update the WAV header with the actual total audio length
            WriteWavHeader(fileStream, sampleRate, channelCount, totalAudioLength);
        }

        private void WriteWavHeader(FileStream fileStream, int sampleRate, int channels, int totalAudioLen)
        {
            // Calculate bits per sample based on the codec context (assuming 16 bits for simplicity)
            int bitsPerSample = 16;
            int byteRate = sampleRate * channels * bitsPerSample / 8;
            int blockAlign = channels * bitsPerSample / 8;
            int fileSize = 36 + totalAudioLen; // 36 + SubChunk2Size

            byte[] header = new byte[44];

            // RIFF chunk descriptor
            header[0] = (byte)'R';
            header[1] = (byte)'I';
            header[2] = (byte)'F';
            header[3] = (byte)'F';
            BitConverter.GetBytes(fileSize).CopyTo(header, 4);
            header[8] = (byte)'W';
            header[9] = (byte)'A';
            header[10] = (byte)'V';
            header[11] = (byte)'E';

            // "fmt " sub-chunk (format details)
            header[12] = (byte)'f';
            header[13] = (byte)'m';
            header[14] = (byte)'t';
            header[15] = (byte)' ';
            BitConverter.GetBytes(16).CopyTo(header, 16); // Sub-chunk size (16 for PCM)
            BitConverter.GetBytes((short)1).CopyTo(header, 20); // Audio format (1 for PCM)
            BitConverter.GetBytes((short)channels).CopyTo(header, 22);
            BitConverter.GetBytes(sampleRate).CopyTo(header, 24);
            BitConverter.GetBytes(byteRate).CopyTo(header, 28);
            BitConverter.GetBytes((short)blockAlign).CopyTo(header, 32);
            BitConverter.GetBytes((short)bitsPerSample).CopyTo(header, 34);

            // "data" sub-chunk (audio data)
            header[36] = (byte)'d';
            header[37] = (byte)'a';
            header[38] = (byte)'t';
            header[39] = (byte)'a';
            BitConverter.GetBytes(totalAudioLen).CopyTo(header, 40); // Sub-chunk size (audio data size)

            // Write the header to the file
            fileStream.Seek(0, SeekOrigin.Begin); // Go to the start of the file
            fileStream.Write(header, 0, header.Length);
        }
    }
}
