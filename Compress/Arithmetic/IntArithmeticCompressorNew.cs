namespace Marar.Core.Compress.Arithmetic
{
    internal class IntArithmeticCompressorNew : FileProcessor
    {
        private readonly byte CodeLength;
        private readonly ulong MaxCode = 1;
        private readonly ulong[] FrequencyDictionary = new ulong[256];
        private readonly double[] ProbabilityDictionary = new double[256];
        private readonly double[] ProbabilityLengths = new double[257];
        private readonly double[] RatioLengths = new double[256];
        private readonly double[] Lengths = new double[257];
        private readonly double[] Lows = new double[257];
        private ulong SourceLength = 0;

        public IntArithmeticCompressorNew(Stream input, Stream output, byte codeLength = 8) : base(input, output)
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
                lastLength += ProbabilityDictionary[i] * MaxCode;
            }
            Lengths[256] = lastLength;
        }
        private void InitProbabilityLengths()
        {
            double lastLength = 0;

            for (ushort i = 0; i < 256; i++)
            {
                ProbabilityLengths[i] = lastLength;
                lastLength += ProbabilityDictionary[i];
            }
            ProbabilityLengths[256] = lastLength;
        }
        private void InitRatioLengths()
        {
            for (short i = 0; i < 256; i++)
            {
                RatioLengths[i] = MaxCode / Lows[i];
            }
        }
        private Tuple<ulong, ulong> GetRange(Tuple<ulong, ulong> baseRange, byte symbol)
        {
            return new(baseRange.Item1 + (ulong)Math.Round(baseRange.Item2 * ProbabilityLengths[symbol]), 
                (ulong)Math.Round(baseRange.Item2 * ProbabilityDictionary[symbol]));
        }
        private double GetLength(double length, byte symbol)
        {
            return length * ProbabilityDictionary[symbol];
        }
        private double ImproveCode(ulong code, double low, byte symbol)
        {
            return (code - low) * RatioLengths[symbol];
        }
        private byte GetSymbol(ulong code)
        {
            for (short i = 0; i < 256; i++)
            {
                if (Lows[i] <= code && code < Lows[i + 1])
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
            InitProbabilityLengths();
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
            InitProbabilityLengths();
            InitLengths();
            InitRatioLengths();

            ulong code;
            ulong length = MaxCode;
            BitStream bitStream = new(Input);
            bitStream.StartRead();

            while (Input.Position < Input.Length)
            {
                code = bitStream.Read(CodeLength);
                
                while (length > 0)
                {
                    byte symbol = GetSymbol(code);
                    code = (ulong)Math.Round(ImproveCode(code, Lows[symbol], symbol));
                    length = (ulong)Math.Round(GetLength(length, symbol));
                    Output.WriteByte(symbol);
                }

                length = MaxCode;
            }
        }
    }
}
