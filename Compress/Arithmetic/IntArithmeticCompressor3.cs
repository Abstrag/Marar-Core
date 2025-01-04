namespace MararCore.Compress.Arithmetic
{
    public class IntArithmeticCompressor3 : FileProcessor
    {
        private readonly byte CodeLength;
        private readonly ulong MaxCode = 1;
        private readonly ulong MaxLength = 1;
        private readonly ulong[] FrequencyDictionary = new ulong[256];
        private ulong SourceLength = 0;

        public IntArithmeticCompressor3(Stream input, Stream output, byte codeLength = 8) : base(input, output)
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
        private Tuple<ulong, ulong>[] GetDictionary(Tuple<ulong, ulong> range)
        {
            Tuple<ulong, ulong>[] result = new Tuple<ulong, ulong>[256];
            Tuple<double, double>[] floatDictionary = new Tuple<double, double>[256];
            double step = (double)range.Item2 / SourceLength;
            double low = range.Item1;

            for (short i = 0; i < 256; i++)
            {
                double length = step * FrequencyDictionary[i];
                floatDictionary[i] = new(low, length);
                low += length;
            }
            for (short i = 0; i < 256; i++)
            {
                result[i] = new((ulong)Math.Round(floatDictionary[i].Item1), (ulong)Math.Round(floatDictionary[i].Item1));
            }

            return result;
        }
        private byte GetSymbol(Tuple<ulong, ulong>[] dicitonary, ulong code)
        {
            for (short i = 0; i < 256; i++)
            {
                if (dicitonary[i].Item1 <= code && code < dicitonary[i].Item1 + dicitonary[i].Item2)
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
            ulong lastLow = 0;
            Tuple<ulong, ulong> range = new(0, MaxLength);
            Tuple<ulong, ulong>[] dictionary = GetDictionary(range);

            InitFrequency();
            WriteDictionary();

            while (Input.Position < Input.Length)
            {
                Logging.WriteLine($"{range.Item1}:{range.Item2 + range.Item1} -> {range.Item2}");
                byte symbol = (byte)Input.ReadByte();

                if (range.Item2 <= 0)
                {
                    Logging.WriteLine($"Written: {lastLow}");
                    bitStream.Write(lastLow, CodeLength);
                    range = new(0, MaxLength);
                }
                else lastLow = range.Item1;

                dictionary = GetDictionary(range);
                range = dictionary[symbol];
            }
            if (lastLow > 0)
            {
                bitStream.Write(lastLow, CodeLength);
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
            BitStream bitStream = new(Input);
            Tuple<ulong, ulong> range = new(0, MaxLength);
            Tuple<ulong, ulong>[] dictionary = GetDictionary(range);
            bitStream.StartRead();

            while (Input.Position < Input.Length)
            {
                ulong code = bitStream.Read(CodeLength);

                while (range.Item2 > 0)
                {
                    dictionary = GetDictionary(range);
                    byte symbol = GetSymbol(dictionary, code);
                    range = dictionary[symbol];
                    Output.WriteByte(symbol);
                    Output.Flush();
                }

                range = new(0, MaxLength);
            }
        }
    }
}
