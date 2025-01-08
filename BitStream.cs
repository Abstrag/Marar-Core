namespace MararCore
{
    public class BitStream
    {
        private Stream Base;
        private byte LastByte = 0;
        private byte LastLength = 0;
        private byte LastCount
        {
            get
            {
                return (byte)(7 - LastLength);
            }
            set
            {
                LastLength = (byte)(7 - value);
            }
        }

        public BitStream(Stream stream)
        {
            Base = stream;
        }

        private void WriteBit(byte bit)
        {
            if (bit != 0) LastByte |= (byte)(1 << (7 - LastLength));
            if (LastLength == 7)
            {
                Base.WriteByte(LastByte);
                LastLength = 0;
                LastByte = 0;
            }
            else LastLength++;
        }

        public void WriteByte(byte data, byte length)
        {
            data <<= 8 - length;
            for (byte i = 0; i < length; i++)
            {
                WriteBit((byte)(data & (byte)(1 << 7 - i)));
            }
        }
        public void Write(byte[] data, byte length)
        {
            for (uint i = 0; i < (data.Length - 1); i++)
                WriteByte(data[i], 8);
            WriteByte(data[^1], length);
        }
        public void Write(ulong data, byte bitsLength)
        {
            for (sbyte i = 0; i < bitsLength; i++)
            {
                LastByte |= (byte)(((data & (ulong)(1 << i)) >> i) << LastLength);
                if (LastLength == 7)
                {
                    Base.WriteByte(LastByte);
                    LastByte = 0;
                    LastLength = 0;
                }
                else LastLength++;
            }
        }
        public void FlushWrite()
        {
            if (LastLength > 0)
                Base.WriteByte(LastByte);
        }

        public byte ReadBit()
        {
            byte result = (byte)((1 << 7 - LastLength) & LastByte);
            if (LastLength == 7)
            {
                LastByte = (byte)Base.ReadByte();
                LastLength = 0;
            }
            else LastLength++;
            return result;
        }

        public byte ReadByte(byte length)
        {
            byte result = 0;

            for (byte i = 0; i < length; i++)
            {
                if (ReadBit() != 0) result |= (byte)(1 << (7 - i));
            }

            return (byte)(result >> (8 - length));
        }
        public ulong Read(byte bitsLength)
        {
            ulong result = 0;
            for (sbyte i = 0; i < bitsLength; i++)
            {
                if (LastLength >= 8)
                {
                    LastByte = (byte)Base.ReadByte();
                    LastLength = 0;
                }

                result |= (ulong)(LastByte & (1 << (7 - LastLength))) << (i - LastLength);
                LastLength++;
            }
            return result;
        }

        public void StartRead()
        {
            LastByte = (byte)Base.ReadByte();
        }
    }
}
