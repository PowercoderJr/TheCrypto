using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace thecrypto
{
    [Serializable]
    public class Mailbox
    {

        internal string name;
        internal string address;
        internal string password;
        internal string smtpDomain;
        internal int smtpPort;
        internal string imapDomain;
        internal int imapPort;

        //internal ObservableCollection<Send_box> send_adresses = new ObservableCollection<Send_box>();

        public Mailbox(string name, string address, string password)
        {
            this.name = name;
            this.address = address;
            this.password = password;
        }

        public void setSmtpServer(string domain, int port)
        {
            this.smtpDomain = domain;
            this.smtpPort = port;
        }

        public void setImapServer(string domain, int port)
        {
            this.imapDomain = domain;
            this.imapPort = port;
        }

        public override string ToString() => name + " <" + address + ">";
    }
}
