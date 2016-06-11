using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using LiskMasterWallet.Properties;

namespace LiskMasterWallet.Helpers
{
    internal static class AppHelpers
    {
        public const string CreatorNotice =
            "Created by Allen Byron Penner 🙆, Lisk delegate byronp, byronp@cryptomic.com";

        public const string CopyrightNotice = "Copyright © 2016  Cryptomic L.L.C.\r\nCopyright © 2016  Lisk";

        public const string GNUGPLNotice =
            "This program is free software: you can redistribute it and/or modify\r\nit under the terms of the GNU General Public License as published by\r\nthe Free Software Foundation, either version 3 of the License, or\r\n(at your option) any later version.\r\n\r\nThis program is distributed in the hope that it will be useful,\r\nbut WITHOUT ANY WARRANTY; without even the implied warranty of\r\nMERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.\r\nSee the GNU General Public License for more details.";

        private const int HASH_BYTE_SIZE = 20;
        private const int ITERATION_INDEX = 0;
        private const int PBKDF2_INDEX = 2;
        private const int PBKDF2_ITERATIONS = 1001;
        private const int SALT_BYTE_SIZE = 16;
        private const int SALT_INDEX = 1;

        internal static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                var bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        internal static long GetServerResponseTime(string url)
        {
            var pinger = new Ping();
            try
            {
                url = url.ToLower();
                url = url.Replace("https://", "");
                url = url.Replace("http://", "");
                url = url.Replace("/", "");

                // Create a buffer of 32 bytes of data to be transmitted.
                var buffer = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                long totaltime = 0;
                // ping the host 6 times
                for (var i = 0; i < 6; i++)
                {
                    var reply = pinger.Send(url, 120, buffer, new PingOptions {DontFragment = true});
                    if (reply != null && reply.Status == IPStatus.Success)
                    {
                        totaltime += reply.RoundtripTime;
                    }
                }
                if (totaltime > 0)
                {
                    // calculate and return the average time
                    totaltime = totaltime/6;
                    return totaltime;
                }
                return -1;
            }
            catch
            {
                return -1;
            }
            finally
            {
                pinger.Dispose();
            }
        }

        public static string CreateHash(string value)
        {
            // Generate a random salt
            var csprng = new RNGCryptoServiceProvider();
            var salt = new byte[SALT_BYTE_SIZE];
            csprng.GetBytes(salt);

            // Hash the password and encode the parameters
            var hash = PBKDF2(value, salt, PBKDF2_ITERATIONS, HASH_BYTE_SIZE);
            return PBKDF2_ITERATIONS + ":" +
                   Convert.ToBase64String(salt) + ":" +
                   Convert.ToBase64String(hash);
        }

        public static bool ValidateHash(string value, string correctHash)
        {
            // Extract the parameters from the hash
            char[] delimiter = {':'};
            var split = correctHash.Split(delimiter);
            var iterations = int.Parse(split[ITERATION_INDEX]);
            var salt = Convert.FromBase64String(split[SALT_INDEX]);
            var hash = Convert.FromBase64String(split[PBKDF2_INDEX]);

            var testHash = PBKDF2(value, salt, iterations, hash.Length);
            return SlowEquals(hash, testHash);
        }

        private static byte[] PBKDF2(string value, byte[] salt, int iterations, int outputBytes)
        {
            var pbkdf2 = new Rfc2898DeriveBytes(value, salt) {IterationCount = iterations};
            return pbkdf2.GetBytes(outputBytes);
        }

        private static bool SlowEquals(byte[] a, byte[] b)
        {
            var diff = (uint) a.Length ^ (uint) b.Length;
            for (var i = 0; i < a.Length && i < b.Length; i++)
                diff |= (uint) (a[i] ^ b[i]);
            return diff == 0;
        }

        public static string GetRandomString(int length)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[length];
            var random = new Random();

            for (var i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(stringChars);
        }

        public static string GetNewCBCVector()
        {
            RETRY:
            var rs = GetRandomString(16);
            var rsb = Encoding.UTF8.GetBytes(rs);
            // this should not be needed but better safe than sorry
            if (rsb.Length == 16)
                return rs;
            goto RETRY;
        }

        public static string EncryptString(string plainText, string passPhrase)
        {
            var cbcvector = (from s in Globals.DbContext.UserSettings select s.CBCVector).First();
            var initVectorBytes = Encoding.UTF8.GetBytes(cbcvector);
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var password = new PasswordDeriveBytes(passPhrase, null);
            var keyBytes = password.GetBytes(Settings.Default.KeySize/8);
            var symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            var encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
            var memoryStream = new MemoryStream();
            var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            var cipherTextBytes = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            return Convert.ToBase64String(cipherTextBytes);
        }

        public static string DecryptString(string cipherText, string passPhrase)
        {
            var cbcvector = (from s in Globals.DbContext.UserSettings select s.CBCVector).First();
            var initVectorBytes = Encoding.ASCII.GetBytes(cbcvector);
            var cipherTextBytes = Convert.FromBase64String(cipherText);
            var password = new PasswordDeriveBytes(passPhrase, null);
            var keyBytes = password.GetBytes(Settings.Default.KeySize/8);
            var symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            var decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
            var memoryStream = new MemoryStream(cipherTextBytes);
            var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            var plainTextBytes = new byte[cipherTextBytes.Length];
            var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
        }

        internal static async Task<bool> IsSecretValid(string secret)
        {
            try
            {
                var res = await Globals.API.Accounts_Open(secret);
                return res != null && res.success && !string.IsNullOrEmpty(res.account.address);
            }
            catch (Exception crap)
            {
                Console.WriteLine("Error: AppHelpers.IsSecretValid " + crap.Message);
                return false;
            }
        }

        internal static DateTime TimestampToDateTime(string timestamp)
        {
            var ts = long.Parse(timestamp);
            return new DateTime(2016, 5, 24, 17, 0, 0, 0).AddSeconds(ts);
        }
    }
}