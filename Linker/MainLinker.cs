using MararCore.Compress.Haffman;
using MararCore.LotStreams;
using System.Text;
using static System.BitConverter;
using static MararCore.Linker.DateTimeConverter;

namespace MararCore.Linker
{
    internal class MainLinker
    {
        private const uint Signature = 0xAF72A04D;
        private readonly string GlobalCache = CacheManager.GetNewFile();
        private readonly Stream MainStream;
        private readonly Crypto GlobalCrypto;
        private byte GrandVersion = 0;
        private byte Version = 0;
        private byte Flags;
        public DateTime CreationDateTime;
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
            return Encoding.UTF8.GetBytes(str + '\0');
        }
        private void WritePrimaryHeader()
        {
            MainStream.Write(GetBytes(Signature));
            MainStream.WriteByte(GrandVersion);
            MainStream.WriteByte(Version);
            MainStream.WriteByte(Flags);
            MainStream.Write(GetBytes(EncodeDateTime(CreationDateTime)));
        }
        private void WriteFS(FSHeader fsHeader)
        {
            bool useTime = UseTime;
            bool largeMode = LargeMode;

            Stream cache;
            if (UseCryptoFS) cache = new FileStream(GlobalCache, FileMode.Create);
            else cache = MainStream;

            cache.Write(GetBytes(fsHeader.Directories.Count));
            foreach (DirectoryFrame directory in fsHeader.Directories)
            {
                cache.Write(GetBytes(directory.Address));
                cache.Write(EncodeString(directory.Name));
                if (useTime) cache.Write(GetBytes(DateTimeConverter.EncodeDateTime(directory.CreationDate)));
            }

            cache.Write(GetBytes(fsHeader.Files.Count));
            for (int i = 0; i < fsHeader.Files.Count; i++)
            {
                FileFrame file = fsHeader.Files[i];
                cache.Write(GetBytes(file.Address));
                if (useTime) cache.Write(GetBytes(DateTimeConverter.EncodeDateTime(file.CreationDate)));
                if (largeMode) cache.Write(GetBytes(file.Length));
                else cache.Write(GetBytes((uint)file.Length));
                cache.Write(EncodeString(file.Name));
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
                MainStream.Position += difference;
                cache.Close();
                File.Delete(GlobalCache);
            }
        }
        private void Compress(FSHeader fsHeader)
        {
            Stream[] files = new Stream[fsHeader.Files.Count];
            for (int i = 0; i < files.Length; i++)
                files[i] = new FileStream(fsHeader.Files[i].ExternalPath, FileMode.Open);

            Stream cacheFile;
            if (UseCrypto) cacheFile = new FileStream(GlobalCache, FileMode.Create);
            else cacheFile = MainStream;

            LotStreamReader lotReader = new(files);

            HaffmanCompressor compressor = new(lotReader, cacheFile);
            compressor.Encode();

            if (UseCrypto) cacheFile.Close();

            for (int i = 0; i < files.Length; i++) 
                files[i].Close();
        }
        private void Encrypt()
        {
            FileStream cacheFile = new(GlobalCache, FileMode.Open);
            GlobalCrypto.Encode(cacheFile, MainStream);
            cacheFile.Close();
        }

        public void LinkTo(string rootDirectory)
        {
            FSHeader fsHeader = new();
            fsHeader.InitFS(rootDirectory, LargeMode);

            WritePrimaryHeader();
            WriteFS(fsHeader);
            //MainStream.Flush();
            Compress(fsHeader);

            if (UseCrypto) Encrypt();

            MainStream.Flush();
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
        public void ReadPrimaryHeader(bool ignoreSignature)
        {
            if (!ignoreSignature)
            {
                if (ToUInt32(MainStream.ReadBytes(4)) != Signature) 
                    throw new Exception("Wrong signature");
            }
            else MainStream.Position += 4;

            GrandVersion = (byte)MainStream.ReadByte();
            Version = (byte)MainStream.ReadByte();
            Flags = (byte)MainStream.ReadByte();
            CreationDateTime = DecodeDateTime(ToInt32(MainStream.ReadBytes(4)));
        }
        public FSHeader ReadFS()
        {
            FSHeader fsHeader = new();

            for (uint directoryCount = ToUInt32(MainStream.ReadBytes(4)); directoryCount > 0; directoryCount--)
            {
                uint address = ToUInt32(MainStream.ReadBytes(4));

                DateTime dateTime;
                if (UseTime) dateTime = DecodeDateTime(ToInt32(MainStream.ReadBytes(4)));
                else dateTime = new();

                string name = ReadString();

                fsHeader.Directories.Add(new(address, dateTime, name));
            }
            for (uint filesCount = ToUInt32(MainStream.ReadBytes(4)); filesCount > 0; filesCount--)
            {
                uint address = ToUInt32(MainStream.ReadBytes(4));

                DateTime dateTime;
                if (UseTime) dateTime = DecodeDateTime(ToInt32(MainStream.ReadBytes(4)));
                else dateTime = new();

                long length;
                if (LargeMode) length = ToInt64(MainStream.ReadBytes(8));
                else length = ToUInt32(MainStream.ReadBytes(4));

                string name = ReadString();

                fsHeader.Files.Add(new(address, dateTime, length, name));
            }

            return fsHeader;
        }

        public void LinkFrom(string rootDirectory, bool ignoreSignature = false)
        {
            ReadPrimaryHeader(ignoreSignature);
            FSHeader fsHeader = ReadFS();

            fsHeader.BindFS(rootDirectory);
        }
    }
}
