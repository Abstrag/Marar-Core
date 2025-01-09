using static System.BitConverter;

namespace MararCore.Linker
{
    internal class LinkTo
    {
        private const uint Signature = 0x4DA072AF;
        private const byte GrandVersion = 0;
        private const byte Version = 0;
        private Stream Output;
        private byte Flags;
        public bool UseTime { set => SetFlag(7, value); get => GetFlag(7); }
        public bool LargeMode { set => SetFlag(6, value); get => GetFlag(6); }

        public LinkTo(Stream output)
        {
            Output = output;
        }
        private bool GetFlag(byte shift)
        {
            if ((Flags & (1 << shift)) > 0) return true;
            return false;
        }
        private void SetFlag(byte shift, bool value)
        {
            if (value) Flags |= (byte)(1 << shift);
            else Flags &= (byte)(1 << shift);
        }
        private byte[] EncodeString(string str)
        {
            byte[] buffer = 
        }
        public void Link(string rootDirectory)
        {
            Output.Write(GetBytes(Signature));
            Output.WriteByte(GrandVersion);
            Output.WriteByte(Version);
            Output.WriteByte(Flags);
            Output.Write(GetBytes(DateTimeConverter.Encode(DateTime.Now)));
            FSHeader fsHeader = new(rootDirectory, LargeMode);
            Output.Write(GetBytes(fsHeader.Directories.Count));
            foreach (DirectoryFrame directory in fsHeader.Directories)
            {
                Output.Write(GetBytes());
            }
        }
    }
}
