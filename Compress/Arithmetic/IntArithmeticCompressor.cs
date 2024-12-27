namespace MararCore.Compress.Arithmetic
{
    public class IntArithmeticCompressor : FileProcessor
    {
        private readonly byte CodeLength;
        private readonly ulong MaxCode = 1;
        private readonly ulong[] FrequencyDictionary = new ulong[256];
        private readonly Tuple<ulong, ulong>[] CurrentDictionary = new Tuple<ulong, ulong>[256];

        public IntArithmeticCompressor(Stream input, Stream output, byte codeLength = 32) : base(input, output)
        {
            CodeLength = codeLength;
            for (byte i = 0; i < CodeLength; i++)
            {
                MaxCode *= 2;
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
        private void UpdateRangeDictionary(Tuple<ulong, ulong> baseRange)
        {
            double step = (baseRange.Item2 - baseRange.Item1) / (double)Input.Length;
            CurrentDictionary[0] = new (0, (ulong)Math.Round(FrequencyDictionary[0] * step));
            
            for (ushort i = 1; i < 256; i++)
            {
                ulong length = (ulong)Math.Round(FrequencyDictionary[i] * step);
                ulong start = CurrentDictionary[i - 1].Item2;
                CurrentDictionary[i] = new(start, start + length);
            }
        }
        public override void Encode()
        {
            BitStream bitStream = new(Output);
            Tuple<ulong, ulong> currentRange = new(0, MaxCode);
            
            InitFrequencyDictionary();
            WriteDictionary();
            UpdateRangeDictionary(currentRange);
            Input.Position = 0;
            
            while (Input.Position < Input.Length)
            {
                if (currentRange.Item2 - currentRange.Item1 > 0)
                {
                    byte symbol = (byte)Input.ReadByte();
                    currentRange = CurrentDictionary[symbol];
                }
                else
                {
                    bitStream.Write(currentRange.Item1, CodeLength);
                    currentRange = new(0, MaxCode);
                    UpdateRangeDictionary(currentRange);
                }
            }
            
            bitStream.Write(currentRange.Item1, CodeLength);
            bitStream.FlushWrite();
        }
    }
}
