namespace MararCore0.Trash
{
    public static class BitControl
    {
        public static byte GetZerosAtEnd(uint value)
        {
            for (byte i = 0; i < 32; i++)
            {
                if ((value & 1) == 1) return i;
                value >>= 1;
            }

            return 32;
        }
        public static string GetBites(ulong value, byte position, byte length)
        {
            string result = "";
            uint mask = 1;
            mask <<= 31 - position;

            for (byte i = 0; i < length; i++)
            {
                if ((value & mask) == mask) result += '1';
                else result += "0";
                mask >>= 1;
            }

            return result;
        }

        public static string UintToString(ulong value)
        {
            string result = "";
            ulong mask = 1;
            mask <<= 63;

            for (byte b = 0; b < 64; b++)
            {
                if ((value & mask) == mask) result += '1';
                else result += "0";
                mask >>= 1;
            }

            return result;
        }
        public static string SingleToString(float value)
        {
            string result = "";
            uint num = BitConverter.SingleToUInt32Bits(value);
            result += GetBites(num, 0, 1) + "||";
            result += GetBites(num, 1, 8) + "||";
            result += GetBites(num, 9, 23);
            return result;
        }
        public static uint GetMantiss(float value)
        {
            uint floatInt = BitConverter.SingleToUInt32Bits(value);
            uint result = floatInt & 0x7FFFFF;

            if (result >> 22 != 1)
            {
                result |= 1 << 23;
            }

            return BitsShift(result);
        }
        public static sbyte GetExponent(float value)
        {
            uint floatInt = BitConverter.SingleToUInt32Bits(value);
            uint raw = floatInt & 0x7F800000;

            return (sbyte)(BitsShift(raw) - 127);
        }
        public static uint BitsShift(uint value)
        {
            if (value != 0)
            {
                for (byte i = 0; i < 32; i++)
                {
                    if ((value & 1) == 0)
                        value >>= 1;
                    else break;
                }
            }

            return value;
        }
        public static string ParseSingle(float value)
        {
            string result = "";

            if (value < 0)
                result += '-';

            result += "M:" + GetMantiss(value);
            result += " E:" + GetExponent(value);

            return result;
        }
    }
}
