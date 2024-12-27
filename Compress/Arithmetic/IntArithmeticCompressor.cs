namespace MararCore.Compress.Arithmetic
{
    public class IntArithmeticCompressor : FileProcessor
    {
        private readonly byte CodeLength;
        private readonly ulong MaxCode = 1;
        private readonly ulong[] Lengths = new ulong[256];
        private readonly ulong[] FrequencyDictionary = new ulong[256];

        public IntArithmeticCompressor(Stream input, Stream output, byte codeLength = 32) : base(input, output)
        {
            CodeLength = codeLength;
            for (byte i = 0; i < CodeLength; i++)
            {
                MaxCode *= 2;
            }
        }

        private void ReadDictionary()
        {
            for (short i = 0; i < 256; i++)
            {
                byte[] buffer = new byte[8];
                Input.Read(buffer);
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
        private void InitFrequencyDictionary()
        {
            while (Input.Position < Input.Length)
            {
                FrequencyDictionary[Input.ReadByte()]++;
            }
        }
        private void InitLengths()
        {
            ulong lastLength = 0;

            for (ushort i = 0; i < 256; i++)
            {
                Lengths[i] = lastLength;
                lastLength += FrequencyDictionary[i];
            }
        }
        private Tuple<ulong, ulong> GetRange(Tuple<ulong, ulong> baseRange, byte symbol)
        {
            double step = (baseRange.Item2 - baseRange.Item1) / (double)Input.Length;
            ulong low = (ulong)Math.Round(baseRange.Item1 + Lengths[symbol] * step);
            return new(low, low + (ulong)Math.Round(FrequencyDictionary[symbol] * step));
        }
        public override void Encode()
        {
            BitStream bitStream = new(Output);
            Tuple<ulong, ulong> currentRange = new(0, MaxCode);
            
            InitFrequencyDictionary();
            InitLengths();
            WriteDictionary();
            Input.Position = 0;
            
            while (Input.Position < Input.Length)
            {
                if (currentRange.Item2 - currentRange.Item1 > 0)
                {
                    currentRange = GetRange(currentRange, (byte)Input.ReadByte());
                }
                else
                {
                    bitStream.Write(currentRange.Item1, CodeLength);
                    currentRange = new(0, MaxCode);
                }
            }
            
            bitStream.Write(currentRange.Item1, CodeLength);
            bitStream.FlushWrite();
        }

        public void Decode()
        {
            BitStream bitStream = new(Input);
            Tuple<ulong, ulong> range = new(0, 0);
            ulong code = 0;
            
            ReadDictionary();
            InitLengths();
            bitStream.StartRead();

            while (Input.Position < Input.Length)
            {
                if (range.Item2 - range.Item1 <= 0)
                {
                    range = new(0, MaxCode); 
                    code = bitStream.Read(CodeLength);
                    continue;
                }
                
                for (ushort i = 0; i < 256; i++)
                {
                    if (code < Lengths[i]) continue;
                    Output.WriteByte((byte)i);
                    range = GetRange(range, (byte)i);
                    break;
                }
            }
        }
    }
}
