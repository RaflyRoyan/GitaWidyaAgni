using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GWAShared.Utilities
{
    public static class CryptoUtility
    {
        public static byte[] KE = new byte[] { 85, 36, 223, 82, 75, 37, 116, 155, 39, 140, 35, 194, 91, 162, 101, 234, 172, 28, 65, 229, 106, 249, 157, 158, 155, 221, 174, 185, 125, 120, 236, 61 };
        public static string EncryptString(string key, string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }
        public static string DecryptString(string key, string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
        public static string EncryptToBase64(string originalText)
        {
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(originalText);

            byte[] encryptedBytes;
            using (var aes = new AesManaged())
            {
                aes.Key = KE;
                aes.GenerateIV();

                using (var encryptor = aes.CreateEncryptor())
                {
                    using (var ms = new MemoryStream())
                    {
                        ms.Write(aes.IV, 0, aes.IV.Length);
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(plaintextBytes, 0, plaintextBytes.Length);
                        }
                        encryptedBytes = ms.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(encryptedBytes);
        }
        public static string DecryptToBase64(string et)
        {
            var encryptedBytes = Convert.FromBase64String(et);
            string decryptedText = "";
            using (var aes = new AesManaged())
            {
                aes.Key = KE;

                using (var ms = new MemoryStream(encryptedBytes))
                {
                    byte[] iv = new byte[16];
                    ms.Read(iv, 0, 16);
                    aes.IV = iv;

                    using (var decryptor = aes.CreateDecryptor())
                    {
                        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (var sr = new StreamReader(cs))
                            {
                                decryptedText = sr.ReadToEnd();
                            }
                        }
                    }
                }
                return decryptedText;
            }
        }
        public static string Encrypt(string text, string key)
        {
            var AES = new System.Security.Cryptography.RijndaelManaged();
            var Hash_AES = new System.Security.Cryptography.MD5CryptoServiceProvider();
            string encrypted = "";
            var hash = new byte[32];
            var temp = Hash_AES.ComputeHash(Encoding.ASCII.GetBytes(key));
            Array.Copy(temp, 0, hash, 0, 16);
            Array.Copy(temp, 0, hash, 15, 16);
            AES.Key = hash;
            AES.Mode = System.Security.Cryptography.CipherMode.ECB;
            var DESEncrypter = AES.CreateEncryptor();
            var Buffer = Encoding.ASCII.GetBytes(text);
            encrypted = Convert.ToBase64String(DESEncrypter.TransformFinalBlock(Buffer, 0, Buffer.Length));
            return encrypted;
        }
    }
}
