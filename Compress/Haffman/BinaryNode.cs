namespace MararCore.Compress.Haffman
{
    internal struct NodeItem
    {
        public byte Symbol
        {
            get
            {
                return Symbols[0];
            }
            set
            {
                Symbols[0] = value;
            }
        }
        public byte[] Symbols;
        public ulong Frequency;
        public NodeItem(byte[] symbols, ulong frequency)
        {
            Symbols = symbols;
            Frequency = frequency;
        }
        public NodeItem(byte symbol, ulong frequency)
        {
            Symbols = [symbol];
            Frequency = frequency;
        }
    }
    internal class BinaryNode
    {
        public NodeItem Item;
        public BinaryNode? Left;
        public BinaryNode? Right;

        public BinaryNode() { }
        public BinaryNode(BinaryNode left, BinaryNode right, NodeItem item)
        {
            Left = left;
            Right = right;
            Item = item;
        }
    }
}
