using Marar.Core;
using MararProject;

namespace Marar.Shell
{
    public class Launcher
    {

        public static byte LinkerVersion = 0;
        public static string CommonVersion = "0.0.1 alpha";
        public static string RootDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public static Stream LogStream = new FileStream(Path.Combine(RootDirectory, "log.txt"), FileMode.Create);
#if false
        public static int Main(string[]? args)
        {
            CacheManager.InitManager(Path.Combine(RootDirectory, "temp"));
            MainShell.LinkerTrace = new LinkerTrace(LogStream);

            int result = MainShell.Run(args ?? []);

            LogStream.Close();
            CacheManager.Flush();
            return result;
        }
#endif
        public static void Main()
        {
            PlainAES128 aes = new(null, null, [0x2b, 0x7e, 0x15, 0x16, 0x28, 0xae, 0xd2, 0xa6, 0xab, 0xf7, 0x15, 0x88, 0x09, 0xcf, 0x4f, 0x3c]);
            aes.Debug();
        }
    }
}