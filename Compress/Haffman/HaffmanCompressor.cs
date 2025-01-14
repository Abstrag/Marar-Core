namespace Marar.Core.Compress.Haffman
{
    internal struct BinaryCode(ulong code, byte bitsCount)
    {
        public ulong Code = code;
        public byte BitsCount = bitsCount;
    }

    internal class HaffmanCompressor(Stream input, Stream output) : FileProcessor(input, output)
    {
        private readonly ulong[] FrequencyDictionary = new ulong[256];

        private void ReadDictionary()
        {
            for (short i = 0; i < 256; i++)
            {
                byte[] buffer = new byte[8];
                Input.ReadExactly(buffer);
                FrequencyDictionary[i] = BitConverter.ToUInt64(buffer);
            }
        }
        private void WriteDictionary()
        {
            foreach (ulong key in FrequencyDictionary)
            {
                Output.Write(BitConverter.GetBytes(key));
            }
        }
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
            List<BinaryNode> nodes = new();
            for (short i = 0; i < 256; i++)
                nodes.Add(new(new((byte)i, FrequencyDictionary[i])));
            return nodes;
        }
        private BinaryNode GetRoot()
        {
            List<BinaryNode> nodeBuffer = GetNodes();
            BinaryNode left;
            BinaryNode right;

            while (nodeBuffer.Count > 1)
            {
                nodeBuffer = SortNodes(nodeBuffer);
                //Debug(nodeBuffer);
                left = nodeBuffer[0];
                right = nodeBuffer[1];
                nodeBuffer.RemoveAt(0);
                nodeBuffer.RemoveAt(0);
                nodeBuffer.Add(new(left, right, new(left.Item.Frequency + right.Item.Frequency)));
            }

            return nodeBuffer[0];
        }
        private static List<BinaryNode> SortNodes(List<BinaryNode> nodeBuffer)
        {
            BinaryNode tempNode;
            for (short i = 0; i < nodeBuffer.Count; i++)
            {
                for (short j = 0; j < nodeBuffer.Count; j++)
                {
                    if (nodeBuffer[i].Item.Frequency < nodeBuffer[j].Item.Frequency)
                    {
                        tempNode = nodeBuffer[j];
                        nodeBuffer[j] = nodeBuffer[i];
                        nodeBuffer[i] = tempNode;
                    }
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
                    address.Add(false);
                    setCode(node.Left, address);
                    address.RemoveAt(address.Count - 1);
                }
                if (node.Right != null)
                {
                    address.Add(true);
                    setCode(node.Right, address);
                    address.RemoveAt(address.Count - 1);
                }
            }
            setCode(root, []);
            return codes;
        }
        
        /*private static string Convert(BinaryCode code)
        {
            string result = "";
            for (short i = code.BitsCount; i > 0; i--)
            {
                ulong a = code.Code & ((ulong)1 << i);
                if (a == 0) result += '0';
                else result += '1';
            }
            return result;
        }*/

        public override void Encode()
        {
            InitFrequency();
            WriteDictionary();

            BitStream bitWriter = new(Output);
            BinaryCode[] codes = GetCodes(GetRoot());
            
            while (Input.Position < Input.Length)
            {
                byte symbol = (byte)Input.ReadByte();
                //Logging.Write(Convert(codes[symbol]) + ' ');
                bitWriter.Write(codes[symbol].Code, codes[symbol].BitsCount);
            }
            bitWriter.FlushWrite();
        }
        public void Decode()
        {
            ReadDictionary();

            BinaryNode tempNode;
            BinaryNode root = GetRoot();
            BitStream reader = new(Input);
            reader.StartRead();

            while (true)
            {
                tempNode = root;
                while (true)
                {
                    if (tempNode.IsLeaf)
                    {
                        //Logging.Write(" ");
                        Output.WriteByte(tempNode.Item.Symbol);
                        break;
                    }
                    else
                    {
                        byte bit;
                        try
                        {
                            bit = reader.ReverseReadBit();
                        }
                        catch
                        {
                            break;
                        }                        
                        //if (bit == 0) Logging.Write(0.ToString());
                        //else Logging.Write(1.ToString());
                        tempNode = tempNode.GetNode(bit);
                    }
                }
            }

            Output.Flush();
            /*BinaryCode[] codes = GetCodes(root);
            byte maxLength = 0;
            byte minLength = 255;
            for (short i = 0; i < 256; i++)
            {
                if (codes[i].BitsCount > maxLength) maxLength = codes[i].BitsCount;
                if ((codes[i].BitsCount + 1) < minLength) minLength = codes[i].BitsCount;
            }
            minLength--;*/

        }
    }
}
