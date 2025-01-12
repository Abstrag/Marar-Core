namespace Marar.Core.Compress.Haffman
{
    internal struct NodeItem
    {
        public byte Symbol;
        public ulong Frequency;
        public NodeItem(byte symbol, ulong frequency)
        {
            Symbol = symbol;
            Frequency = frequency;
        }
        public NodeItem(ulong frequency)
        {
            Frequency = frequency;
        }
    }
    internal class BinaryNode
    {
        public NodeItem Item;
        public BinaryNode? Left;
        public BinaryNode? Right;
        public bool IsLeaf => Left == null && Right == null;

        public BinaryNode() { }
        public BinaryNode(BinaryNode left, BinaryNode right, NodeItem item)
        {
            Left = left;
            Right = right;
            Item = item;
        }
        public BinaryNode(NodeItem item)
        {
            Item = item;
        }

        public BinaryNode? GetNode(byte side)
        {
            if(side > 0) return Right;
            return Left;
        }
    }
}
