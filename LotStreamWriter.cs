namespace MararCore
{
    internal class LotStreamWriter : Stream
    {
        private ulong StreamCounter = 0;
        private long[] Lengths;
        public Stream[] Streams { get; private set; }
        public override long Length
        {
            get
            {
                long result = 0;
                foreach (var length in Lengths)
                {
                    result += length;
                }
                return result;
            }
        }
        public override bool CanRead => false;
        public override bool CanSeek => Streams[0].CanSeek;
        public override bool CanWrite => true;
        public override long Position
        {
            get
            {
                long result = 0;
                for (uint i = 0; i <= StreamCounter; i++)
                {
                    result += Lengths[i];
                }
                return result;
            }
            set
            {
                for (long j = 0; j < Streams.Length; j++)
                {
                    Streams[j].Position = 0;
                }
                for (uint i = 0; i < Streams.Length; i++)
                {
                    value -= Lengths[i];
                    if (value < 0)
                    {
                        StreamCounter = i;
                        Streams[i].Position = value + Lengths[i];
                        break;
                    }
                }
                throw new Exception("Invalid position");
            }
        }

        public LotStreamWriter(Stream[] streams, long[] lengths)
        {
            Streams = streams;
            Lengths = lengths;
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
            int startCount = count;

            for (; StreamCounter < (ulong)Streams.Length; StreamCounter++)
            {
                int length = (int)(Lengths[StreamCounter] - Streams[StreamCounter].Position);
                if (length >= count)
                {
                    Streams[StreamCounter].Write(buffer, offset, count);
                    break;
                }
                count -= length;
                Streams[StreamCounter].Write(buffer, offset, count);
                offset += length;
            }

            Position += startCount;
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
