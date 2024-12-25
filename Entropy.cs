namespace MararCore
{
    public static class Entropy
    {
        public static double Compute(Stream input)
        {
            Dictionary<byte, double> frequnces = new();

            long start = input.Position;
            while (input.Position < input.Length)
            {
                byte symbol = (byte)input.ReadByte();
                if (!frequnces.TryAdd(symbol, 1))
                    frequnces[symbol]++;
            }
            input.Position = start;

            foreach (byte symbol in frequnces.Keys)
            {
                frequnces[symbol] = frequnces[symbol] / input.Length;
            }

            double result = 0;

            foreach (byte symbol in frequnces.Keys)
            {
                result -= frequnces[symbol] * Math.Log(frequnces[symbol], frequnces.Count);
            }

            return result;
        }
    }
}
