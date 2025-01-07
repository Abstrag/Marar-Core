namespace MararCore.Compress.Haffman
{
    internal struct BinaryCode(ulong code, byte bitsCount)
    {
        public ulong Code = code;
        public byte BitsCount = bitsCount;
    }

    public class HaffmanCompressor(Stream input, Stream output) : FileProcessor(input, output)
    {
        private readonly BitStream BitWriter = new(output);
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
        private static List<BinaryNode> SortNodes(List<BinaryNode> nodeBuffer)
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
        private static BinaryCode[] GetCodes(BinaryNode root)
        {
            BinaryCode[] codes = new BinaryCode[256];

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
        
        public override void Encode()
        {
            InitFrequency();

            List<BinaryNode> nodeBuffer = GetNodes();
            BinaryNode left;
            BinaryNode right;

            while (nodeBuffer.Count > 1) 
            {
                nodeBuffer = SortNodes(nodeBuffer);
                left = nodeBuffer[0];
                right = nodeBuffer[1];
                nodeBuffer.Add(new(left, right, new(left.Item.Frequency + right.Item.Frequency)));
                nodeBuffer.RemoveAt(0);
                nodeBuffer.RemoveAt(1);
            }

            BinaryCode[] codes = GetCodes(nodeBuffer[0]);
            
            while (Input.Position < Input.Length)
            {
                byte symbol = (byte)Input.ReadByte();
                BitWriter.Write(codes[symbol].Code, codes[symbol].BitsCount);
            }
            BitWriter.FlushWrite();
        }
    }
}
