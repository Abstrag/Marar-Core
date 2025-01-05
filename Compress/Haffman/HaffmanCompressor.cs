
namespace MararCore.Compress.Haffman
{
    public class HaffmanCompressor(Stream input, Stream output) : FileProcessor(input, output)
    {
        private readonly ulong[] SymbolDictionary = new byte[256];
        private readonly ulong[] FrequencyDictionary = new ulong[256];

        private void InitFrequency()
        {
            while (Input.Position < Input.Length)
            {
                FrequencyDictionary[Input.ReadByte()]++;
            }
            Input.Position = 0;
        }
        private void InitDicitonary()
        {
            Dictionary<byte, ulong> frequency = new(256);
            for (short i = 0; i < 256; i++)
            {
                frequency[(byte)i] = FrequencyDictionary[i];
            }
            while (frequency.Count > 0)
            {

            }
        }
        public override void Encode()
        {
            InitFrequency();
        }
    }
}
