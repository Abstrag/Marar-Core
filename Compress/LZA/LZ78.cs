namespace MararCore0.Compress.LZA
{
    public class LZ78 : FileProcessor
    {
        private byte CodeLength;
        private List<byte[]> OrderDictionary = new();
        private uint DictionaryLength;

        public LZ78(Stream input, Stream output, byte codeLength = 12) : base(input, output)
        {
            CodeLength = codeLength;
            DictionaryLength = (uint)MathF.Pow(2, CodeLength);
        }

        private ushort AddOrder(byte[] data)
        {
            if (OrderDictionary.Count >= DictionaryLength)
            {
                OrderDictionary.Clear();
            }

            OrderDictionary.Add(data);
            return (ushort)OrderDictionary.Count;
        }
        public override void Encode()
        {
            BitStream bitStream = new(Output);
            List<byte> order = new();
            Predicate<byte[]> predicate = new(arr => arr.SequenceEqual(order.ToArray()));
            ushort tempCode = 0;

            while (Input.Position < Input.Length)
            {
                order.Add((byte)Input.ReadByte());
                int longIndex = OrderDictionary.FindIndex(predicate);
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
