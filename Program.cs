using MararCore.Compress.LZA;

namespace MararCore
{
    public class Program
    {
        private static string Origin = @"Y:\Users\bar32\Pictures\луна2.png";
        private static string Encoded = @"Y:\Users\bar32\Pictures\sample.bin";
        private static string Decoded = @"Y:\Users\bar32\Pictures\figna.bin";

        public static void Main()
        {
            FileStream input = new(Origin, FileMode.Open);
            FileStream output = new(Encoded, FileMode.Create);
            FileProcessor processor = new LZ78(input, output);
            processor.Encode();
            Console.WriteLine($"{input.Length} -> {output.Length}");
            input.Close();
            output.Close();

            /*Console.WriteLine(Enumerable.SequenceEqual([97, 98], [97, 98]));
            List<byte[]> dictionary = new();

            dictionary.Add([0, 91, 68]);
            dictionary.Add([0, 91, 64]);

            byte[] line = [0, 91, 61];

            Predicate<byte[]> predicate = new(arr => Enumerable.SequenceEqual(arr, line));
            Console.WriteLine(dictionary.FindIndex(predicate));*/
        }
    }
}