namespace MararCore0.Compress.LZA
{
    public class LZ78 : FileProcessor
    {
        private byte CodeLength;
        private ushort DictionaryLength = 0;
        private byte[][] OrderDictionary;
        private uint MaxDictionaryLength;

        public LZ78(Stream input, Stream output, byte codeLength = 12) : base(input, output)
        {
            CodeLength = codeLength;
            MaxDictionaryLength = (uint)MathF.Pow(2, CodeLength);
            OrderDictionary = new byte[MaxDictionaryLength][];
        }

        private ushort AddOrder(byte[] data)
        {
            if (DictionaryLength >= MaxDictionaryLength)
            {
                Console.WriteLine($"{Output.Position / (double)Input.Position} in {Input.Position}");
                DictionaryLength = 0;
            }
                
            OrderDictionary.Add(data);
            return (ushort)OrderDictionary.Count;
        }
        public override void Encode()
        {
            BitStream bitStream = new(Output);
            List<byte> order = new();
            Predicate<byte[]> predicate = new(arr => Enumerable.SequenceEqual(arr, order.ToArray()));
            ushort tempCode = 0;

            while (Input.Position < Input.Length)
            {
                order.Add((byte)Input.ReadByte());
                int longIndex = Array.FindIndex(OrderDictionary, predicate);
                if (longIndex >= 0)
                {
                    tempCode = (ushort)longIndex;
                    continue;
                }
                bitStream.Write((ulong)(tempCode << CodeLength | order[^1]), (byte)(CodeLength + 8));
                AddOrder(order.ToArray());
                order.Clear();
                tempCode = 0;
            }

            bitStream.FlushWrite();
        }
    }
}
