namespace MararCore0.LZA
{
    public class LZ78 : FileProcessor
    {
        private DateTime Time;
        private void Start()
        {
            Time = DateTime.Now;
        }
        private void End()
        {
            Console.WriteLine(Time.Second - DateTime.Now.Second);
        }
        private const byte CodeLength = 9;
        private Dictionary<byte[], ushort> OrderDictionary = new();
        private uint DictionaryLength = (uint)MathF.Pow(2, CodeLength);
        private ulong OverflowCounter = 0;

        public LZ78(Stream input, Stream output) : base(input, output) { }

        private ushort AddOrder(byte[] data)
        {
            if (OrderDictionary.Count >= DictionaryLength)
            {
                OrderDictionary.Clear();
                //Console.WriteLine($"{OverflowCounter}) {Input.Position}:{Output.Position} {Output.Position / (double)Input.Position}");
                OverflowCounter++;
            }
                
            OrderDictionary.Add(data, (ushort)(OrderDictionary.Count + 1));
            return (ushort)OrderDictionary.Count;
        }
        public override void Encode()
        {
            Start();
            BitStream bitStream = new(Output);
            List<byte> order = new();
            ushort tempCode = 0;

            while (Input.Position < Input.Length)
            {
                order.Add((byte)Input.ReadByte());
                if (OrderDictionary.ContainsKey(order.ToArray()))
                {
                    tempCode = OrderDictionary[order.ToArray()];
                    continue;
                }
                bitStream.Write(BitConverter.GetBytes(tempCode), CodeLength - 8);
                bitStream.WriteByte(order[^1], 8);
                AddOrder(order.ToArray());
                order.Clear();
                tempCode = 0;
            }

            bitStream.FlushWrite();
            End();
        }
    }
}
