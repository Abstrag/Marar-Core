namespace MararCore.Compress.Haffman
{
    public class HaffmanCompressor(Stream input, Stream output) : FileProcessor(input, output)
    {
        private struct NodeItem(byte symbol, ulong frequency)
        {
            public byte Symbol = symbol;
            public ulong Frequency = frequency;
        }

        private readonly ulong[] FrequencyDictionary = new ulong[256];

        private void InitFrequency()
        {
            while (Input.Position < Input.Length)
            {
                FrequencyDictionary[Input.ReadByte()]++;
            }
            Input.Position = 0;
        }
        private List<BinaryNode> GetNodes()
        {
            List<BinaryNode> nodes = new(256);
            Dictionary<byte, ulong> frequency = new(256);
            for (short i = 0; i < 256; i++)
            {
                frequency[(byte)i] = FrequencyDictionary[i];
            }
            NodeItem getMaxFrequency()
            {
                NodeItem result = new(0, 0);
                for (short i = 0; i < frequency.Count; i++)
                {
                    if (frequency[(byte)i] > result.Frequency)
                    {
                        result.Symbol = (byte)i;
                        result.Frequency = frequency[(byte)i];
                    }
                }
                frequency.Remove(result.Symbol);
                return result;
            }
            for (short i = 0; i < 256; i++)
            {
                nodes[i].Item = getMaxFrequency();
            }
            return nodes;
        }
        public override void Encode()
        {
            InitFrequency();
            List<BinaryNode> nodeBuffer = GetNodes();
            while (nodeBuffer.Count > 1)
            {
                //nodeBuffer.Add(nodeBuffer);
            }
        }
    }
}
