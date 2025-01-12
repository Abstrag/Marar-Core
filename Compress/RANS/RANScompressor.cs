namespace Marar.Core.Compress.RANS
{
    internal class RANScompressor(Stream input, Stream output) : FileProcessor(input, output)
    {
        private const uint ConstL = 1 << 16;
        private const uint ConstK = 10;
        private readonly uint ConstM = 1 << (int)ConstK;
        private readonly ulong[] Lengths = new ulong[257];
        private readonly ulong[] FrequencyDictionary = new ulong[256];
        private readonly ulong[] FrequencyLows = new ulong[257];
        private ulong SourceLength = 0;

        private void InitFrequencyDictionary()
        {
            while (Input.Position < Input.Length)
            {
                FrequencyDictionary[Input.ReadByte()]++;
            }
            Input.Position = 0;
        }
        private void ImproveFrequency()
        {
            double ratio = (double)ConstM / SourceLength;
            for (short i = 0; i < 256; i++)
            {
                FrequencyDictionary[i] = (ulong)Math.Round(FrequencyDictionary[i] * ratio);
            }
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
        public override void Encode()
        {
            SourceLength = (ulong)Input.Length;
            InitFrequencyDictionary();
            WriteDictionary();
            ImproveFrequency();
            InitFrequencyLows();

            uint tempCode = 0;
            long symbolEncode(byte symbol, uint code)
            {
                if ((FrequencyDictionary[symbol] <= 0 && FrequencyDictionary[symbol] > ConstM) || code < ConstL)
                {
                    return -1;
                }
                return 0;
            }
        }
    }
}
