using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace thecrypto
{
    /// <summary>
    /// Логика взаимодействия для WriteLetterWindow.xaml
    /// </summary>
    public partial class WriteLetterWindow : Window
    {
        private class FileInfo
        {
            private string path;
            public string FilePath => path;
            public string FileName => System.IO.Path.GetFileName(path);

            public FileInfo(string path)
            {
                this.path = path;
            }
        }

        private Mailbox mailbox;
        private bool useSsl;

        public WriteLetterWindow(Mailbox mailbox, bool useSsl)
        {
            InitializeComponent();
            this.mailbox = mailbox;
            this.useSsl = useSsl;

            senderNameTB.Text = mailbox.Name;
            sendetAddressTB.Text = "<" + mailbox.Address + ">";
        }

        private void attachBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Прикрепить файл...";
            ofd.Multiselect = true;
            if (ofd.ShowDialog().Value)
            {
                foreach (string filename in ofd.FileNames)
                {
                    FileInfo fileInfo = new FileInfo(filename);
                    attachmentsPanel.Items.Add(fileInfo);
                }
            }
        }

        private void removeAttachmentBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            attachmentsPanel.Items.Remove((sender as FrameworkElement).DataContext);
        }

        private void sendBtn_Click(object s, RoutedEventArgs e)
        {
            if (recipientsTB.Text.Trim().Length == 0)
            {
                Utils.showWarning("Укажите хотя бы один адрес в поле \"Получатели\"");
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
                message.Body = "<html><meta charset=\"utf-8\"><body>" + bodyHtmlEditor.ContentHtml + "</body></html>";
                message.IsBodyHtml = true;
                foreach (FileInfo f in attachmentsPanel.Items)
                    message.Attachments.Add(new Attachment(f.FilePath));

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
                    Utils.showError(ex.Message);
                }
            }
        }
    }
}
