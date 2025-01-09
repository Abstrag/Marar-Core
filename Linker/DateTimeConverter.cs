namespace MararCore.Linker
{
    internal static class DateTimeConverter
    {
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
        public static DateTime Decode(int source)
        {
            // доделай 
            return DateTime.Now;
        }
    }
}
