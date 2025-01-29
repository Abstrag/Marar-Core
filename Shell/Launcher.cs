using Marar.Core;

namespace Marar.Shell
{
    public class Launcher
    {
        public static byte LinkerVersion = 0;
        public static string CommonVersion = "0.0.1 alpha";
        public static string RootDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public static Stream LogStream = new FileStream(Path.Combine(RootDirectory, "log.txt"), FileMode.Create);

        public static int Main(string[]? args)
        {
            CacheManager.InitManager(Path.Combine(RootDirectory, "temp"));
            MainShell.LinkerTrace = new LinkerTrace(LogStream);

            int result = MainShell.Run(args ?? []);

            LogStream.Close();
            CacheManager.Flush();
            return result;
        }
    }
}