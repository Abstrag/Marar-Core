using MararCore.Compress.Haffman;

namespace MararCore
{
    public class Program
    {
        private static string Origin = @"Y:\Users\bar32\Pictures\canon\IMG_0128.CR2";
        private static string Encoded = @"Y:\Users\bar32\Pictures\sample.bin";
        private static string Decoded = @"Y:\Users\bar32\Pictures\figna.bin";

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
#if true
#if true
            FileStream f1 = new(Encoded, FileMode.Open);
            LotStreamWriter f2 = new(GetFiles(@"Y:\Users\bar32\Pictures\DecodedDirectory"), [1731, 19333, 1663169]);

            HaffmanCompressor compressor = new(f1, f2);
            DateTime start = DateTime.Now;
            compressor.Decode();
            double time = (DateTime.Now - start).TotalSeconds;
            Console.WriteLine(time);
            Console.WriteLine($"{f1.Length / time / 1048576} Мб/с");
            Console.WriteLine(f2.Length / (double)f1.Length);
            f1.Close();
            f2.Close();
#else
            LotStreamReader f1 = new(GetFiles(@"Y:\Users\bar32\Pictures\TestDirectory"));
            FileStream f2 = new(Encoded, FileMode.Create);

            HaffmanCompressor compressor = new(f1, f2);
            DateTime start = DateTime.Now;
            compressor.Encode();
            double time = (DateTime.Now - start).TotalSeconds;
            Console.WriteLine(time);
            Console.WriteLine($"{f1.Length / time / 1048576} Мб/с");
            Console.WriteLine(f2.Length / (double)f1.Length);
            f1.Close();
            f2.Close();
#endif
#endif
        }
    }
}