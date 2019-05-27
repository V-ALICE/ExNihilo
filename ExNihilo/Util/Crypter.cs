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
            using (ICryptoTransform _iCrypto = new TripleDESCryptoServiceProvider().CreateEncryptor(_key, _iv))
            {
                var _encryptedData = _iCrypto.TransformFinalBlock(data, 0, data.Length);
                File.WriteAllBytes(fileName, _encryptedData);
            }
        }
        public static byte[] DecryptFile(string fileName)
        {
            using (ICryptoTransform _iCrypto = new TripleDESCryptoServiceProvider().CreateDecryptor(_key, _iv))
            {
                var _byteData = File.ReadAllBytes(fileName);
                return _iCrypto.TransformFinalBlock(_byteData, 0, _byteData.Length);
            }
        }
    }

}
