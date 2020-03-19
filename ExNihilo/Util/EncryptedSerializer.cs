using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace ExNihilo.Util
{
    public static class EncryptedSerializer
    {
        private static readonly byte[] Key = Encoding.Default.GetBytes("B9obFB76n8fg8PAq");
        private static readonly byte[] Iv  = Encoding.Default.GetBytes("6jHJf87ss3b0IJ9e");

        private static void EncryptFile(string fileName, byte[] data)
        {
            using (var iCrypto = new TripleDESCryptoServiceProvider().CreateEncryptor(Key, Iv))
            {
                var encryptedData = iCrypto.TransformFinalBlock(data, 0, data.Length);
                File.WriteAllBytes(fileName, encryptedData);
            }
        }
        private static byte[] DecryptFile(string fileName)
        {
            using (var iCrypto = new TripleDESCryptoServiceProvider().CreateDecryptor(Key, Iv))
            {
                var byteData = File.ReadAllBytes(fileName);
                return iCrypto.TransformFinalBlock(byteData, 0, byteData.Length);
            }
        }

        public static void SerializeOut(string fileName, object o)
        {
            if (File.Exists(fileName)) File.Delete(fileName);

            var formatter = new BinaryFormatter();
            var test = new MemoryStream();
            formatter.Serialize(test, o);
            EncryptFile(fileName, test.ToArray());
            test.Close();
        }

        public static object DeserializeIn(string fileName)
        {
            if (!File.Exists(fileName)) return null;

            var test = new MemoryStream(DecryptFile(fileName));
            var o = new BinaryFormatter().Deserialize(test);
            test.Close();
            return o;
        }
    }

}
