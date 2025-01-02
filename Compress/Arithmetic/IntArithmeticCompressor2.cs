namespace MararCore.Compress.Arithmetic
{
    public class IntArithmeticCompressor2 : FileProcessor
    {
        private readonly byte CodeLength;
        private readonly ulong MaxCode = 1;
        private readonly ulong[] FrequencyLows = new ulong[257];
        private readonly ulong[] FrequencyDictionary = new ulong[256];
        private readonly double[] ProbabilityLow = new double[257];
        private readonly double[] ProbabilityFrequency = new double[256];
        private ulong SourceLength = 0;

        public IntArithmeticCompressor2(Stream input, Stream output, byte codeLength = 32) : base(input, output)
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
        private void InitFrequencyLows()
        {
            ulong lastLength = 0;

            for (ushort i = 0; i < 256; i++)
            {
                FrequencyLows[i] = lastLength;
                lastLength += FrequencyDictionary[i];
            }
            FrequencyLows[256] = lastLength;
        }

        public override void Encode()
        {
            InitFrequencyDictionary();
            InitFrequencyLows();

        }

        public void Decode()
        {
            
        }
    }
}
