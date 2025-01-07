namespace MararCore.Compress.Haffman
{
    file struct BinaryCode(ulong code, byte bitsCount)
    {
        public ulong Code = code;
        public byte BitsCount = bitsCount;
    }

    public class HaffmanCompressor(Stream input, Stream output) : FileProcessor(input, output)
    {
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
            for (short i = 0; i < 256; i++)
            {
                nodes[i].Item.Frequency = FrequencyDictionary[i];
                nodes[i].Item.Symbol = (byte)i;
            }
            return nodes;
        }
        private List<BinaryNode> SortNodes(List<BinaryNode> nodeBuffer)
        {
            BinaryNode tempNode;
            for (short i = 1; i < nodeBuffer.Count; i++)
            {
                if (nodeBuffer[i - 1].Item.Frequency > nodeBuffer[i].Item.Frequency)
                {
                    tempNode = nodeBuffer[i];
                    nodeBuffer[i] = nodeBuffer[i - 1];
                    nodeBuffer[i - 1] = tempNode;
                }
            }
            return nodeBuffer;
        }
        
        public override void Encode()
        {
            InitFrequency();
            List<BinaryNode> nodeBuffer = GetNodes();
            while (nodeBuffer.Count > 0) 
            {
                nodeBuffer = SortNodes(nodeBuffer);
                BinaryNode left = nodeBuffer[0];
                BinaryNode right = nodeBuffer[1];
                nodeBuffer.Add(new(left, right, new()));
                nodeBuffer.RemoveAt(0);
                nodeBuffer.RemoveAt(1);
            }
        }
    }
}
