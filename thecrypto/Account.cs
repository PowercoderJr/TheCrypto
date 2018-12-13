using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Management;

namespace thecrypto
{
    [Serializable]
    public class Account : ICloneable
    {
        [NonSerialized]
        public const string ACCOUNTS_DIR_NAME = "accounts";
        public const string ACCOUNTS_LIST_FILENAME = "list.txt";
        public const string ACCOUNT_FILENAME = "account.tcr";

        public static string getAccountsDirPath()
        {
            return ACCOUNTS_DIR_NAME;
        }

        public static string getAccountsListPath()
        {
            return Path.Combine(ACCOUNTS_DIR_NAME, ACCOUNTS_LIST_FILENAME);
        }

        public static string getAccountFilePath(string login)
        {
            return Path.Combine(ACCOUNTS_DIR_NAME, login, ACCOUNT_FILENAME);
        }

        public static string getAccountPath(string login)
        {
            return Path.Combine(ACCOUNTS_DIR_NAME, login);
        }

        internal string login;
        internal string digest;
        internal int currMailbox;

        public List<Box> boxes = new List<Box>();

        public bool Use_save_post = false;

        //ящик
        [Serializable]
        public class Box
        {
            public string adress = "";
            public string cr_pswd = "";
            [NonSerialized]
            public string pswd = "";

            //список адресов получателей
            public List<Send_box> send_adresses = new List<Send_box>();
        }

        //адреса получателей для каждого ящика
        [Serializable]
        public class Send_box
        {
            public string adress = "";
            public string key_RSA_keys_public = "";
            public string key_RSA_DS_public = "";

            //ключи для RSA
            public string My_key_RSA_keys_private = "";
            public string My_key_RSA_keys_public = "";
            public string My_key_RSA_DS_private = "";
            public string My_key_RSA_DS_public = "";
            //ключ Rijndael
            public string My_key_Rijndael = "";
        }

        public int port_smtp = 587;
        public int port_pop3 = 995;
        public int port_imap = 993;
        public int count = 5;
        public string name_smpt = "smtp.";
        public string name_imap = "imap.";

        public byte curr_proto = 0;

        public bool ssl = true;

        public bool Crypt = false;
        public bool DS = false;

        public Account(string login, string digest="", int currMailbox=-1)
        {
            this.login = login;
            this.digest = digest;
            this.currMailbox = currMailbox;
        }

        public string getAccountPath()
        {
            return getAccountPath(login);
        }

        public string getAccountFilePath()
        {
            return getAccountFilePath(login);
        }

        // TODO: почему static?
        // TODO: по варрианту - SHA1
        // TODO: посолить
        public static string GetSHA512(string text)
        {
            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] message = UE.GetBytes(text);
            string encodedData = Convert.ToBase64String(message);

            SHA512Managed hashString = new SHA512Managed();
            byte[] hashValue = hashString.ComputeHash(UE.GetBytes(encodedData));
            string hex = "";
            foreach (byte x in hashValue)
            {
                hex += String.Format("{0:x2}", x);
            }
            return hex;
        }

        public void Serialize()
        {
            Directory.CreateDirectory(Path.Combine(ACCOUNTS_DIR_NAME, login));
            using (FileStream fstream = File.Open(getAccountFilePath(), FileMode.Create))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fstream, this);
                fstream.Flush();
            }
        }

        // TODO: зачем нестатичкеский / зачем возвращает объект?
        public Account Deserialize()
        {
            Account account = null;
            FileStream fstream = File.Open(getAccountFilePath(), FileMode.Open);
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            account = (Account)binaryFormatter.Deserialize(fstream);
            fstream.Close();
            return account;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
