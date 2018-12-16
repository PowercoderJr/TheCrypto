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

        public string Name { get; set; }
        public string Address { get; set; }
        public string Password { get; set; }
        public string SmtpDomain { get; set; }
        public int SmtpPort { get; set; }
        public string ImapDomain { get; set; }
        public int ImapPort { get; set; }

        //internal ObservableCollection<Send_box> send_adresses = new ObservableCollection<Send_box>();

        public Mailbox(string name, string address, string password)
        {
            this.Name = name;
            this.Address = address;
            this.Password = password;
        }

        public void setSmtpServer(string domain, int port)
        {
            this.SmtpDomain = domain;
            this.SmtpPort = port;
        }

        public void setImapServer(string domain, int port)
        {
            this.ImapDomain = domain;
            this.ImapPort = port;
        }

        public override string ToString() => Name + " <" + Address + ">";
    }
}
