using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace thecrypto
{
    /// <summary>
    /// Логика взаимодействия для WriteLetterWindow.xaml
    /// </summary>
    public partial class WriteLetterWindow : Window
    {
        private class FileInfo
        {
            private string fullName;
            public string FullName => fullName;
            public string DisplayName => System.IO.Path.GetFileName(fullName);

            public FileInfo(string fullName)
            {
                this.fullName = fullName;
            }
        }

        private Mailbox mailbox;
        private bool useSsl;
        public CryptoKey KeyToDeliver { get; set; }

        public WriteLetterWindow(Mailbox mailbox, ObservableCollection<CryptoKey> keys, bool useSsl)
        {
            InitializeComponent();
            this.mailbox = mailbox;
            this.useSsl = useSsl;

            senderNameTB.Text = mailbox.Name;
            sendetAddressTB.Text = "<" + mailbox.Address + ">";

            ObservableCollection<CryptoKey> encryptionKeys = SelectKeys(keys, CryptoKey.Purpose.Encryption, true);
            encryptionCB.ItemsSource = encryptionKeys;
            ObservableCollection<CryptoKey> signatureKeys = SelectKeys(keys, CryptoKey.Purpose.Signature, false);
            signatureCB.ItemsSource = signatureKeys;
        }

        public void AttachFile(string fullName)
        {
            FileInfo f = new FileInfo(fullName);
            attachmentsPanel.Items.Add(f);
        }

        private void attachBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Прикрепить файл...";
            ofd.Multiselect = true;
            if (ofd.ShowDialog().Value)
                foreach (string filename in ofd.FileNames)
                    AttachFile(filename);
        }

        private void removeAttachmentBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (KeyToDeliver == null)
                attachmentsPanel.Items.Remove((sender as FrameworkElement).DataContext);
        }

        private void sendBtn_Click(object s, RoutedEventArgs e)
        {
            if (recipientsTB.Text.Trim().Length == 0)
            {
                Utils.ShowWarning("Укажите хотя бы один адрес в поле \"Получатели\"");
                return;
            }

            if (encryptChb.IsChecked.Value && encryptionCB.SelectedItem as CryptoKey == null)
            {
                Utils.ShowWarning("Выберите ключ шифрования из списка или снимите галочку");
                return;
            }

            if (signChb.IsChecked.Value && signatureCB.SelectedItem as CryptoKey == null)
            {
                Utils.ShowWarning("Выберите подпись из списка или снимите галочку");
                return;
            }

            string senderName = senderNameTB.Text.Trim();
            senderName = senderName.Length > 0 ? senderName : mailbox.Name;
            MailAddress sender = new MailAddress(mailbox.Address, senderName);
            using (MailMessage message = new MailMessage())
            {
                message.From = sender;
                message.To.Add(recipientsTB.Text);
                if (recipientsCcTB.Text.Trim().Length > 0)
                    message.CC.Add(recipientsCcTB.Text);
                if (recipientsBccTB.Text.Trim().Length > 0)
                    message.Bcc.Add(recipientsBccTB.Text);
                message.Subject = subjectTB.Text.Length > 0 ? subjectTB.Text : "Без темы";

                if (KeyToDeliver != null)
                {
                    message.Headers.Add(Cryptography.KEY_DELIVERY_HEADER, "public");

                    string filename = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "tcr-public.key");
                    KeyToDeliver.SerializeToFile(filename);
                    AttachFile(filename);

                    string purpuse;
                    if (KeyToDeliver.KeyPurpose == CryptoKey.Purpose.Encryption)
                        purpuse = "для шифрования";
                    else if (KeyToDeliver.KeyPurpose == CryptoKey.Purpose.Signature)
                        purpuse = "для верификации цифровой подписи";
                    else
                        throw new NotImplementedException("Как Вы здесь оказались?");

                    string ownerMatch;
                    if (KeyToDeliver.OwnerAddress.Equals(mailbox.Address))
                        ownerMatch = "<span style=\"color: " + Utils.ColorToHexString(Colors.Green) +
                                "\">ключ принадлежит отправителю</span>";
                    else
                        ownerMatch = "<span style=\"color: " + Utils.ColorToHexString(Colors.DarkOrange) +
                                "\">не совпадает с адресом отправителя</span>";

                    StringBuilder body = new StringBuilder();
                    body.Append("Это письмо содержит открытый ключ " + purpuse + "<br>");
                    body.Append("Адрес владельца ключа: " + KeyToDeliver.OwnerAddress + " - " + ownerMatch + "<br>");
                    body.Append("Дата и время создания ключа: " + KeyToDeliver.DateTime + "<br>");
                    body.Append("<br>");
                    body.Append("Приймите запрос в The Crypto, чтобы добавить этот ключ в Вашу библиотеку ключей");
                    bodyHtmlEditor.ContentHtml = body.ToString();
                }
                message.Body = "<html><meta charset=\"" + App.HTML_CHARSET + "\"><body>" + bodyHtmlEditor.ContentHtml + "</body></html>";

                CryptoKey signatureKey = signatureCB.SelectedItem as CryptoKey;
                if (signatureKey != null)
                {
                    string signature = Cryptography.Sign(message.Body, signatureKey);
                    message.Headers.Add(Cryptography.SIGNATURE_ID_HEADER, signatureKey.Id);
                    message.Headers.Add(Cryptography.SIGNATURE_HEADER, signature);
                }

                CryptoKey encryptionKey = encryptionCB.SelectedItem as CryptoKey;
                if (encryptionKey != null)
                {
                    message.Body = Cryptography.Encrypt(message.Body, encryptionKey);
                    message.Headers.Add(Cryptography.ENCRYPTION_ID_HEADER, encryptionKey.Id);
                }

                message.IsBodyHtml = true;
                foreach (FileInfo f in attachmentsPanel.Items)
                    message.Attachments.Add(new Attachment(f.FullName));

                try
                {
                    using (SmtpClient smtp = new SmtpClient(mailbox.SmtpDomain, mailbox.SmtpPort))
                    {
                        smtp.Credentials = new NetworkCredential(mailbox.Address, mailbox.Password);
                        smtp.EnableSsl = useSsl;
                        smtp.Send(message);
                    }
                    Close();
                }
                catch (Exception ex)
                {
                    Utils.ShowError(ex.Message);
                }
            }
        }

        private ObservableCollection<CryptoKey> SelectKeys(ObservableCollection<CryptoKey> keys,
            CryptoKey.Purpose purpose, bool includePublic)
        {
            return new ObservableCollection<CryptoKey>(keys.Where(key =>
                key.KeyPurpose == purpose && (includePublic || !key.PublicOnly)));
        }

        private void encryptChb_Unchecked(object sender, RoutedEventArgs e)
        {
            encryptionCB.SelectedItem = null;
        }

        private void encryptionCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (encryptionCB.SelectedItem != null)
                encryptChb.IsChecked = true;
        }

        private void signChb_Unchecked(object sender, RoutedEventArgs e)
        {
            signatureCB.SelectedItem = null;
        }

        private void signatureCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (signatureCB.SelectedItem != null)
                signChb.IsChecked = true;
        }
    }
}
