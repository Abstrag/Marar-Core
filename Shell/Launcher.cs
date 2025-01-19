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
            /*#if false
                        CacheManager.RootDirectory = @"Y:\Users\bar32\Desktop\NaCondiciiDebug\temp";
                        CacheManager.GlobalClear();
                        CacheManager.InitManager();

                        FileStream input = new(Encoded, FileMode.Open);

                        MainLinker linker = new(input, MD5.HashData([0]), SHA256.HashData([0]));
                        linker.LinkFrom(LocalDirectory);

                        CacheManager.Flush();
            #else
                        CacheManager.RootDirectory = @"Y:\Users\bar32\Desktop\NaCondiciiDebug\temp";
                        CacheManager.GlobalClear();
                        CacheManager.InitManager();

                        FileStream output = new(Encoded, FileMode.Create);

                        MainLinker linker = new(output, MD5.HashData([0]), SHA256.HashData([0]));
                        linker.UseTime = true;
                        linker.UseCrypto = true;
                        linker.UseCryptoFS = true;
                        linker.LargeMode = true;
                        linker.LinkTo(@"Y:\Users\bar32\Desktop\NaCondiciiDebug\TestDirectory");

                        output.Close();
                        CacheManager.Flush();
            #endif*/
        }
    }
}