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
        private readonly string GlobalCache = CacheManager.GetNewFile();
        private readonly Stream MainStream;
        private readonly Crypto GlobalCrypto;
        private byte Flags;
        public bool UseTime { set => SetFlag(7, value); get => GetFlag(7); }
        public bool LargeMode { set => SetFlag(6, value); get => GetFlag(6); }
        public bool UseCrypto { set => SetFlag(5, value); get => GetFlag(5); }
        public bool UseCryptoFS { set => SetFlag(4, value); get => GetFlag(4); }

        public MainLinker(Stream mainStream, byte[] iv, byte[] key)
        {
            MainStream = mainStream;
            GlobalCrypto = new(iv, key);
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
            List<byte> buffer = new(Encoding.UTF8.GetBytes(str)) { 0 };
            return [.. buffer];
        }
        private string ReadString()
        {
            List<byte> buffer = [(byte)MainStream.ReadByte()];

            while (buffer[^-1] > 0 || MainStream.Position < MainStream.Length)
            {
                buffer.Add((byte)MainStream.ReadByte());
            }

            return Encoding.UTF8.GetString(buffer.ToArray());
        }
        private void WritePrimaryHeader()
        {
            MainStream.Write(GetBytes(Signature));
            MainStream.WriteByte(GrandVersion);
            MainStream.WriteByte(Version);
            MainStream.WriteByte(Flags);
            MainStream.Write(GetBytes(DateTimeConverter.Encode(DateTime.Now)));
        }
        private Stream[] WriteFS(string rootDirectory)
        {
            bool useTime = UseTime;
            bool largeMode = LargeMode;

            Stream cache;
            if (UseCryptoFS) cache = new FileStream(GlobalCache, FileMode.Create);
            else cache = MainStream;

            FSHeader fsHeader = new(rootDirectory, LargeMode);
            fsHeader.InitFS();

            cache.Write(GetBytes(fsHeader.Directories.Count));
            foreach (DirectoryFrame directory in fsHeader.Directories)
            {
                cache.Write(GetBytes(directory.Address));
                cache.Write(EncodeString(directory.Name));
                if (useTime) cache.Write(GetBytes(DateTimeConverter.Encode(directory.CreationDate)));
            }

            cache.Write(GetBytes(fsHeader.Files.Count));
            for (int i = 0; i < fsHeader.Files.Count; i++)
            {
                FileFrame file = fsHeader.Files[i];
                cache.Write(GetBytes(file.Address));
                cache.Write(EncodeString(file.Name));
                if (largeMode) cache.Write(GetBytes(file.Length));
                else cache.Write(GetBytes((uint)file.Length));
                if (useTime) cache.Write(GetBytes(DateTimeConverter.Encode(file.CreationDate)));
            }

            if (UseCryptoFS)
            {
                cache.Close();
                cache = new FileStream(GlobalCache,  FileMode.Open);
                MainStream.Position += 4;
                long startPosition = MainStream.Position;
                GlobalCrypto.Encode(cache, MainStream);
                long difference = MainStream.Position - startPosition;
                MainStream.Position = startPosition - 4;
                MainStream.Write(GetBytes((int)difference));
                MainStream.Position += difference + 4;
                cache.Close();
                File.Delete(GlobalCache);
            }

            return fsHeader.FileStreams.ToArray();
        }
        private void Compress()
        {

        }

        public void LinkTo(string rootDirectory)
        {
            WritePrimaryHeader();

            Stream[] files = WriteFS(rootDirectory);

            FileStream cacheFile = new(GlobalCache, FileMode.Create);
            LotStreamReader lotReader = new(files);

            HaffmanCompressor compressor = new(lotReader, cacheFile);
            compressor.Encode();
            cacheFile.Close();
            cacheFile = new(GlobalCache, FileMode.Open);
            GlobalCrypto.Encode(cacheFile, MainStream);
            MainStream.Flush();
        }
    }
}
