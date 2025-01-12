namespace Marar.Core.Compress.LZA
{
    internal class LZ78 : FileProcessor
    {
        private byte CodeLength;
        private ushort DictionaryLength = 0;
        private byte[][] OrderDictionary;
        private uint MaxDictionaryLength;

        public LZ78(Stream input, Stream output, byte codeLength = 16) : base(input, output)
        {
            CodeLength = codeLength;
            MaxDictionaryLength = (uint)MathF.Pow(2, CodeLength);
            OrderDictionary = new byte[MaxDictionaryLength][];
        }

        private void AddOrder(byte[] data)
        {
            if (DictionaryLength >= MaxDictionaryLength)
            {
#if DEBUG
                Console.WriteLine($"{Output.Position / (double)Input.Position} in {Input.Position} {MathF.Floor(Input.Position * 100 / Input.Length)}%");
#endif
                DictionaryLength = 0;
            }

            OrderDictionary[DictionaryLength] = data;
            DictionaryLength++;
        }
        public override void Encode()
        {
            BitStream bitStream = new(Output);
            List<byte> order = new();
            Predicate<byte[]> predicate = new(arr => Enumerable.SequenceEqual(arr ?? [], order.ToArray()));
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
                bitStream.Write((ulong)(tempCode << 8 | order[^1]), (byte)(CodeLength + 8));
                AddOrder(order.ToArray());
                order.Clear();
                tempCode = 0;
            }

            bitStream.FlushWrite();
        }
    }
}
