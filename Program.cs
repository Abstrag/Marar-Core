using MararCore.Linker;

namespace MararCore
{
    public class Program
    {
        private static string Origin = @"Y:\Users\bar32\Desktop\NaCondiciiDebug\gitter.jpg";
        private static string Encoded = @"Y:\Users\bar32\Desktop\NaCondiciiDebug\encoded.bin";
        private static string Decoded = @"Y:\Users\bar32\Desktop\NaCondiciiDebug\decoded.bin";

        public static void Main()
        {

            /*CacheManager.RootDirectory = @"Y:\Users\bar32\Desktop\NaCondiciiDebug\temp";
            CacheManager.GlobalClear();
            CacheManager.InitManager();

            FileStream output = new(Encoded, FileMode.Create);

            MainLinker linker = new(output, MD5.HashData([0]), SHA256.HashData([0]));
            linker.UseTime = true;
            linker.UseCrypto = true;
            linker.UseCryptoFS = true;
            linker.LargeMode = false;
            linker.LinkTo(@"Y:\Users\bar32\Desktop\NaCondiciiDebug\TestDirectory");

            output.Close();
            CacheManager.Flush();*/
            /*
            #if false
                        FileStream f1 = new(Origin, FileMode.Open);
                        FileStream f2 = new(Encoded, FileMode.Create);
                        Crypto crypto = new(MD5.HashData([0]), SHA256.HashData([0]), f1, f2);
                        crypto.Encode();
                        f1.Close();
                        f2.Close();
            #else
                        FileStream f1 = new(Encoded, FileMode.Open);
                        FileStream f2 = new(Decoded, FileMode.Create);
                        Crypto crypto = new(MD5.HashData([0]), SHA256.HashData([0]), f1, f2);
                        crypto.Decode();
                        f1.Close();
                        f2.Close();
            #endif

            #if false
            #if true
                        FileStream f1 = new(Encoded, FileMode.Open);
                        LotStreamWriter f2 = new(GetFiles(@"Y:\Users\bar32\Desktop\NaCondiciiDebug\DecodedDirectory"), [1731, 19333, 1663169]);

                        HaffmanCompressor compressor = new(f1, f2);
                        DateTime start = DateTime.Now;
                        compressor.Decode();
                        double time = (DateTime.Now - start).TotalSeconds;
                        Console.WriteLine(time);
                        Console.WriteLine($"{f1.Length / time / 1048576} Мб/с");
                        Console.WriteLine(f2.Length / (double)f1.Length);
                        f1.Close();
                        f2.Close();
                        f2.Flush();
            #else
                        LotStreamReader f1 = new(GetFiles(@"Y:\Users\bar32\Desktop\NaCondiciiDebug\TestDirectory"));
                        FileStream f2 = new(Encoded, FileMode.Create);

                        HaffmanCompressor compressor = new(f1, f2);
                        DateTime start = DateTime.Now;
                        compressor.Encode();
                        double time = (DateTime.Now - start).TotalSeconds;
                        Console.WriteLine($"{time} секунд");
                        Console.WriteLine($"{f1.Length / time / 1048576} Мб/с");
                        Console.WriteLine($"k = {f2.Length / (double)f1.Length}");
                        f1.Close();
                        f2.Close();
            #endif
            #endif
            */
        }
    }
}