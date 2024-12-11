namespace MararCore0
{
    public class ArithmeticCompressor : FileProcessor
    {
        private Dictionary<byte, long> IntDictionary { get; set; } = new();
        private long SourceLength { get; set; } = 0;

        public ArithmeticCompressor(Stream input, Stream output) : base(input, output) { }

        private Dictionary<byte, Tuple<double, double>> GetRanges(Tuple<double, double> originRange)
        {
            Dictionary<byte, Tuple<double, double>> rangeDictionary = new();

            double lastBorder = originRange.Item1;
            double k = originRange.GetLength() / SourceLength;

            foreach (byte symbol in IntDictionary.Keys)
            {
                rangeDictionary.Add(symbol, new(lastBorder, lastBorder + IntDictionary[symbol] * k));
                lastBorder = rangeDictionary[symbol].Item2;
            }

            return rangeDictionary;
        }

        private void ComputeFrequence()
        {
            SourceLength = Input.Length;

            while (Input.Position < Input.Length)
            {
                byte symbol = (byte)Input.ReadByte();
                if (!IntDictionary.TryAdd(symbol, 1))
                    IntDictionary[symbol]++;
            }
        }
        private void WriteDictionary()
        {
            Output.WriteByte((byte)(IntDictionary.Count - 1));

            foreach (byte symbol in IntDictionary.Keys)
            {
                Output.WriteByte(symbol);
                Output.Write(BitConverter.GetBytes(IntDictionary[symbol]));
            }
        }
        public override void Encode()
        {
            ComputeFrequence();

            Input.Position = 0;
            WriteDictionary();

            Tuple<double, double> lastRange = new(0, 1);
            byte counter = 0;

            while (Input.Position < Input.Length)
            {
                if (lastRange.IsLastTrue() || counter == 255)
                {
                    if (counter == 0) throw new Exception("E1: Oops, eternal loop");
                    //Output.WriteByte(counter);
                    Output.Write(BitConverter.GetBytes(lastRange.Item1));
                    //Logging.WriteLine($"Block end. Counter: {counter} and Range: {lastRange.Item1}:{lastRange.Item2} in IPosition: {Input.Position}");
                    counter = 0;
                    lastRange = new(0, 1);
                }
                else
                {
                    byte symbol = (byte)Input.ReadByte();
                    lastRange = GetRanges(lastRange)[symbol];
                    counter++;
                }
            }

            Output.Write(BitConverter.GetBytes(lastRange.Item1));
        }

        private void ReadDictionary()
        {
            byte dictionaryLength = (byte)Input.ReadByte();

            for (ushort i = 0; i <= dictionaryLength; i++)
            {
                byte symbol = (byte)Input.ReadByte();
                long symbolFreq = BitConverter.ToInt64(Input.ReadBytes(8));
                SourceLength += symbolFreq;

                IntDictionary.Add(symbol, symbolFreq);
            }
        }
        private Tuple<double, double> GetRange(Dictionary<byte, Tuple<double, double>> floatDictionary, double num)
        {
            foreach (byte key in floatDictionary.Keys)
            {
                if (floatDictionary[key].ContainsIt(num)) return floatDictionary[key];
            }
            throw new Exception("E2");
        }
        public void Decode()
        {
            ReadDictionary();

            while (Input.Position < Input.Length)
            {
                Tuple<double, double> lastRange = new(0, 1);
                double address = BitConverter.ToDouble(Input.ReadBytes(8));

                while(true)
                {
                    bool isTrue = lastRange.IsLastTrue();
                    if (isTrue) 
                        break;
                    else
                    {
                        Dictionary<byte, Tuple<double, double>> rangeDictionary = GetRanges(lastRange);
                        lastRange = GetRange(rangeDictionary, address);
                        Output.WriteByte(rangeDictionary.GetByteKey(lastRange));
                    }
                }

                //Logging.WriteLine($"End block. OPosition: {Output.Position}");
            }
        }
    }
}
