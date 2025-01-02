namespace MararCore.Compress.Arithmetic
{
    public class IntArithmeticCompressor2 : FileProcessor
    {
        private readonly byte CodeLength;
        private readonly ulong MaxCode = 1;
        private readonly ulong MaxLength = 1;
        private readonly ulong[] FrequencyDictionary = new ulong[256];
        private readonly ulong[] FrequencyLows = new ulong[257];
        private readonly double[] ProbabilityDictionary = new double[256];
        private readonly double[] ProbabilityLows = new double[257];
        private readonly double[] RatioDictionary = new double[256];
        private ulong SourceLength = 0;

        public IntArithmeticCompressor2(Stream input, Stream output, byte codeLength = 32) : base(input, output)
        {
            CodeLength = codeLength;
            for (byte i = 0; i < CodeLength; i++)
            {
                MaxCode *= 2;
            }
            MaxLength = MaxCode;
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
        private void InitFrequency()
        {
            while (Input.Position < Input.Length)
            {
                FrequencyDictionary[Input.ReadByte()]++;
            }

            ulong lastLow = 0;

            for (ushort i = 0; i < 256; i++)
            {
                FrequencyLows[i] = lastLow;
                lastLow += FrequencyDictionary[i];
            }
            FrequencyLows[256] = lastLow;
        }
        private void InitProbability()
        {
            for (short i = 0; i < 256; i++)
            {
                ProbabilityDictionary[i] = FrequencyDictionary[i] / SourceLength;
            }

            double lastLow = 0;
            for (ushort i = 0; i < 256; i++)
            {
                ProbabilityLows[i] = lastLow;
                lastLow += ProbabilityDictionary[i];
            }
            ProbabilityLows[256] = lastLow;
        }
        private void InitRatio()
        {
            for (short i = 0; i < 256; i++)
            {
                ProbabilityDictionary[i] = MaxLength / FrequencyDictionary[i];
            }
        }
        private ulong GetCode(byte symbol, ulong code, ulong length)
        {
            return code + (ulong)Math.Round(length * ProbabilityLows[symbol]);
        }
        private ulong ImproveCode(byte symbol, ulong code, ulong low)
        {
            return (ulong)Math.Round(RatioDictionary[symbol] * (code - low));
        }
        private ulong GetLength(ulong symbol, ulong length)
        {
            return (ulong)Math.Round(length * ProbabilityDictionary[symbol]);
        }
        private byte GetSymbol(byte symbol, ulong code)
        {
            for (short i = 0; i < 256; i++)
            {
                if (FrequencyLows[i] <= code && code < FrequencyLows[i + 1])
                {
                    return (byte)i;
                }
            }
            throw new Exception("Жопа");
        }

        public override void Encode()
        {
            SourceLength = (ulong)Input.Length;
            ulong code = 0;
            ulong length = MaxLength;
            byte symbol;
            BitStream bitStream = new(Output);

            InitFrequency();
            InitProbability();
            WriteDictionary();

            while (Input.Position < Input.Length)
            {
                symbol = (byte)Input.ReadByte();
                length = GetLength(symbol, length);

                if (length <= 0)
                {
                    bitStream.Write(code, CodeLength);
                    code = 0;
                    length = MaxLength;
                }
                    
                code = GetCode(symbol, code, length);
                length = GetLength(symbol, length);
            }
            bitStream.Write(code, CodeLength);
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
            InitRatio();
            BitStream bitStream = new(Output);
            bitStream.StartRead();

            while (Input.Position < Input.Length)
            {
                ulong code = BitConverter.ToUInt64(Input.ReadBytes(8));

            }
        }
    }
}
