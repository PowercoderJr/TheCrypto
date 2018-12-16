using System.Windows;

namespace thecrypto
{
    /// <summary>
    /// Логика взаимодействия для MailboxWindow.xaml
    /// </summary>
    public partial class MailboxWindow : Window
    {
        public const string SMTP_SUBDOMAIN = "smtp.";
        public const string IMAP_SUBDOMAIN = "imap.";

        internal Mailbox mailbox;

        public MailboxWindow(string name="", string address="")
        {
            InitializeComponent();
            this.mailbox = new Mailbox(name, address, "");
            nameTB.Text = mailbox.Name;
            addressTB.Text = mailbox.Address;
        }

        public MailboxWindow(Mailbox mailbox)
        {
            InitializeComponent();
            this.mailbox = mailbox;
            nameTB.Text = mailbox.Name;
            addressTB.Text = mailbox.Address;
            smtpDomainTB.Text = mailbox.SmtpDomain;
            smtpPortTB.Text = mailbox.SmtpPort.ToString();
            smtpAutosetChB.IsChecked = false;
            imapDomainTB.Text = mailbox.ImapDomain;
            imapPortTB.Text = mailbox.ImapPort.ToString();
            imapAutosetChB.IsChecked = false;
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            string name = nameTB.Text.Trim();
            string address = addressTB.Text.Trim();

            if (!Utils.validateEmail(address))
            {
                Utils.showWarning("Введён некорректный адрес");
                return;
            }

            if (smtpAutosetChB.IsChecked.Value)
            {
                string server = getServerByEmail(address);
                smtpDomainTB.Text = SMTP_SUBDOMAIN + server;
                smtpPortTB.Text = "587";
            }

            if (imapAutosetChB.IsChecked.Value)
            {
                string server = getServerByEmail(address);
                imapDomainTB.Text = IMAP_SUBDOMAIN + server;
                imapPortTB.Text = "993";
            }
            
            if (name.Length == 0 || address.Length == 0 || passTB.Password.Length == 0 ||
                    smtpDomainTB.Text.Trim().Length == 0 || smtpPortTB.Text.Length == 0 ||
                    imapDomainTB.Text.Length == 0 || imapPortTB.Text.Length == 0)
            {
                Utils.showWarning("Заполните все поля");
                return;
            }

            this.mailbox.Name = name;
            this.mailbox.Address = address;
            this.mailbox.Password = passTB.Password;
            try
            {
                this.mailbox.setSmtpServer(smtpDomainTB.Text.Trim(), int.Parse(smtpPortTB.Text.Trim()));
                this.mailbox.setImapServer(imapDomainTB.Text.Trim(), int.Parse(imapPortTB.Text.Trim()));
            }
            catch
            {
                Utils.showWarning("Введён некорректный порт");
                return;
            }

            DialogResult = true;
        }

        private static string getServerByEmail(string email)
        {
            string server = email.Substring(email.IndexOf('@') + 1);
            if (server.Equals("inbox.ru") || server.Equals("bk.ru") || server.Equals("mail.ua"))
                server = "mail.ru";
            return server;
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
