using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace thecrypto
{
    // TODO: сделать абстрактным и разбить на подклассы?
    [Serializable]
    public class CryptoKey
    {
        public const string DEFAULT_EXT = ".key";
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
            this.Id = Utils.ByteArrayToHexString(Cryptography.GetSha1(publicKey));
            this.OwnerAddress = ownerAddress;
            this.PublicKey = publicKey;
            this.PrivateKey = privateKey;
            this.PublicOnly = PrivateKey == null;
            this.KeyPurpose = purpose;
            this.DateTime = DateTime.Now;
        }

        /*public static MemoryStream serializeToStream(CryptoKey key)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, key);
            return stream;
        }

        public static object deserializeFromStream(MemoryStream stream)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            stream.Seek(0, SeekOrigin.Begin);
            object o = formatter.Deserialize(stream);
            return o;
        }*/

        public CryptoKey GetPublicCryptoKey()
        {
            CryptoKey output = new CryptoKey(PublicKey, null, Name, OwnerAddress, KeyPurpose);
            output.DateTime = this.DateTime;
            return output;
        }

        public void SerializeToFile(string filename)
        {
            using (FileStream fstream = File.Open(filename, FileMode.Create))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fstream, this);
            }
        }

        public static CryptoKey DeserializeFromFile(string filename)
        {
            CryptoKey key = null;
            using (FileStream fstream = File.Open(filename, FileMode.Open))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                key = binaryFormatter.Deserialize(fstream) as CryptoKey;
            }
            return key;
        }

        public override string ToString()
        {
            return Name + " (" + OwnerAddress + ")";
        }
    }
}
