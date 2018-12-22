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
using System.Collections.ObjectModel;

namespace thecrypto
{
    [Serializable]
    public class Account
    {
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
        internal bool useSsl;

        internal ObservableCollection<Mailbox> mailboxes;
        internal ObservableCollection<CryptoKey> keys;


        public Account(string login, string digest="", int currMailbox=-1)
        {
            this.login = login;
            this.digest = digest;

            this.useSsl = true;
            this.mailboxes = new ObservableCollection<Mailbox>();
            this.keys = new ObservableCollection<CryptoKey>();
        }

        public string getAccountPath()
        {
            return getAccountPath(login);
        }

        public string getAccountFilePath()
        {
            return getAccountFilePath(login);
        }

        public void serialize()
        {
            Directory.CreateDirectory(Path.Combine(ACCOUNTS_DIR_NAME, login));
            using (FileStream fstream = File.Open(getAccountFilePath(), FileMode.Create))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fstream, this);
            }
        }

        public static Account deserialize(string login)
        {
            Account account = null;
            using (FileStream fstream = File.Open(getAccountFilePath(login), FileMode.Open))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                account = binaryFormatter.Deserialize(fstream) as Account;
            }
            return account;
        }
    }
}
