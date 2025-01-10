namespace MararCore.Linker
{
    internal static class DateTimeConverter
    {
        private static uint GetBits(int origin, byte maskLength, byte shift)
        {
            int mask = (int)(0xFFFFFFFF >> (32 - maskLength));
            origin &= (mask << shift);
            return (uint)origin >> shift;
        }
        public static int Encode(DateTime dateTime)
        {
            int result = 0;
            result |= (dateTime.Year - 2000) << 25;
            result |= dateTime.Month << 21;
            result |= dateTime.Day << 16;
            result |= dateTime.Hour << 11;
            result |= dateTime.Minute << 5;
            result |= dateTime.Second / 2;
            return result;
        }
        public static DateTime Decode(uint source)
        {
            DateTime dateTime = new(0);
            dateTime.AddYears(1999 + GetBits(source, ));
            return DateTime.Now;
        }
    }
}
