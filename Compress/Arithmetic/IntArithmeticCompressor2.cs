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

        public IntArithmeticCompressor2(Stream input, Stream output, byte codeLength = 8) : base(input, output)
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
            Input.Position = 0;           
        }
        private void InitFrequencyLows()
        {
            ulong lastLow = 0;

            for (ushort i = 0; i < 256; i++)
            {
                FrequencyLows[i] = lastLow;
                lastLow += FrequencyDictionary[i];
            }
            FrequencyLows[256] = lastLow;
        }
        private void ImproveFrequency()
        {
            double ratio = (double)MaxLength / SourceLength;
            for (short i = 0; i < 256; i++)
            {
                FrequencyDictionary[i] = (ulong)Math.Round(FrequencyDictionary[i] * ratio);
            }
        }
        private void InitProbability()
        {
            for (short i = 0; i < 256; i++)
            {
                ProbabilityDictionary[i] = FrequencyDictionary[i] / (double)MaxLength;
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
                RatioDictionary[i] = (double)MaxLength / FrequencyDictionary[i];
            }
        }
        private double GetCode(byte symbol, double code, double length)
        {
            return code + length * ProbabilityLows[symbol];
        }
        private double ImproveCode(byte symbol, double code)
        {
            return RatioDictionary[symbol] * (code - FrequencyLows[symbol]);
        }
        private double GetLength(byte symbol, double length)
        {
            return length * ProbabilityDictionary[symbol];
        }
        private byte GetSymbol(double code)
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
            double lastCode = 0;
            double code = 0;
            double length = MaxLength;
            byte symbol;
            short counter = -1;

            InitFrequency();
            WriteDictionary();
            ImproveFrequency();
            InitFrequencyLows();
            InitProbability();

            BitStream bitStream = new(Output);

            while (Input.Position < Input.Length)
            {
                symbol = (byte)Input.ReadByte();
                lastCode = code;
                code = GetCode(symbol, lastCode, length);
                length = GetLength(symbol, length);

                if (length < 1 || counter >= 256)
                {
                    bitStream.Write((ulong)counter, 8);
                    bitStream.Write((ulong)Math.Round(lastCode), CodeLength);
                    code = GetCode(symbol, 0, MaxLength);
                    length = GetLength(symbol, MaxLength);
                    counter = 0;
                }
                else counter++;
            }
            bitStream.Write((ulong)counter, 8);
            bitStream.Write((ulong)Math.Round(code), CodeLength);
            bitStream.FlushWrite();
        }

        public void Decode()
        {
            ReadDictionary();
            for (short i = 0; i < 256; i++)
            {
                SourceLength += FrequencyDictionary[i];
            }
            ImproveFrequency();
            InitFrequencyLows();
            InitProbability();
            InitRatio();
            BitStream bitStream = new(Input);
            bitStream.StartRead();

            while (Input.Position < Input.Length)
            {
                ushort counter = (ushort)bitStream.Read(8);
                double code = bitStream.Read(CodeLength);
                byte symbol = GetSymbol(code);
                Output.WriteByte(symbol);

                while (counter != 0)
                {
                    code = ImproveCode(symbol, code);
                    symbol = GetSymbol(code);
                    counter--;
                    Output.WriteByte(symbol);
                }
                Output.Flush();
            }
        }
    }
}
