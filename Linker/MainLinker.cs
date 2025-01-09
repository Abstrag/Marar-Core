using MararCore.Compress.Haffman;
using MararCore.LotStreams;
using System.Text;
using static System.BitConverter;

namespace MararCore.Linker
{
    internal class MainLinker
    {
        private const uint Signature = 0x4DA072AF;
        private const byte GrandVersion = 0;
        private const byte Version = 0;
        private readonly Stream MainStream;
        private byte Flags;
        public bool UseTime { set => SetFlag(7, value); get => GetFlag(7); }
        public bool LargeMode { set => SetFlag(6, value); get => GetFlag(6); }

        public MainLinker(Stream mainStream)
        {
            MainStream = mainStream;
        }
        private static byte[] EncodeString(string str)
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
        private bool GetFlag(byte shift)
        {
            if ((Flags & (1 << shift)) > 0) return true;
            return false;
        }
        private void SetFlag(byte shift, bool value)
        {
            if (value) Flags |= (byte)(1 << shift);
            else Flags &= (byte)~(1 << shift);
        }
        private string ReadString()
        {
            List<byte> buffer = new();

            buffer.Add((byte)MainStream.ReadByte());
            while (buffer[^-1] > 0 || MainStream.Position < MainStream.Length)
            {
                buffer.Add((byte)MainStream.ReadByte());
            }

            return Encoding.UTF8.GetString(buffer.ToArray());
        }

        public void LinkTo(string rootDirectory, byte[] iv, byte[] key)
        {
            bool useTime = UseTime;
            bool largeMode = LargeMode;
            MainStream.Write(GetBytes(Signature));
            MainStream.WriteByte(GrandVersion);
            MainStream.WriteByte(Version);
            MainStream.WriteByte(Flags);
            MainStream.Write(GetBytes(DateTimeConverter.Encode(DateTime.Now)));

            FSHeader fsHeader = new(rootDirectory, LargeMode);
            fsHeader.InitFS();

            MainStream.Write(GetBytes(fsHeader.Directories.Count));
            foreach (DirectoryFrame directory in fsHeader.Directories)
            {
                MainStream.Write(GetBytes(directory.Address));
                MainStream.Write(EncodeString(directory.Name));
                if (useTime) MainStream.Write(GetBytes(DateTimeConverter.Encode(directory.CreationDate)));
            }

            MainStream.Write(GetBytes(fsHeader.Files.Count));
            for (int i = 0; i < fsHeader.Files.Count; i++)
            {
                FileFrame file = fsHeader.Files[i];
                MainStream.Write(GetBytes(file.Address));
                MainStream.Write(EncodeString(file.Name));
                if (largeMode) MainStream.Write(GetBytes(file.Length));
                else MainStream.Write(GetBytes((uint)file.Length));
                if (useTime) MainStream.Write(GetBytes(DateTimeConverter.Encode(file.CreationDate)));
            }

            string cachePath = CacheManager.GetNewFile();
            FileStream cacheFile = new(cachePath, FileMode.Create);
            LotStreamReader lotReader = new(fsHeader.FileStreams.ToArray());

            HaffmanCompressor compressor = new(lotReader, cacheFile);
            compressor.Encode();
            cacheFile.Close();
            cacheFile = new(cachePath, FileMode.Open);
            Crypto crypto = new(iv, key, cacheFile, new BorderedStream(MainStream, MainStream.Position));
            crypto.Encode();
            MainStream.Flush();
        }
    }
}
