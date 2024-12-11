namespace MararCore0.LZA
{
    /*internal struct Order
    {
        public byte[] Data;
    }*/

    public class DLZ : FileProcessor
    {
        private Dictionary<byte[], ushort> Dictionary = new();
        public DLZ(Stream input, Stream output) : base(input, output) { }

        private DateTime Time;
        private void Start()
        {
            Time = DateTime.Now;
        }
        private void End()
        {
            Console.WriteLine(Time.Second - DateTime.Now.Second);
        }
        private ushort AddOrder(byte[] data)
        {
            //if (data.Length > 2) Console.WriteLine(data.Length);
            if (Dictionary.Count >= 3840)
            {
                Dictionary.Clear();
                //Console.WriteLine("Clean dictionary");
            }
            Dictionary.Add(data, (ushort)(Dictionary.Count + 256));
            return (ushort)(Dictionary.Count + 255);
        }
        private ushort GetIndex(byte[] data)
        {
            if (data.Length == 1) return data[0];
            return Dictionary[data];
        }
        public override void Encode()
        {
            Start();
            BitStream stream = new(Output);
            List<byte> order = new();
            List<byte> commonOrder = new();
            ushort prevIndex = 0;
            ushort currentIndex = 0;

            while (Input.Position < Input.Length)
            {
                order.Add((byte)Input.ReadByte());
                if (order.Count == 1) currentIndex = order[0];
                else if (Dictionary.ContainsKey(order.ToArray()))
                {
                    currentIndex = GetIndex(order.ToArray());
                    continue;
                }
                else 
                {
                    order.RemoveAt(order.Count - 1);
                    Input.Position--;

                    if (commonOrder.Count == 0)
                    {
                        commonOrder.AddRange(order);
                        prevIndex = currentIndex;
                    }
                    else
                    {
                        commonOrder.AddRange(order);
                        AddOrder(commonOrder.ToArray());
                        commonOrder.Clear();
                        stream.Write(BitConverter.GetBytes(prevIndex << 12 | currentIndex), 24);
                    }

                    order.Clear();
                }
            }
            if (commonOrder.Count != 0)
            {
                stream.Write(BitConverter.GetBytes(prevIndex), 12);
                if (order.Count != 0) stream.Write(BitConverter.GetBytes(currentIndex), 12);
            }
            stream.FlushWrite();

            End();
        }
    }
}
