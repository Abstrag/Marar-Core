using MararCore.Compress.Haffman;

namespace MararCore
{
    public class Program
    {
        private static BinaryCode[] GetCodes(BinaryNode root)
        {
            BinaryCode[] codes = new BinaryCode[5];

            BinaryCode convert(List<bool> address)
            {
                BinaryCode result = new();
                result.BitsCount = (byte)address.Count;
                for (byte i = 0; i < result.BitsCount; i++)
                {
                    if (address[i]) result.Code |= (ulong)1 << i;
                }
                return result;
            }
            void setCode(BinaryNode node, List<bool> address)
            {
                if (node.IsLeaf)
                {
                    codes[node.Item.Symbol] = convert(address);
                    return;
                }
                if (node.Left != null)
                {
                    List<bool> local = address.ToList();
                    local.Add(false);
                    setCode(node, local);
                }
                if (node.Right != null)
                {
                    List<bool> local = address.ToList();
                    local.Add(true);
                    setCode(node, local);
                }
            }
            setCode(root, []);
            return codes;
        }

        #if true
        private static string Origin = @"Y:\Users\bar32\Pictures\маруся.jpeg";
        private static string Encoded = @"Y:\Users\bar32\Pictures\sample.bin";
        private static string Decoded = @"Y:\Users\bar32\Pictures\figna.bin";
        #else
        private static string Origin = "/home/admen/Рабочий стол/MararTest/466893-vbig.png";
        private static string Encoded = "/home/admen/Рабочий стол/MararTest/f2.bin";
        private static string Decoded = "/home/admen/Рабочий стол/MararTest/f3.bin";
        #endif

        public static void Main()
        {
            /*BinaryNode node = new();
            node.Left = new(new((byte)'a', 5));
            node.Right = new();*/
#if true
#if false
            FileStream f1 = new(Encoded, FileMode.Open);
            FileStream f2 = new(Decoded, FileMode.Create);

            HaffmanCompressor compressor = new(f1, f2);
            DateTime start = DateTime.Now;
            compressor.Decode();
            Console.WriteLine((DateTime.Now - start).Seconds);
            Console.WriteLine(f2.Length / (double)f1.Length);
            f1.Close();
            f2.Close();
#else
            FileStream f1 = new(Origin, FileMode.Open);
            FileStream f2 = new(Encoded, FileMode.Create);

            HaffmanCompressor compressor = new(f1, f2);
            DateTime start = DateTime.Now;
            compressor.Encode();
            Console.WriteLine((DateTime.Now - start).TotalSeconds);
            Console.WriteLine(f2.Length / (double)f1.Length);
            f1.Close();
            f2.Close();
#endif
#endif
        }
    }
}