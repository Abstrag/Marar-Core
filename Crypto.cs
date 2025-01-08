using System.Security.Cryptography;

namespace MararCore
{
    public class Crypto : FileProcessor
    {
        public readonly Aes Alghorithm = Aes.Create();
        public byte[] GetIV() => Alghorithm.IV;
        public byte[] GetKey() => Alghorithm.Key;

        public Crypto(Stream input, Stream output) : base(input, output)
        {
            Alghorithm.GenerateIV();
            Alghorithm.GenerateKey();
            CreateCrypto();
        }

        public Crypto(byte[] iv, byte[] key, Stream input, Stream output) : base(input, output)
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

        public override void Encode()
        {
            ICryptoTransform encryptor = Alghorithm.CreateEncryptor();
            CryptoStream cryptoStream = new(Output, encryptor, CryptoStreamMode.Write);

            Input.CopyTo(cryptoStream);
            cryptoStream.FlushFinalBlock();
        }
        public void Decode()
        {
            try
            {
                ICryptoTransform decryptor = Alghorithm.CreateDecryptor();
                CryptoStream cryptoStream = new(Input, decryptor, CryptoStreamMode.Read);
                cryptoStream.Flush();
                cryptoStream.CopyTo(Output);
                cryptoStream.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
