using System.Windows;

namespace thecrypto
{
    /// <summary>
    /// Логика взаимодействия для MailboxWindow.xaml
    /// </summary>
    public partial class MailboxWindow : Window
    {
        public const string DEFAULT_SMTP_SUBDOMAIN = "smtp.";
        public const string DEFAULT_SMTP_PORT = "587";
        public const string DEFAULT_IMAP_SUBDOMAIN = "imap.";
        public const string DEFAULT_IMAP_PORT = "993";

        internal Mailbox mailbox;

        public MailboxWindow(Mailbox mailbox)
        {
            InitializeComponent();
            this.mailbox = mailbox ?? new Mailbox("", "", "");
            nameTB.Text = this.mailbox.Name;
            addressTB.Text = this.mailbox.Address;
            smtpDomainTB.Text = this.mailbox.SmtpDomain;
            smtpPortTB.Text = this.mailbox.SmtpPort.ToString();
            smtpAutosetChB.IsChecked = mailbox == null;
            imapDomainTB.Text = this.mailbox.ImapDomain;
            imapPortTB.Text = this.mailbox.ImapPort.ToString();
            imapAutosetChB.IsChecked = mailbox == null;
            nameTB.Focus();
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            string name = nameTB.Text.Trim();
            string address = addressTB.Text.Trim();

            if (!Utils.ValidateEmail(address))
            {
                Utils.ShowWarning("Введён некорректный адрес");
                return;
            }

            if (smtpAutosetChB.IsChecked.Value)
            {
                string server = GetServerByEmail(address);
                smtpDomainTB.Text = DEFAULT_SMTP_SUBDOMAIN + server;
                smtpPortTB.Text = DEFAULT_SMTP_PORT;
            }

            if (imapAutosetChB.IsChecked.Value)
            {
                string server = GetServerByEmail(address);
                imapDomainTB.Text = DEFAULT_IMAP_SUBDOMAIN + server;
                imapPortTB.Text = DEFAULT_IMAP_PORT;
            }
            
            if (name.Length == 0 || address.Length == 0 || passTB.Password.Length == 0 ||
                    smtpDomainTB.Text.Trim().Length == 0 || smtpPortTB.Text.Length == 0 ||
                    imapDomainTB.Text.Length == 0 || imapPortTB.Text.Length == 0)
            {
                Utils.ShowWarning("Заполните все поля");
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
                Utils.ShowWarning("Введён некорректный порт");
                return;
            }

            DialogResult = true;
        }

        private static string GetServerByEmail(string email)
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
