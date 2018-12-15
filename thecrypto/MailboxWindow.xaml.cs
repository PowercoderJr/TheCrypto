﻿using System.Windows;

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
            nameTB.Text = name;
            addressTB.Text = address;
        }

        public MailboxWindow(Mailbox mailbox)
        {
            InitializeComponent();
            nameTB.Text = mailbox.name;
            addressTB.Text = mailbox.address;
            smtpDomainTB.Text = mailbox.smtpDomain;
            smtpPortTB.Text = mailbox.smtpPort.ToString();
            smtpAutosetChB.IsChecked = false;
            imapDomainTB.Text = mailbox.imapDomain;
            imapPortTB.Text = mailbox.imapPort.ToString();
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
                smtpPortTB.Text = "465";
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

            this.mailbox = new Mailbox(name, address, passTB.Password);
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
