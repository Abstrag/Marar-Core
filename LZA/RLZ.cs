namespace MararCore0.LZA
{
    /*internal struct Order
    {
        public byte[] Data;
    }*/

    public class RLZ : FileProcessor
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
        private List<byte[]> Dictionary = new();
        public RLZ(Stream input, Stream output) : base(input, output) { }

        private bool IsEquals(byte[] data1, byte[] data2)
        {
            if(data1.Length != data2.Length) return false;
            for (uint i = 0; i < data1.Length; i++)
            {
                if(data1[i] != data2[i]) return false;
            }
            return true;
        }
        private int Consist(byte[] data)
        {
            if(data.Length == 1) return data[0];
            for (int i = 0; i < Dictionary.Count; i++)
            {
                if (IsEquals(data, Dictionary.ElementAt(i))) return (ushort)i;
            }
            return -1;
        }
        private ushort AddOrder(byte[] data)
        {
            if (Dictionary.Count >= 3840)
            {
                Dictionary.Clear();
                //Console.WriteLine("Clean dictionary");
            }
            Dictionary.Add(data);
            return (ushort)(Dictionary.Count + 255);
        }
        private void Write(uint indexes)
        {
            byte[] raw = BitConverter.GetBytes(indexes);
            Output.Write(raw, 0, 3);
        }
        public override void Encode()
        {
            Start();
            uint lastIndexes = 0;
            bool isFirst = true;
            byte[] lastOrder = [];
            List<byte> exodus = [];
            List<byte> currentOrder = [];

            while (Input.Position < Input.Length)
            {
                currentOrder.Add((byte)Input.ReadByte());
                int index = Consist(currentOrder.ToArray());
                if (index > -1)
                {
                    if (isFirst) lastIndexes |= (uint)index << 12;
                    else lastIndexes |= (uint)index;
                    lastOrder = currentOrder.ToArray();
                    continue;
                }
                Input.Position--;
                currentOrder.Clear();
                exodus.AddRange(lastOrder);
                if (exodus.Count > lastOrder.Length)
                {
                    //Console.WriteLine($"{Input.Position}:{Output.Position}\t{Output.Position / (double)Input.Position}\t{Dictionary.Count}");
                    AddOrder(exodus.ToArray());
                    exodus.Clear();
                    lastOrder = [];
                    isFirst = true;
                    Write(lastIndexes);
                    lastIndexes = 0;
                }
                else isFirst = false;
            }
            Output.Write(BitConverter.GetBytes((ushort)(lastIndexes >> 8)));
            End();
        }
    }
}
