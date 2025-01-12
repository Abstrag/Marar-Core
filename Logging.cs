using Marar.Shell;
using System.Text;

namespace Marar.Core
{
    internal class Logging
    {
        public static FileStream LogFile = new(Path.Combine(Launcher.RootDirectory, "log.txt"), FileMode.Create);

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
