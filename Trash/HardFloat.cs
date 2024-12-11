namespace MararCore0.Trash
{
    public class HardFloat
    {
        private static byte GetMantissWeight(ulong mantiss)
        {
            for (byte i = 0; i < 64; i++)
            {
                ulong mask = (ulong)1 << i;
                if ((mantiss & mask) > 0) return i;
            }
            return 0;
        }
        private static uint GetMantiss(ulong raw)
        {
            return (uint)((raw & 0xFFFFFFFFFFFFF | 0x10000000000000) >> 21);
        }
        private static short GetExponent(ulong raw)
        {
            return (short)((raw >> 52 & 0x7FF) - 1023);
        }
        public static uint GetInt(double value)
        {
            if (value == 0) return 0;
            ulong raw = BitConverter.DoubleToUInt64Bits(value);
            return GetMantiss(raw) >> Math.Abs(GetExponent(raw));
        }
        public static double GetDouble(uint a)
        {
            ulong result = 0;
            ushort exponent = 0;

            for (byte i = 0; i < 32; i++)
            {
                if ((a & 0x80000000) == 0x80000000) break;
                a <<= 1;
                exponent++;
            }

            exponent = (ushort)(1023 - exponent);
            result |= (ulong)exponent << 52;
            result |= (ulong)a << 21;

            return BitConverter.UInt64BitsToDouble(result);
        }
        public static bool CheckOverflow(double value)
        {
            ulong raw = BitConverter.DoubleToUInt64Bits(value);
            ulong mantiss = GetMantiss(raw);
            if (mantiss > 0xFFFFFFFF || Math.Abs(GetExponent(raw)) >= 32 - GetMantissWeight(mantiss)) return true;
            else return false;
        }
    }
}
