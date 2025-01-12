namespace MararCore
{
    internal class BorderedStream : Stream
    {
        private readonly long MaskLength;
        private readonly long Shift;
        private readonly Stream MainStream;
        public override bool CanRead => MainStream.CanRead;
        public override bool CanSeek => MainStream.CanSeek;
        public override bool CanWrite => MainStream.CanWrite;
        public override long Length => MaskLength;
        public override long Position
        {
            get => MainStream.Position - Shift;
            set => MainStream.Position = value + Shift;
        }
        public BorderedStream(Stream mainStream, long shift, long maskLength)
        {
            MainStream = mainStream;
            Shift = shift;
            MaskLength = maskLength;
        }
        public BorderedStream(Stream mainStream, long shift) : this(mainStream, shift, mainStream.Length - shift) { }
        public BorderedStream(Stream mainStream) : this(mainStream, mainStream.Position) { }

        public override void Flush()
        {
            MainStream.Flush();
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (count > Length) count = (int)(Length - Position);
            return MainStream.Read(buffer, offset, count);
        }
        public override int ReadByte()
        {
            return MainStream.ReadByte();
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            return MainStream.Seek(offset, origin);
        }
        public override void SetLength(long value)
        {
            MainStream.SetLength(value);
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            MainStream.Write(buffer, offset, count);
        }
        public override void WriteByte(byte value)
        {
            MainStream.WriteByte(value);
        }
    }
}
