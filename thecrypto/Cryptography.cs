using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace thecrypto
{
    static class Cryptography
    {
        public const string SALT = "BeLucky";

        // TODO: по варрианту - SHA1
        public static byte[] getSHA512(string text)
        {
            UnicodeEncoding encoding = new UnicodeEncoding();
            byte[] message = encoding.GetBytes(text);
            string encodedData = Convert.ToBase64String(message);
            SHA512Managed hashString = new SHA512Managed();
            return hashString.ComputeHash(encoding.GetBytes(encodedData));
        }
    }
}
