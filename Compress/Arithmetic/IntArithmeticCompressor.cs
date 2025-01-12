namespace MararCore.Compress.Arithmetic
{
    internal class IntArithmeticCompressor : FileProcessor
    {
        private readonly byte CodeLength;
        private readonly ulong MaxCode = 1;
        private readonly ulong[] Lengths = new ulong[257];
        private readonly ulong[] FrequencyDictionary = new ulong[256];
        private ulong SourceLength = 0;

        public IntArithmeticCompressor(Stream input, Stream output, byte codeLength = 32) : base(input, output)
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
            Lengths[256] = lastLength;
        }
        private Tuple<ulong, ulong> GetRange(Tuple<ulong, ulong> baseRange, byte symbol)
        {
            double step = (baseRange.Item2 - baseRange.Item1) / (double)SourceLength;
            ulong low = (ulong)Math.Round(baseRange.Item1 + Lengths[symbol] * step);
            return new(low, low + (ulong)Math.Round(FrequencyDictionary[symbol] * step));
        }
        private byte GetSymbol(Tuple<ulong, ulong> baseRange, ulong code)
        {
            //ulong l = SourceLength * (baseRange.Item1 - code) / (baseRange.Item2 - baseRange.Item1);
            //code -= baseRange.Item1;
            //code = (ulong)(code * SourceLength / (double)(baseRange.Item2 - baseRange.Item1));
            double trueCode = (code - baseRange.Item1) * (double)SourceLength / (baseRange.Item2 - baseRange.Item1);
            short symbol = -1;

            for (short i = 0; i < 256; i++)
            {
                if (Lengths[i] <= trueCode && trueCode < Lengths[i + 1])
                    symbol = i;
            }
            Output.Flush();
            if (symbol > -1)
                return (byte)symbol;
            throw new Exception("Жопа");
        }
        public override void Encode()
        {
            SourceLength = (ulong)Input.Length;
            BitStream bitStream = new(Output);
            Tuple<ulong, ulong> lastRange = new(0, MaxCode);
            Tuple<ulong, ulong> currentRange = new(0, MaxCode);
            
            InitFrequencyDictionary();
            InitLengths();
            WriteDictionary();
            Input.Position = 0;
            
            while (Input.Position < Input.Length)
            {
                byte symbol = (byte)Input.ReadByte();
                Logging.WriteLine($"{currentRange.Item1}:{currentRange.Item2} {symbol}");

                if (currentRange.Item2 - currentRange.Item1 <= 1)
                {
                    Logging.WriteLine($"W:{lastRange.Item1}");
                    bitStream.Write(lastRange.Item1, CodeLength);
                    lastRange = new(0, MaxCode);
                }
                else
                {
                    lastRange = currentRange;
                }
                currentRange = GetRange(lastRange, symbol);
            }
            if (currentRange.Item2 - currentRange.Item1 != 0)
            {
                bitStream.Write(currentRange.Item1, CodeLength);
            }

            bitStream.FlushWrite();
        }

        public void Decode()
        {
            BitStream bitStream = new(Input);
            Tuple<ulong, ulong> range = new(0, MaxCode);
            ulong code;
            byte symbol;
            
            ReadDictionary();
            for (short i = 0; i < 256; i++)
            {
                SourceLength += FrequencyDictionary[i];
            }
            InitLengths();
            bitStream.StartRead();

            while (Input.Position < Input.Length)
            {
                range = new(0, MaxCode);
                code = bitStream.Read(CodeLength);

                while (range.Item2 - range.Item1 > 1)
                {
                    symbol = GetSymbol(range, code);
                    Output.WriteByte(symbol);
                    Output.FlushAsync();
                    range = GetRange(range, symbol);
                    Logging.WriteLine($"{range.Item1}:{range.Item2} {symbol}");
                }
            }
        }
    }
}
