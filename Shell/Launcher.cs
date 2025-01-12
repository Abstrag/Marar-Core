namespace Marar.Shell
{
    public class Launcher
    {
        private static string LocalDirectory = @"Y:\Users\bar32\Desktop\NaCondiciiDebug\DecodedDirectory";
        private static string Origin = @"Y:\Users\bar32\Desktop\NaCondiciiDebug\gitter.jpg";
        private static string Encoded = @"Y:\Users\bar32\Desktop\NaCondiciiDebug\encoded.bin";
        private static string Decoded = @"Y:\Users\bar32\Desktop\NaCondiciiDebug\decoded.bin";

        public static void Main(string[]? args)
        {
            MainShell shell = new();

            Console.WriteLine("Hello, world!");
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