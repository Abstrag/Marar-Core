namespace MararCore
{
    public class LotStreamReader : Stream
    {
        private uint StreamCounter = 0;
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
                long result = 0;
                for (uint i = 0; i < StreamCounter; i++)
                {
                    result += Streams[i].Length;
                }
                return result + Streams[StreamCounter].Position;
            }
            set
            {
                long position = value;
                long counter = 0;
                for (uint i = 0; i < Streams.Length; i++)
                {
                    position -= Streams[i].Length;
                    if (position < 0)
                    {
                        StreamCounter = i;
                        Streams[i].Position = position;
                        break;
                    }
                    if (position == 1)
                    {
                        //counter = 
                        break;
                    }
                }
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
            throw new NotImplementedException();
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
