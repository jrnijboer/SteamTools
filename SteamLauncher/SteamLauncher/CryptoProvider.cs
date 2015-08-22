namespace SteamLauncher
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    public class CryptoProvider
    {
        private static readonly string IV = "89d01e80-d0fd-442f-addf-2cf16797f071";               
        private static readonly string Salt = "606f6cdc-96ff-44e3-aa33-c2c0fb967cb7";

        public static string Encrypt(string plainString, string key)
        {
            try
            {
                using (AesCryptoServiceProvider myAes = GetProvider(key))
                {
                    // Encrypt the string to an array of bytes. 
                    byte[] encrypted = EncryptStringToBytes_Aes(plainString, myAes);
                    return Convert.ToBase64String(encrypted);
                }

            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static string Decrypt(string encryptedString, string key)
        {
            try
            {
                using (AesCryptoServiceProvider myAes = GetProvider(key))
                {
                    // Encrypt the string to an array of bytes. 
                    var encryptedBytes = Convert.FromBase64String(encryptedString);
                    var decrypted = DecryptStringFromBytes_Aes(encryptedBytes, myAes);

                    return decrypted;
                }

            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private static AesCryptoServiceProvider GetProvider(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("Key");

            key = key + Salt;

            var provider = new AesCryptoServiceProvider();
            provider.BlockSize = 128;

            var mySHA256 = SHA256.Create();
            mySHA256.ComputeHash(Encoding.Unicode.GetBytes(key));

            if (mySHA256.Hash == null || mySHA256.Hash.Length <= 0)
                throw new ArgumentNullException("IV");

            provider.Key = mySHA256.Hash;

            var md5 = MD5.Create();
            md5.ComputeHash(Encoding.Unicode.GetBytes(CryptoProvider.IV));

            if (md5.Hash == null || md5.Hash.Length <= 0)
                throw new ArgumentNullException("IV");

            provider.IV = md5.Hash;

            return provider;
        }

        static byte[] EncryptStringToBytes_Aes(string plainText, AesCryptoServiceProvider cryptoProvider)
        {
            // Check arguments. 
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");

            byte[] encrypted;

            // Create a decrytor to perform the stream transform.
            ICryptoTransform encryptor = cryptoProvider.CreateEncryptor(cryptoProvider.Key, cryptoProvider.IV);

            // Create the streams used for encryption. 
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {

                        //Write all data to the stream.
                        swEncrypt.Write(plainText);
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }

            return encrypted;
        }

        static string DecryptStringFromBytes_Aes(byte[] cipherText, AesCryptoServiceProvider cryptoProvider)
        {
            // Check arguments. 
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");

            string plaintext = null;
                        
            // Create a decrytor to perform the stream transform.
            ICryptoTransform decryptor = cryptoProvider.CreateDecryptor(cryptoProvider.Key, cryptoProvider.IV);

            // Create the streams used for decryption. 
            using (MemoryStream msDecrypt = new MemoryStream(cipherText))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {

                        // Read the decrypted bytes from the decrypting stream 
                        // and place them in a string.
                        plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }

            return plaintext;
        }
    }
}