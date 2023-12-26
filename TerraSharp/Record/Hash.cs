using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TerraSharp.Record
{
    public class Hash
    {
        private static string ConvertToHex(byte[] messageDigest)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in messageDigest)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
        }

        public static string GenerateMD5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                return ConvertToHex(md5.ComputeHash(Encoding.UTF8.GetBytes(input)));
            }
        }

        public static string GenerateSHAHash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return ConvertToHex(sha256.ComputeHash(Encoding.UTF8.GetBytes(input)));
            }
        }

        public static string GeneratePasswordHashWithSalt(string text)
        {
            try
            {
                return GenerateSaltedHash(text, GenerateSalt());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return "";
        }

        private static string GenerateSaltedHash(string textToHash, byte[] salt)
        {
            using (MD5 md5 = MD5.Create())
            {
                md5.TransformBlock(salt, 0, salt.Length, null, 0);
                md5.TransformFinalBlock(Encoding.UTF8.GetBytes(textToHash), 0, textToHash.Length);
                return ConvertToHex(md5.Hash);
            }
        }

        public static string GeneratePasswordHash(string password)
        {
            int iterations = 1000;
            byte[] salt = GenerateSalt();

            byte[] hash = GeneratePBEHash(password, iterations, salt, 64);

            return iterations + ":" + ConvertToHex(salt) + ":" + ConvertToHex(hash);
        }

        private static byte[] GenerateSalt()
        {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] salt = new byte[16];
                rng.GetBytes(salt);
                return salt;
            }
        }

        private static bool ValidatePassword(string originalPassword, string storedPasswordHash)
        {
            string[] parts = storedPasswordHash.Split(':');

            int iterations = int.Parse(parts[0]);
            byte[] salt = ConvertToBytes(parts[1]);
            byte[] hash = ConvertToBytes(parts[2]);

            byte[] originalPasswordHash = GeneratePBEHash(originalPassword, iterations, salt, hash.Length);

            int difference = hash.Length ^ originalPasswordHash.Length;

            for (int i = 0; i < hash.Length && i < originalPasswordHash.Length; i++)
            {
                difference |= hash[i] ^ originalPasswordHash[i];
            }

            return difference == 0;
        }

        private static byte[] GeneratePBEHash(string password, int iterations, byte[] salt, int keyLength)
        {
            using (Rfc2898DeriveBytes pbkdf2 = new(password, salt, iterations))
            {
                return pbkdf2.GetBytes(keyLength);
            }
        }

        private static byte[] ConvertToBytes(string hex)
        {
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }

        public static string GenerateChecksum(string filePath)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                using (FileStream stream = File.OpenRead(filePath))
                {
                    byte[] hash = sha256.ComputeHash(stream);
                    return ConvertToHex(hash);
                }
            }
        }
    }
}
