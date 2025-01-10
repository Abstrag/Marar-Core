using System.Security.Cryptography;

namespace MararCore
{
    public class Crypto
    {
        public readonly Aes Alghorithm = Aes.Create();
        public byte[] GetIV() => Alghorithm.IV;     // 128 бит = 16 байт
        public byte[] GetKey() => Alghorithm.Key;   // 256 бит = 32 байта

        public Crypto()
        {
            Alghorithm.GenerateIV();
            Alghorithm.GenerateKey();
            InitCrypto();
        }

        public Crypto(byte[] iv, byte[] key)
        {
            InitCrypto();
            Alghorithm.IV = iv;
            Alghorithm.Key = key;
        }

        private void InitCrypto()
        {
            Alghorithm.Mode = CipherMode.CBC;
            Alghorithm.Padding = PaddingMode.ISO10126;
            Alghorithm.BlockSize = 128;
        }

        public void Encode(Stream input, Stream output)
        {
            ICryptoTransform encryptor = Alghorithm.CreateEncryptor();
            CryptoStream cryptoStream = new(output, encryptor, CryptoStreamMode.Write);

            input.CopyTo(cryptoStream);
            cryptoStream.FlushFinalBlock();
        }
        public void Decode(Stream input, Stream output)
        {
            ICryptoTransform decryptor = Alghorithm.CreateDecryptor();
            CryptoStream cryptoStream = new(input, decryptor, CryptoStreamMode.Read);
            cryptoStream.Flush();
            cryptoStream.CopyTo(output);
            cryptoStream.Close();
        }
    }
}
