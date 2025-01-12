namespace MararCore.Linker
{
    public static class DateTimeConverter
    {
        private static int GetBits(int origin, byte maskLength, byte shift)
        {
            int mask = (int)(0xFFFFFFFF >> (32 - maskLength));
            origin &= (mask << shift);
            return origin >> shift;
        }
        public static int EncodeDateTime(DateTime dateTime)
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
        public static DateTime DecodeDateTime(int source)
        {
            DateTime dateTime = new(0);
            dateTime = dateTime.AddYears(1999 + GetBits(source, 7, 25));
            dateTime = dateTime.AddMonths(GetBits(source, 4, 21) - 1);
            dateTime = dateTime.AddDays(GetBits(source, 5, 16) - 1);
            dateTime = dateTime.AddHours(GetBits(source, 5, 11));
            dateTime = dateTime.AddMinutes(GetBits(source, 6, 5));
            dateTime = dateTime.AddSeconds(GetBits(source, 5, 0) * 2);
            return dateTime;
        }
    }
}
