namespace BlazorSamples.Shared.SpeechToText
{
    public class DuplexMemoryStream : Stream
    {
        private readonly MemoryStream _writeStream = new MemoryStream();
        private readonly MemoryStream _readStream = new MemoryStream();
        private readonly object _lock = new object();

        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (_lock)
            {
                _writeStream.Write(buffer, offset, count);
                // Update the read stream with new data
                UpdateReadStream(buffer, offset, count);
            }
        }

        private void UpdateReadStream(byte[] buffer, int offset, int count)
        {
            long currentPosition = _readStream.Position;
            _readStream.Position = _readStream.Length;
            _readStream.Write(buffer, offset, count);
            _readStream.Position = currentPosition;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (_lock)
            {
                return _readStream.Read(buffer, offset, count);
            }
        }

        // Implement other required members of the Stream class
        // ...

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => true;

        public override long Length => int.MaxValue;

        public override long Position
        {
            get => _readStream.Position;
            set => _readStream.Position = value;
        }

        public override void Flush()
        {
            _readStream.Flush();
            _writeStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _readStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }
    }
}
