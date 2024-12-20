namespace MararCore0
{
    internal class LotStream : Stream
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
        public override bool CanRead => Streams[0].CanRead;
        public override bool CanSeek => Streams[0].CanSeek;
        public override bool CanWrite => Streams[0].CanWrite;
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
                    if (position == 0)
                    {
                        counter = 
                        break;
                    }
                }
            }
        }

        public LotStream(Stream[] streams)
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
