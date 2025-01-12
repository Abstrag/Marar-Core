using MararCore.Linker;
using System.Security.Cryptography;

namespace MararCore
{
    public class Program
    {
        private static string LocalDirectory = @"Y:\Users\bar32\Desktop\NaCondiciiDebug\DecodedDirectory";
        private static string Origin = @"Y:\Users\bar32\Desktop\NaCondiciiDebug\gitter.jpg";
        private static string Encoded = @"Y:\Users\bar32\Desktop\NaCondiciiDebug\encoded.bin";
        private static string Decoded = @"Y:\Users\bar32\Desktop\NaCondiciiDebug\decoded.bin";

        public static void Main()
        {
            /*MemoryStream s1 = new([0, 95, 37, 184]);
            MemoryStream s2 = new([10, 54, 21, 49]);
            s1.SetLength(2);
            s1.CopyTo(s2);
            s1.SetLength(4);
            s1.ToArray();
            s2.ToArray();*/

#if true
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
            linker.LargeMode = false;
            linker.LinkTo(@"Y:\Users\bar32\Desktop\NaCondiciiDebug\TestDirectory");

            output.Close();
            CacheManager.Flush();
#endif
        }
    }
}