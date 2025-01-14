using Marar.Core.Compress.Haffman;
using Marar.Core.LotStreams;
using System.Text;
using static System.BitConverter;
using static Marar.Core.Linker.DateTimeConverter;
using System.Security.Cryptography;

namespace Marar.Core.Linker
{
    public enum LinkerCondition
    {
        PrimaryHeader,
        FSHeader,
        Crypto,
        Compress
    }
    public interface ILinkerTrace
    {
        public void Trace(LinkerCondition condition);
    }
    public class MainLinker
    {
        private const uint Signature = 0xAF72A04D;
        private readonly ILinkerTrace TraceManager;
        private readonly string GlobalCache = CacheManager.GetNewFile();
        private readonly Stream MainStream;
        private Crypto GlobalCrypto;
        private byte Flags;
        public byte Version = 0;
        public FSHeader FileHeader = new();
        public DateTime CreationDateTime;
        public bool UseTime { set => SetFlag(7, value); get => GetFlag(7); }
        public bool LargeMode { set => SetFlag(6, value); get => GetFlag(6); }
        public bool UseCrypto { set => SetFlag(5, value); get => GetFlag(5); }
        public bool UseCryptoFS { set => SetFlag(4, value); get => GetFlag(4); }

        public MainLinker(Stream mainStream, ILinkerTrace traceManager)
        {
            TraceManager = traceManager;
            MainStream = mainStream;
        }

        public void SetCrypto(byte[] key)
        {
            GlobalCrypto = new(MD5.HashData(key), key);
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
            return Encoding.UTF8.GetBytes(str + '\0');
        }
        private void WritePrimaryHeader()
        {
            MainStream.Write(GetBytes(Signature));
            MainStream.WriteByte(Version);
            MainStream.WriteByte(Flags);
            MainStream.Write(GetBytes(EncodeDateTime(CreationDateTime)));
        }
        private void WriteFS()
        {
            bool useTime = UseTime;
            bool largeMode = LargeMode;

            Stream cache;
            if (UseCryptoFS) cache = new FileStream(GlobalCache, FileMode.Create);
            else cache = MainStream;

            cache.Write(GetBytes(FileHeader.Directories.Count));
            for (int i = 0; i < FileHeader.Directories.Count; i++)
            {
                cache.Write(GetBytes(FileHeader.Directories[i].Address));
                if (useTime) cache.Write(GetBytes(EncodeDateTime(FileHeader.Directories[i].CreationDate)));
                cache.Write(EncodeString(FileHeader.Directories[i].Name));
            }

            cache.Write(GetBytes(FileHeader.Files.Count));
            for (int i = 0; i < FileHeader.Files.Count; i++)
            {
                FileFrame file = FileHeader.Files[i];
                cache.Write(GetBytes(file.Address));
                if (useTime) cache.Write(GetBytes(EncodeDateTime(file.CreationDate)));
                if (largeMode) cache.Write(GetBytes(file.Length));
                else cache.Write(GetBytes((uint)file.Length));
                cache.Write(EncodeString(file.Name));
            }

            if (UseCryptoFS)
            {
                cache.Close();
                cache = new FileStream(GlobalCache,  FileMode.Open);
                MainStream.Write(GetBytes(cache.Length));
                GlobalCrypto.Encode(cache, MainStream);
                cache.Close();
                File.Delete(GlobalCache);
            }
        }
        private void Compress()
        {
            Stream[] files = new Stream[FileHeader.Files.Count];
            for (int i = 0; i < files.Length; i++)
                files[i] = new FileStream(FileHeader.Files[i].ExternalPath, FileMode.Open);

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
            CreationDateTime = DateTime.Now;

            FileHeader.InitFS(rootDirectory, LargeMode);

            TraceManager.Trace(LinkerCondition.PrimaryHeader);
            WritePrimaryHeader();
            TraceManager.Trace(LinkerCondition.FSHeader);
            WriteFS();
            TraceManager.Trace(LinkerCondition.Compress);
            Compress();

            if (UseCrypto)
            {
                TraceManager.Trace(LinkerCondition.Crypto);
                Encrypt();
            }

            MainStream.Flush();
        }

        private static long ImproveNumber(long number)
        {
            long remains = number % 16;
            if (remains > 0) number -= remains - 16;
            return number;
        }
        private static string ReadString(Stream source)
        {
            List<byte> buffer = [(byte)source.ReadByte()];

            while (buffer[^1] > 0 && source.Position < source.Length)
            {
                buffer.Add((byte)source.ReadByte());
            }
            buffer.RemoveAt(buffer.Count - 1);
            return Encoding.UTF8.GetString(buffer.ToArray());
        }
        public void ReadPrimaryHeader(bool ignoreSignature = false)
        {
            MainStream.Position = 0;
            if (!ignoreSignature)
            {
                if (ToUInt32(MainStream.ReadBytes(4)) != Signature)
                    throw new Exception("Wrong signature");
            }
            else MainStream.Position += 4;

            Version = (byte)MainStream.ReadByte();
            Flags = (byte)MainStream.ReadByte();
            CreationDateTime = DecodeDateTime(ToInt32(MainStream.ReadBytes(4)));
        }
        public void ReadFS()
        {
            MainStream.Position = 10;
            Stream cacheFile;

            if (UseCryptoFS)
            {
                long length = ToInt64(MainStream.ReadBytes(8));
                cacheFile = new FileStream(GlobalCache, FileMode.Create);
                GlobalCrypto.Decode(MainStream, cacheFile, length);
                cacheFile.Close();
                cacheFile = new FileStream(GlobalCache, FileMode.Open);
                MainStream.Position = 18 + ImproveNumber(length);
            }
            else cacheFile = MainStream;

            for (uint directoryCount = ToUInt32(cacheFile.ReadBytes(4)); directoryCount > 0; directoryCount--)
            {
                uint address = ToUInt32(cacheFile.ReadBytes(4));

                DateTime dateTime;
                if (UseTime) dateTime = DecodeDateTime(ToInt32(cacheFile.ReadBytes(4)));
                else dateTime = new();

                string name = ReadString(cacheFile);

                FileHeader.Directories.Add(new(address, dateTime, name));
            }
            for (uint filesCount = ToUInt32(cacheFile.ReadBytes(4)); filesCount > 0; filesCount--)
            {
                uint address = ToUInt32(cacheFile.ReadBytes(4));

                DateTime dateTime;
                if (UseTime) dateTime = DecodeDateTime(ToInt32(cacheFile.ReadBytes(4)));
                else dateTime = new();

                long length;
                if (LargeMode) length = ToInt64(cacheFile.ReadBytes(8));
                else length = ToUInt32(cacheFile.ReadBytes(4));

                string name = ReadString(cacheFile);

                FileHeader.Files.Add(new(address, dateTime, length, name));
            }

            if (UseCryptoFS)
            {
                cacheFile.Close();
                File.Delete(GlobalCache);
            }
        }
        private void CreateFS()
        {
            for (int i = 0; i < FileHeader.Directories.Count; i++)
            {
                Directory.CreateDirectory(FileHeader.Directories[i].ExternalPath);
            }
        }
        private void Decrypt()
        {
            FileStream cacheFile = new(GlobalCache, FileMode.Create);
            //GlobalCrypto.Decode(MainStream, cacheFile, MainStream.Length - MainStream.Position);
            GlobalCrypto.Decode(MainStream, cacheFile);
            cacheFile.Close();
        }
        private void Decompress()
        {
            bool useTime = UseTime;
            long[] lengths = new long[FileHeader.Files.Count];
            Stream[] files = new Stream[FileHeader.Files.Count];
            for (int i = 0; i < files.Length; i++)
            {
                lengths[i] = FileHeader.Files[i].Length;
                files[i] = new FileStream(FileHeader.Files[i].ExternalPath, FileMode.Create);
                if (useTime) File.SetCreationTime(FileHeader.Files[i].ExternalPath, FileHeader.Files[i].CreationDate);
            }
            LotStreamWriter lotWriter = new(files, lengths);
            Stream cacheFile;
            if (UseCrypto) cacheFile = new FileStream(GlobalCache, FileMode.Open);
            else cacheFile = MainStream;

            HaffmanCompressor decompressor = new(cacheFile, lotWriter);
            decompressor.Decode();

            if (UseCrypto) cacheFile.Close();

            for (int i = 0; i < files.Length; i++)
                files[i].Close();
        }

        public void LinkFrom(string rootDirectory, bool ignoreSignature = false)
        {
            ReadPrimaryHeader(ignoreSignature);

            ReadFS();
            FileHeader.BindFS(rootDirectory);
            CreateFS();

            if (UseCrypto) Decrypt();
            Decompress();
        }
    }
}
