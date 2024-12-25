namespace MararCore
{
    public class LotStreamReader : Stream
    {
        private long InternalPosition = 0;
        private long StreamCounter = 0;
        public Stream[] Streams { get; private set; }
        public override long Length
        {
            get
            {
                long result = 0;
                foreach (var stream in Streams)
                {
                    result += stream.Length;
                }
                return result;
            }
        }
        public override bool CanRead => true;
        public override bool CanSeek => Streams[0].CanSeek;
        public override bool CanWrite => false;
        public override long Position
        {
            get
            {
                return InternalPosition;
            }
            set
            {
                long position = value;
                long i = 0;
                for (; i < Streams.LongLength; i++)
                {
                    position -= Streams[i].Length;
                    if (position < 0)
                    {
                        StreamCounter = i;
                        Streams[i].Position = position + Streams[i].Length;
                        InternalPosition = position;
                        break;
                    }
                }
                if (i >= Streams.Length) throw new Exception("Invalid position");
            }
        }

        public LotStreamReader(Stream[] streams)
        {
            Streams = streams;
        }
        public override void Flush()
        {
            foreach (var stream in Streams)
            {
                stream.Flush();
            }
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            int result = 0;
            long length = count;
            for (; StreamCounter < Streams.LongLength && length > 0; StreamCounter++)
            {
                int localLength = (int)Streams[StreamCounter].Length;
                length -= Streams[StreamCounter].Length;
                if (length <= 0)
                {
                    Streams[StreamCounter].Read(buffer, offset, localLength);
                    InternalPosition += localLength;
                }
                else
                {
                    Streams[StreamCounter].Read(buffer, offset, localLength);
                    InternalPosition += localLength;
                    offset += localLength;
                }
                result += localLength;
            }
            if (result == count) return 0;
            return result;
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
    }
}
