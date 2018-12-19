using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace thecrypto
{
    [Serializable]
    class CryptoKey
    {
        public enum Purpose { Encryption, Signature }

        public string Name { get; set; }
        public string Id { get; private set; }
        public string OwnerAddress { get; private set; }
        public string PublicKey { get; private set; }
        public string PrivateKey { get; private set; }
        public bool PublicOnly { get; private set; }
        public Purpose KeyPurpose { get; private set; }
        public DateTime DateTime { get; private set; }

        public CryptoKey(RSACryptoServiceProvider rsa, string name, string ownerAddress) :
                this(rsa.ToXmlString(false), rsa.PublicOnly ? null : rsa.ToXmlString(true),
                name, ownerAddress, Purpose.Encryption)
        {
            ;
        }

        public CryptoKey(DSACryptoServiceProvider dsa, string name, string ownerAddress) :
                this(dsa.ToXmlString(false), dsa.PublicOnly ? null : dsa.ToXmlString(true),
                name, ownerAddress, Purpose.Signature)
        {
            ;
        }

        internal CryptoKey(string publicKey, string privateKey, string name, string ownerAddress,
                Purpose purpose)
        {
            this.Name = name;
            this.Id = Utils.byteArrayToHexString(Cryptography.getSHA512(publicKey));
            this.OwnerAddress = ownerAddress;
            this.PublicKey = publicKey;
            this.PrivateKey = privateKey;
            this.PublicOnly = PrivateKey == null;
            this.KeyPurpose = purpose;
            this.DateTime = DateTime.Now;
        }

        public CryptoKey getPublicCryptoKey()
        {
            CryptoKey output = new CryptoKey(PublicKey, null, Name, OwnerAddress, KeyPurpose);
            output.DateTime = this.DateTime;
            return output;
        }
    }
}
