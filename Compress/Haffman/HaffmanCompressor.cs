
namespace MararCore.Compress.Haffman
{
    public class HaffmanCompressor(Stream input, Stream output) : FileProcessor(input, output)
    {
        private readonly byte[] SortedSymbols = new byte[256];
        private readonly ulong[] FrequencyDictionary = new ulong[256];
        private void InitFrequency()
        {
            while (Input.Position < Input.Length)
            {
                FrequencyDictionary[Input.ReadByte()]++;
            }
            Input.Position = 0;
        }
        private void InitSorted()
        {
            List<ulong> localFrequencies = new(FrequencyDictionary);

        }
        public override void Encode()
        {
            InitFrequency();
        }
    }
}
