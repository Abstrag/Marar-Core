using MararCore.Compress.Arithmetic;

namespace MararCore
{
    public class Program
    {
        #if true
        private static string Origin = @"Y:\Users\bar32\Pictures\sample.txt";
        private static string Encoded = @"Y:\Users\bar32\Pictures\sample.bin";
        private static string Decoded = @"Y:\Users\bar32\Pictures\figna.bin";
        #else
        private static string Origin = "/home/admen/Рабочий стол/MararTest/466893-vbig.png";
        private static string Encoded = "/home/admen/Рабочий стол/MararTest/f2.bin";
        private static string Decoded = "/home/admen/Рабочий стол/MararTest/f3.bin";
        #endif

        public static void Main()
        {
#if true
            FileStream f1 = new(Encoded, FileMode.Open);
            FileStream f2 = new(Decoded, FileMode.Create);

            IntArithmeticCompressor3 compressor = new(f1, f2);
            DateTime start = DateTime.Now;
            compressor.Decode();
            Console.WriteLine((DateTime.Now - start).Seconds);
            Console.WriteLine(f2.Length / (double)f1.Length);
            f1.Close();
            f2.Close();
#else
            FileStream f1 = new(Origin, FileMode.Open);
            FileStream f2 = new(Encoded, FileMode.Create);

            IntArithmeticCompressor3 compressor = new(f1, f2);
            DateTime start = DateTime.Now;
            compressor.Encode();
            Console.WriteLine((DateTime.Now - start).TotalSeconds);
            Console.WriteLine(f2.Length / (double)f1.Length);
            f1.Close();
            f2.Close();
#endif
        }
    }
}