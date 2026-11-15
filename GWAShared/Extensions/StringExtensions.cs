using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWAShared.Extensions
{
    public static class StringExtensions
    {
        const string strKey = "b14ca5898a4e4133bbce2ea2315a1916";
        public static string EncryptString<T>(this string plainText)
        {
            string classname = typeof(T).Name;
            string key = string.Concat(typeof(T).Name, DateTime.Now.ToString("ddMMyyyy"), strKey.Substring(0, strKey.Length - 8 - classname.Length));
            return Utilities.CryptoUtility.EncryptString(key, plainText);
        }
        public static string DecryptString<T>(this string cipherText)
        {
            string classname = typeof(T).Name;
            string key = string.Concat(typeof(T).Name, DateTime.Now.ToString("ddMMyyyy"), strKey.Substring(0, strKey.Length - 8 - classname.Length));
            return Utilities.CryptoUtility.DecryptString(key, cipherText);
        }
        public static string GetRandomKey()
        {
            string validchars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            Random rnd = new Random();
            string ret = "";
            ret = ret + validchars.Substring(rnd.Next(0, validchars.Length), 1);
            ret = ret + validchars.Substring(rnd.Next(0, validchars.Length), 1);
            ret = ret + validchars.Substring(rnd.Next(0, validchars.Length), 1);
            ret = ret + validchars.Substring(rnd.Next(0, validchars.Length), 1);
            ret = ret + validchars.Substring(rnd.Next(0, validchars.Length), 1);
            ret = ret + validchars.Substring(rnd.Next(0, validchars.Length), 1);
            return ret;
        }
        public static string GeneratedPassword(string user, string pass)
        {
            System.Security.Cryptography.TripleDESCryptoServiceProvider des = new System.Security.Cryptography.TripleDESCryptoServiceProvider();
            System.Security.Cryptography.MD5CryptoServiceProvider MD5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            des.Key = MD5.ComputeHash(System.Text.ASCIIEncoding.ASCII.GetBytes(user.ToUpper().Trim()));
            des.Mode = System.Security.Cryptography.CipherMode.ECB;
            byte[] Buffer = System.Text.ASCIIEncoding.ASCII.GetBytes(pass);
            return Convert.ToBase64String(des.CreateEncryptor().TransformFinalBlock(Buffer, 0, Buffer.Length));
        }
    }
}
