namespace MararCore.Compress.Arithmetic
{
    public class IntArithmeticCompressorNew : FileProcessor
    {
        private readonly byte CodeLength;
        private readonly ulong MaxCode = 1;
        private readonly double[] Lengths = new double[257];
        private readonly ulong[] FrequencyDictionary = new ulong[256];
        private readonly double[] ProbabilityDictionary = new double[256];
        private ulong SourceLength = 0;

        public IntArithmeticCompressorNew(Stream input, Stream output, byte codeLength = 32) : base(input, output)
        {
            CodeLength = codeLength;
            for (byte i = 0; i < CodeLength; i++)
            {
                MaxCode *= 2;
            }
            MaxCode--;
        }

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
        private void InitProbability()
        {
            for (short i = 0; i < 256; i++)
            {
                ProbabilityDictionary[i] = (double)FrequencyDictionary[i] / SourceLength;
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
            double lastLength = 0;

            for (ushort i = 0; i < 256; i++)
            {
                Lengths[i] = lastLength;
                lastLength += ProbabilityDictionary[i];
            }
            Lengths[256] = lastLength;
        }
        private Tuple<ulong, ulong> GetRange(Tuple<ulong, ulong> baseRange, byte symbol)
        {
            return new(baseRange.Item1 + (ulong)Math.Round(baseRange.Item2 * Lengths[symbol]), 
                (ulong)Math.Round(baseRange.Item2 * ProbabilityDictionary[symbol]));
        }
        private double ConvertCode(Tuple<ulong, ulong> range, double code)
        {
            code -= range.Item1;
            return code / range.Item2;
        }
        private byte GetSymbol(Tuple<ulong, ulong> range, double code)
        {
            for (short i = 0; i < 256; i++)
            {
                if (Lengths[i] <= code && code < Lengths[i + 1])
                {
                    return (byte)i;
                }
            }
            throw new Exception("Жопа");
        }
        
        public override void Encode()
        {
            SourceLength = (ulong)Input.Length;
            BitStream bitStream = new(Output);

            Tuple<ulong, ulong> lastRange = new(0, MaxCode);
            Tuple<ulong, ulong> range = new(0, MaxCode);

            InitFrequencyDictionary();
            InitProbability();
            InitLengths();
            WriteDictionary();
            Input.Position = 0;
            
            while (Input.Position < Input.Length)
            {
                byte symbol = (byte)Input.ReadByte();
                range = GetRange(lastRange, symbol);

                if (range.Item2 == 0)
                {
                    bitStream.Write(lastRange.Item1, CodeLength);
                    range = GetRange(new(0, MaxCode), symbol);
                }

                lastRange = range;
            }
            if (range.Item2 != 0 || range.Item2 != MaxCode)
            {
                bitStream.Write(range.Item1, CodeLength);
            }

            bitStream.FlushWrite();
        }
        public void Decode()
        {
            ReadDictionary();
            for (short i = 0; i < 256; i++)
            {
                SourceLength += FrequencyDictionary[i];
            }
            InitProbability();
            InitLengths();

            double code;
            Tuple<ulong, ulong> range = new(0, MaxCode);
            BitStream bitStream = new(Input);
            bitStream.StartRead();

            while (Input.Position < Input.Length)
            {
                code = bitStream.Read(CodeLength);
                
                while (range.Item2 > 0)
                {
                    code = ConvertCode(range, code);
                    byte symbol = GetSymbol(range, code);
                    range = GetRange(range, symbol);
                    Output.WriteByte(symbol);
                }

                range = new(0, MaxCode);
            }
        }
    }
}
