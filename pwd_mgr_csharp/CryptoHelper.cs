using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace pwd_mgr_csharp
{
    public static class CryptoHelper
    {
        public static string GeneratePassword(int length)
        {
            string pass = BaseConverter.EncodeBytes(GenerateRandomBytes(length));
            return pass.Substring(0, length);
        }
        public static byte[] GenerateRandomBytes(int length)
        {
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] bytes = new byte[length];
                rng.GetBytes(bytes, 0, length);
                return bytes;
            }
        }
        public static byte[] GetHash(string inputString)
        {
            using (HashAlgorithm algorithm = SHA256.Create())
            {
                return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
            }
        }
        public static byte[] Encrypt(Aes aes, byte[] plain, byte[] iv, string key)
        {
            byte[] salt = GenerateRandomBytes(8);

            aes.Key = CreateKey(key, salt);
            aes.GenerateIV();
            byte[] encrypted = aes.EncryptCbc(plain, aes.IV, PaddingMode.PKCS7); //encrypt password from plaintext
            byte[] first = Combine(aes.IV, encrypted); // combine
            byte[] second = Combine(salt, first);
            return second;
        }

        public static byte[] Decrypt(Aes aes, byte[] bytes, string key)
        {
            byte[] salt = bytes.Take(8).ToArray(); //get salt with first 8 bytes

            aes.Key = CreateKey(key, salt); //create a key


            byte[] iv = bytes.Skip(8).Take(16).ToArray(); //16 bytes AFTER salt

            byte[] encrypted = bytes.Skip(24).Take(bytes.Length - 24).ToArray(); //the rest of the ciphertext

            byte[] decryptedBytes = aes.DecryptCbc(encrypted, iv, PaddingMode.PKCS7); //decrypt using PKCS7 padding

            return decryptedBytes;

        }

        // private methods:

        // source:
        // https://www.techiedelight.com/concatenate-byte-arrays-csharp/
        private static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] result = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, result, 0, first.Length); //copy  the first array into result array
            Buffer.BlockCopy(second, 0, result, first.Length, second.Length); //copy second array into result array, but starting at first.Length
            return result;

        }
        private static byte[] CreateKey(string pwd, byte[] salt)
        {
            const int Iterations = 300;
            var keyGenerator = new Rfc2898DeriveBytes(pwd, salt, Iterations);
            return keyGenerator.GetBytes(32);

        }
    }
}
