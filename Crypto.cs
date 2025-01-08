using System.Security.Cryptography;

namespace MararCore
{
    public class Crypto
    {
        public readonly Aes Alghorithm = Aes.Create();
        public byte[] GetIV() => Alghorithm.IV;
        public byte[] GetKey() => Alghorithm.Key;

        public Crypto()
        {
            Alghorithm.GenerateIV();
            Alghorithm.GenerateKey();
            CreateCrypto();
        }

        public Crypto(byte[] iv, byte[] key)
        {
            CreateCrypto();
            Alghorithm.IV = iv;
            Alghorithm.Key = key;
        }

        private void CreateCrypto()
        {
            Alghorithm.Mode = CipherMode.CBC;
            Alghorithm.Padding = PaddingMode.ISO10126;
            Alghorithm.BlockSize = 128;
        }

        public ulong EncryptStream(Stream input, Stream output)
        {
            ulong positionBefore = (ulong)output.Position;

            ICryptoTransform encryptor = Alghorithm.CreateEncryptor();
            CryptoStream cryptoStream = new(output, encryptor, CryptoStreamMode.Write);

            input.CopyTo(cryptoStream);
            cryptoStream.FlushFinalBlock();

            return (ulong)output.Position - positionBefore;
        }

        public void DecryptStream(Stream input, Stream output)
        {
            try
            {
                ICryptoTransform decryptor = Alghorithm.CreateDecryptor();
                CryptoStream cryptoStream = new(input, decryptor, CryptoStreamMode.Read);
                cryptoStream.Flush();
                cryptoStream.CopyTo(output);
                cryptoStream.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
