using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ExNihilo.Util
{
    public static class Crypter
    {
        private static readonly byte[] _key = Encoding.Default.GetBytes("AliceDungeonGame");
        private static readonly byte[] _iv = Encoding.Default.GetBytes("CheatingNotAllow");

        public static void EncryptFile(string fileName, byte[] data)
        {
            using (ICryptoTransform iCrypto = new TripleDESCryptoServiceProvider().CreateEncryptor(_key, _iv))
            {
                var encryptedData = iCrypto.TransformFinalBlock(data, 0, data.Length);
                File.WriteAllBytes(fileName, encryptedData);
            }
        }
        public static byte[] DecryptFile(string fileName)
        {
            using (ICryptoTransform iCrypto = new TripleDESCryptoServiceProvider().CreateDecryptor(_key, _iv))
            {
                var byteData = File.ReadAllBytes(fileName);
                return iCrypto.TransformFinalBlock(byteData, 0, byteData.Length);
            }
        }
    }

}
