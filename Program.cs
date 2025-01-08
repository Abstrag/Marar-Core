using MararCore.Compress.Haffman;
using MararCore.LotStreams;

namespace MararCore
{
    public class Program
    {
        private static string Origin = @"Y:\Users\bar32\Desktop\NaCondiciiDebug\gitter.jpg";
        private static string Encoded = @"Y:\Users\bar32\Desktop\NaCondiciiDebug\encoded.bin";
        private static string Decoded = @"Y:\Users\bar32\Desktop\NaCondiciiDebug\decoded.bin";

        private static FileStream[] GetFiles(string path)
        {
            string[] files = Directory.GetFiles(path);
            FileStream[] result = new FileStream[files.Length];
            for (long i = 0; i < files.LongLength; i++)
            {
                result[i] = new FileStream(files[i], FileMode.Open);
            }
            return result;
        }

        public static void Main()
        {
            /*BinaryNode node = new();
            node.Left = new(new((byte)'a', 5));
            node.Right = new();*/
            FileStream f1 = new(Origin, FileMode.Open);
            FileStream f2 = new(Encoded, FileMode.Create);
            Crypto crypto = new()
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
        }
    }
}