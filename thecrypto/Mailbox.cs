using System;

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
