namespace MararCore.Compress.Arithmetic
{
    public class IntArithmeticCompressor3 : FileProcessor
    {
        private readonly byte CodeLength;
        private readonly ulong MaxCode = 1;
        private readonly ulong MaxLength = 1;
        private readonly ulong[] FrequencyDictionary = new ulong[256];
        private ulong SourceLength = 0;

        public IntArithmeticCompressor3(Stream input, Stream output, byte codeLength = 32) : base(input, output)
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
                result[i] = new((ulong)Math.Round(floatDictionary[i].Item1), (ulong)Math.Round(floatDictionary[i].Item2));
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
            InitFrequency();
            WriteDictionary();

            SourceLength = (ulong)Input.Length;
            BitStream bitStream = new(Output);
            ulong lastLow = 0;
            ushort counter = 0;
            byte symbol = 0;
            Tuple<ulong, ulong> range = new(0, MaxLength);

            while (Input.Position < Input.Length)
            {
                if (range.Item2 <= 1 || counter > 256)
                {
                    Logging.WriteLine($"Written: {lastLow}; Counter: {counter - 1}");
                    bitStream.Write((ulong)(counter - 1), 8);
                    bitStream.Write(lastLow, CodeLength);
                    range = GetDictionary(new(0, MaxLength))[symbol];
                    counter = 1;
                }
                else
                {
                    Logging.WriteLine($"{range.Item1}:{range.Item2 + range.Item1} -> {range.Item2}");
                    symbol = (byte)Input.ReadByte();
                    lastLow = range.Item1;
                    range = GetDictionary(range)[symbol];
                    counter++;
                }
            }
            if (lastLow > 0)
            {
                Logging.WriteLine($"Written: {lastLow}; Counter: {counter}");
                bitStream.Write(counter, 8);
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
            Tuple<ulong, ulong>[] dictionary;
            bitStream.StartRead();

            while (Input.Position < Input.Length)
            {
                ushort counter = (ushort)bitStream.Read(8);
                ulong code = bitStream.Read(CodeLength);

                while (range.Item2 > 0 && counter > 0)
                {
                    dictionary = GetDictionary(range);
                    byte symbol = GetSymbol(dictionary, code);
                    range = dictionary[symbol];
                    Output.WriteByte(symbol);
                    Output.Flush();
                    counter--;
                }

                range = new(0, MaxLength);
            }
        }
    }
}
