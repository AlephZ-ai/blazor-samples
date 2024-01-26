
namespace BlazorSamples.Ws
{
    public class AudioConverter
    {
        static byte[] empty = Array.Empty<byte>();
        public (byte[] converted, int convertedLength) ConvertBuffer(byte[] data)
        {
            return (empty, 0);
        }
    }
}
