using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace thecrypto
{
    static class Cryptography
    {
        public const string SALT = "BeLucky";
        public const string KEY_DELIVERY_HEADER = "X-thecrypto-key-delivery-id";
        public const string ENCRYPTION_ID_HEADER = "X-thecrypto-encryption-id";
        public const string SIGNATURE_ID_HEADER = "X-thecrypto-signature-id";
        public const string SIGNATURE_HEADER = "X-thecrypto-signature";
        public static readonly Encoding E = Encoding.Unicode;
        private const bool DO_OAEP_PADDING = true;
        private const CipherMode DES_CIPHER_MODE = CipherMode.CBC;
        private const PaddingMode DES_PADDING_MODE = PaddingMode.ISO10126;
        private static readonly string SIGN_HASH_ALGORITHM_NAME = HashAlgorithmName.SHA1.Name;

        public static byte[] GetSha1(byte[] data)
        {
            byte[] hash;
            using (SHA1Managed sha1 = new SHA1Managed())
                hash = sha1.ComputeHash(data);
            return hash;
        }

        public static byte[] GetSha1(string data)
        {
            return GetSha1(E.GetBytes(data));
        }

        public static string Encrypt(string data, CryptoKey rsaPublicKey)
        {
            byte[] encryptedData, encryptedDesKey, desIV;
            try
            {
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    RSAParameters rsaParams;
                    using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                    {
                        rsa.FromXmlString(rsaPublicKey.PublicKey);
                        rsaParams = rsa.ExportParameters(false);
                    }

                    des.Mode = DES_CIPHER_MODE;
                    des.GenerateKey();
                    des.GenerateIV();
                    encryptedData = DesEncrypt(data, des.Key, des.IV);
                    encryptedDesKey = RsaEncrypt(des.Key, rsaParams, DO_OAEP_PADDING);
                    desIV = des.IV;
                }
            }
            catch (Exception ex)
            {
                Utils.ShowError(ex.Message);
                //throw;
                return null;
            }

            return new XDocument
            (
                new XElement
                (
                    "root",
                    new XElement("data", Convert.ToBase64String(encryptedData)),
                    new XElement("key", Convert.ToBase64String(encryptedDesKey)),
                    new XElement("IV", Convert.ToBase64String(desIV))
                )
            ).ToString();
        }

        public static string Decrypt(string crtptopack, CryptoKey rsaPrivateKey)
        {
            string decryptedData;
            try
            {
                RSAParameters rsaParams;
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.FromXmlString(rsaPrivateKey.PrivateKey);
                    rsaParams = rsa.ExportParameters(true);
                }

                XDocument cryptopackXml = XDocument.Parse(crtptopack);
                byte[] data = Convert.FromBase64String(cryptopackXml.Element("root").Element("data").Value);
                byte[] desKey = RsaDecrypt(Convert.FromBase64String(cryptopackXml.Element("root").Element("key").Value), rsaParams, DO_OAEP_PADDING);
                byte[] desIV = Convert.FromBase64String(cryptopackXml.Element("root").Element("IV").Value);
                decryptedData = DesDecrypt(data, desKey, desIV);
            }
            catch (Exception ex)
            {
                Utils.ShowError(ex.Message);
                //throw;
                return null;
            }

            return decryptedData;
        }

        public static byte[] DesEncrypt(string plainText, byte[] key, byte[] IV)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException("key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                des.Mode = DES_CIPHER_MODE;
                des.Key = key;
                des.IV = IV;
                des.Padding = DES_PADDING_MODE;
                ICryptoTransform encryptor = des.CreateEncryptor(des.Key, des.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt, E))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }
            return encrypted;
        }

        public static string DesDecrypt(byte[] cipherText, byte[] key, byte[] IV)
        {
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException("key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            string plaintext = null;
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                des.Mode = DES_CIPHER_MODE;
                des.Key = key;
                des.IV = IV;
                des.Padding = DES_PADDING_MODE;

                ICryptoTransform decryptor = des.CreateDecryptor(des.Key, des.IV);
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (StreamReader srDecrypt = new StreamReader(csDecrypt, E))
                    plaintext = srDecrypt.ReadToEnd();
            }
            return plaintext;
        }

        public static byte[] RsaEncrypt(byte[] dataToEncrypt, RSAParameters RSAKeyInfo, bool doOAEPPadding = DO_OAEP_PADDING)
        {
            try
            {
                byte[] encryptedData;
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportParameters(RSAKeyInfo);
                    encryptedData = rsa.Encrypt(dataToEncrypt, doOAEPPadding);
                }
                return encryptedData;
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public static byte[] RsaDecrypt(byte[] dataToDecrypt, RSAParameters RSAKeyInfo, bool doOAEPPadding = DO_OAEP_PADDING)
        {
            try
            {
                byte[] decryptedData;
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportParameters(RSAKeyInfo);
                    decryptedData = rsa.Decrypt(dataToDecrypt, doOAEPPadding);
                }
                return decryptedData;
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        public static string Sign(string data, CryptoKey dsaPrivateKey)
        {
            byte[] signature;
            using (DSACryptoServiceProvider dsa = new DSACryptoServiceProvider())
            {
                try
                {
                    DSAParameters dsaParams;
                    dsa.FromXmlString(dsaPrivateKey.PrivateKey);
                    dsaParams = dsa.ExportParameters(true);

                    byte[] hash = GetSha1(data);
                    signature = dsa.SignHash(hash, SIGN_HASH_ALGORITHM_NAME);
                }
                catch (Exception ex)
                {
                    signature = null;
                    Utils.ShowError(ex.Message);
                }
            }
            return signature == null ? null : Utils.ByteArrayToHexString(signature);
        }

        public static bool Verify(string data, string signature, CryptoKey dsaPublicKey)
        {
            using (DSACryptoServiceProvider dsa = new DSACryptoServiceProvider())
            {
                try
                {
                    DSAParameters dsaParams;
                    dsa.FromXmlString(dsaPublicKey.PublicKey);
                    dsaParams = dsa.ExportParameters(false);

                    byte[] hash = GetSha1(data);
                    return dsa.VerifyHash(hash, SIGN_HASH_ALGORITHM_NAME, Utils.HexStringToByteArray(signature));
                }
                catch (Exception ex)
                {
                    Utils.ShowError(ex.Message);
                }
            }
            return false;
        }
    }
}
