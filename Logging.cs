using System.Text;

namespace MararCore
{
    public class Logging
    {
        public static FileStream LogFile = new(@"Y:\Users\bar32\Pictures\trace.txt", FileMode.Create);

        public static void WriteLine(string message)
        {
            LogFile.Write(Encoding.UTF8.GetBytes(message + '\n'));
            LogFile.Flush();
        }
        public static void Write(string message)
        {
            LogFile.Write(Encoding.UTF8.GetBytes(message));
            LogFile.Flush();
        }
    }
}
