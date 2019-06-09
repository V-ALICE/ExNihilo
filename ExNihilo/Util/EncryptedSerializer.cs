using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace ExNihilo.Util
{
    public static class EncryptedSerializer
    {
        private static readonly byte[] _key = Encoding.Default.GetBytes("B9obFB76n8fg8PAq");
        private static readonly byte[] _iv  = Encoding.Default.GetBytes("6jHJf87ss3b0IJ9e");

        private static void EncryptFile(string fileName, byte[] data)
        {
            using (ICryptoTransform iCrypto = new TripleDESCryptoServiceProvider().CreateEncryptor(_key, _iv))
            {
                var encryptedData = iCrypto.TransformFinalBlock(data, 0, data.Length);
                File.WriteAllBytes(fileName, encryptedData);
            }
        }
        private static byte[] DecryptFile(string fileName)
        {
            using (ICryptoTransform iCrypto = new TripleDESCryptoServiceProvider().CreateDecryptor(_key, _iv))
            {
                var byteData = File.ReadAllBytes(fileName);
                return iCrypto.TransformFinalBlock(byteData, 0, byteData.Length);
            }
        }

        public static void SerializeOut(string fileName, object o)
        {
            if (File.Exists(fileName)) File.Delete(fileName);

            var formatter = new BinaryFormatter();
            try
            {
                var test = new MemoryStream();
                formatter.Serialize(test, o);
                EncryptFile(fileName, test.ToArray());
                test.Close();
            }
            catch (SerializationException e)
            {
                Console.WriteLine(@"Failed to serialize. Reason: " + e.Message);
            }
        }

        public static object DeserializeIn(string fileName)
        {
            if (!File.Exists(fileName)) return null;

            object o = null;
            try
            {
                var test = new MemoryStream(DecryptFile(fileName));
                o = new BinaryFormatter().Deserialize(test);
                test.Close();
            }
            catch (SerializationException e)
            {
                Console.WriteLine(@"Failed to deserialize. Reason: " + e.Message);
            }
            return o;
        }
    }

}
