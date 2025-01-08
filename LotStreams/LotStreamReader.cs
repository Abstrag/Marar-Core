namespace MararCore.LotStreams
{
    public class LotStreamReader : Stream
    {
        private Stream CurrentStream => Streams[StreamCounter];
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
                for (long j = 0; j < Streams.Length; j++)
                {
                    Streams[j].Position = 0;
                }
                long position = value;
                long i = 0;
                for (; i < Streams.LongLength; i++)
                {
                    position -= Streams[i].Length;
                    if (position < 0)
                    {
                        StreamCounter = i;
                        Streams[i].Position = position + Streams[i].Length;
                        InternalPosition = value;
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
        /*public override int Read(byte[] buffer, int offset, int count)
        {
            int result = 0;
            long length = count;
            while (StreamCounter < Streams.LongLength && length > 0)
            {
                int localLength = (int)(Streams[StreamCounter].Length - Streams[StreamCounter].Position);
                length -= Streams[StreamCounter].Length;
                if (length <= 0)
                {
                    Streams[StreamCounter].ReadExactly(buffer, offset, count);
                    InternalPosition += count;
                }
                else
                {
                    Streams[StreamCounter].ReadExactly(buffer, offset, localLength);
                    InternalPosition += localLength;
                    offset += localLength;
                }
                result += localLength;
                StreamCounter++;
            }
            if (result == count) return 0;
            return result;
        }*/
        public override int Read(byte[] buffer, int offset, int count)
        {
            int result = 0;
            int originalCount = count;

            while (count > 0 && Position < Length)
            {
                long streamLength = CurrentStream.Length - CurrentStream.Position;
                long remains = count - streamLength;

                if (count - streamLength <= 0)
                {
                    CurrentStream.ReadExactly(buffer, offset, count);
                    InternalPosition += count;
                    if (count - streamLength == 0) StreamCounter++;
                    return originalCount;
                }
                else
                {
                    CurrentStream.ReadExactly(buffer, offset, (int)streamLength);
                    offset += (int)remains;
                    count -= (int)streamLength;
                    result += (int)streamLength;
                    StreamCounter++;
                }
            }
            return result;
        }
        public override int ReadByte()
        {
            if (InternalPosition < Length)
            {
                while (true)
                {
                    if (CurrentStream.Position < CurrentStream.Length)
                    {
                        InternalPosition++;
                        return CurrentStream.ReadByte();
                    }
                    else StreamCounter++;
                }
            }
            else return -1;
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
