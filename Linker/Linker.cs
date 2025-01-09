using MararCore.LotStreams;
using System.Text;
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
            /*
                public static string Coding(byte[] data, string codePage = "utf-8")
                {
                    if (codePage == "utf-8") return Encoding.UTF8.GetString(data);
                    if (codePage == "ascii") return Encoding.ASCII.GetString(data);
                    if (codePage == "latin1") return Encoding.Latin1.GetString(data);
                    return string.Empty;
                }
                public static byte[] Coding(string data, string codePage = "utf-8")
                {
                    if (codePage == "utf-8") return Encoding.UTF8.GetBytes(data);
                    if (codePage == "ascii") return Encoding.ASCII.GetBytes(data);
                    if (codePage == "latin1") return Encoding.Latin1.GetBytes(data);
                    return [0];
                }
                */
            byte[] buffer = Encoding.UTF8.GetBytes(str);
            return buffer;
        }
        public void Link(string rootDirectory)
        {
            bool useTime = UseTime;
            bool largeMode = LargeMode;
            Output.Write(GetBytes(Signature));
            Output.WriteByte(GrandVersion);
            Output.WriteByte(Version);
            Output.WriteByte(Flags);
            Output.Write(GetBytes(DateTimeConverter.Encode(DateTime.Now)));

            FSHeader fsHeader = new(rootDirectory, LargeMode);

            Output.Write(GetBytes(fsHeader.Directories.Count));
            foreach (DirectoryFrame directory in fsHeader.Directories)
            {
                Output.Write(GetBytes(directory.Address));
                Output.Write(EncodeString(directory.Name));
                if (useTime) Output.Write(GetBytes(DateTimeConverter.Encode(directory.CreationDate)));
            }

            Output.Write(GetBytes(fsHeader.Files.Count));
            for (int i = 0; i < fsHeader.Files.Count; i++)
            {
                FileFrame file = fsHeader.Files[i];
                Output.Write(GetBytes(file.Address));
                Output.Write(EncodeString(file.Name));
                if (largeMode) Output.Write(GetBytes(file.Length));
                else Output.Write(GetBytes((uint)file.Length));
                if (useTime) Output.Write(GetBytes(DateTimeConverter.Encode(file.CreationDate)));
            }

            LotStreamReader lotReader = new(fsHeader.FileStreams.ToArray());

        }
    }
}
