namespace MararCore.Compress.Haffman
{
    internal class BinaryNode
    {
        public object? Item;
        public BinaryNode? Left;
        public BinaryNode? Right;

        public BinaryNode() { }
        public BinaryNode(BinaryNode left, BinaryNode right, object item)
        {
            Left = left;
            Right = right;
            Item = item;
        }
    }
}
