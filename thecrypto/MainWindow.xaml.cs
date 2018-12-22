using MailKit;
using MailKit.Net.Imap;
using Microsoft.Win32;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
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
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged, IDisposable
    {
        private static DataTemplate letterDT;

        static MainWindow()
        {
            MimeMessage m; // DEBUG

            var spFactory = new FrameworkElementFactory(typeof(StackPanel));
            spFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            var senderTbFactory = new FrameworkElementFactory(typeof(TextBlock));
            senderTbFactory.SetBinding(TextBlock.TextProperty, new Binding("From[0].Name"));
            spFactory.AppendChild(senderTbFactory);

            var sep1TbFactory = new FrameworkElementFactory(typeof(TextBlock));
            sep1TbFactory.SetValue(TextBlock.TextProperty, " -> ");
            spFactory.AppendChild(sep1TbFactory);

            var receiverTbFactory = new FrameworkElementFactory(typeof(TextBlock));
            receiverTbFactory.SetBinding(TextBlock.TextProperty, new Binding("To[0].Address"));
            spFactory.AppendChild(receiverTbFactory);

            var sep2TbFactory = new FrameworkElementFactory(typeof(TextBlock));
            sep2TbFactory.SetValue(TextBlock.TextProperty, " : ");
            spFactory.AppendChild(sep2TbFactory);

            var subjectTbFactory = new FrameworkElementFactory(typeof(TextBlock));
            subjectTbFactory.SetBinding(TextBlock.TextProperty, new Binding("Subject"));
            spFactory.AppendChild(subjectTbFactory);

            // TODO: установить триггер "прочитано-не прочитано"

            var sep3TbFactory = new FrameworkElementFactory(typeof(TextBlock));
            sep3TbFactory.SetValue(TextBlock.TextProperty, " (");
            spFactory.AppendChild(sep3TbFactory);

            var datetimeTbFactory = new FrameworkElementFactory(typeof(TextBlock));
            datetimeTbFactory.SetBinding(TextBlock.TextProperty, new Binding("Date"));
            spFactory.AppendChild(datetimeTbFactory);

            var sep4TbFactory = new FrameworkElementFactory(typeof(TextBlock));
            sep4TbFactory.SetValue(TextBlock.TextProperty, ")");
            spFactory.AppendChild(sep4TbFactory);

            letterDT = new DataTemplate();
            letterDT.VisualTree = spFactory;
        }

        private Account account;
        private Mailbox currMailbox;
        public Mailbox CurrMailbox
        {
            get => currMailbox;
            set
            {
                currMailbox = value;
                NotifyPropertyChanged("CurrMailbox");
            }
        }
        private MimeMessage currMessage;
        public MimeMessage CurrMessage
        {
            get => currMessage;
            set
            {
                currMessage = value;
                NotifyPropertyChanged("CurrMessage");
            }
        }
        private ImapClient imap;

        public event PropertyChangedEventHandler PropertyChanged;
        // This method is called by the Set accessor of each property.  
        // The CallerMemberName attribute that is applied to the optional propertyName  
        // parameter causes the property name of the caller to be substituted as an argument.  
        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow(Account account)
        {
            InitializeComponent();
            this.account = account;
            this.CurrMailbox = null;

            //DataContext = this;
            nameLabel.Content = account.login;
            mailboxesLB.ItemsSource = this.account.mailboxes;
        }

        private void addMailboxBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MailboxWindow mw = new MailboxWindow(null);
            mw.Owner = this;
            if (mw.ShowDialog().Value)
            {
                string address = mw.addressTB.Text.Trim();
                if (!account.mailboxes.Any(item => item.Address.Equals(address)))
                {
                    account.mailboxes.Add(mw.mailbox);
                    account.Serialize();
                }
                else
                {
                    Utils.ShowWarning(address + " уже есть в списке почтовых ящиков");
                }
            }
        }

        private void editMailboxBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (mailboxesLB.SelectedItem != null)
            {
                Mailbox mailbox = mailboxesLB.SelectedItem as Mailbox;
                MailboxWindow mw = new MailboxWindow(mailbox);
                mw.Owner = this;
                if (mw.ShowDialog().Value)
                {
                    int index = mailboxesLB.SelectedIndex;
                    // TEMP: иначе не обновляется элемент в листбоксе
                    account.mailboxes.RemoveAt(index);
                    account.mailboxes.Insert(index, mw.mailbox);
                    mailboxesLB.SelectedIndex = index;
                    // TEMP/
                    account.Serialize();
                }
            }
        }

        private void removeMailboxBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mailbox mailbox = mailboxesLB.SelectedItem as Mailbox;
            if (mailbox != null)
            {
                if (Utils.ShowConfirmation("Вы действительно хотите удалить " + mailboxesLB.SelectedItem +
                        " из списка почтовых ящиков? Сохранённые письма и ключи этого ящика также будут удалены.")
                        == MessageBoxResult.Yes)
                {
                    account.mailboxes.RemoveAt(mailboxesLB.SelectedIndex);
                    string mailboxPath = System.IO.Path.Combine(account.GetAccountPath(), mailbox.Address);
                    if (Directory.Exists(mailboxPath))
                        Directory.Delete(mailboxPath, true);
                    account.keys.RemoveByCondition(k => k.OwnerAddress.Equals(mailbox.Address));
                    account.Serialize();

                    if (mailbox == CurrMailbox)
                    {
                        currEmailLabel.Content = null;
                        lettersTV.Items.Clear();
                        FillLetterForm(null);
                        CurrMailbox = null;
                    }
                }
            }
        }

        private void CheckoutMailbox(Mailbox mailbox)
        {
            currEmailLabel.Content = "Выполняется подключение...";
            lettersTV.Items.Clear();
            FillLetterForm(null);
            // TODO: выполнять подключение и загрузку писем в отдельном потоке
            if (ImapConnect(mailbox))
            {
                this.CurrMailbox = mailbox;
                currEmailLabel.Content = mailbox;
                DownloadLetters();
                DisplayLetters();
            }
            else
            {
                currEmailLabel.Content = "";
            }

        }

        private void mailboxesLB_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Mailbox mailbox = mailboxesLB.SelectedItem as Mailbox;
            if (mailbox != null)
                CheckoutMailbox(mailbox);
        }

        private bool ImapConnect(Mailbox mailbox)
        {
            DisposeImap();
            imap = new ImapClient();
            try
            {
                imap.Connect(mailbox.ImapDomain, mailbox.ImapPort, account.useSsl);
                imap.Authenticate(mailbox.Address, mailbox.Password);
                /*Utils.showWarning("Не удалось выполнить вход в " + mailbox.Address + ". Проверьте правильность адреса и пароля.");
            else*/
                return true;
            }
            catch (Exception e)
            {
                Utils.ShowWarning(e.Message);
            }
            /*else
                Utils.showWarning("Ошибка соединения IMAP");*/
            return false;
        }

        public void DownloadLetters()
        {
            /*foreach (ImapFolder folder in imap.GetFolders(imap.PersonalNamespaces.First()))
                downloadFolder(folder);*/
            DownloadFolder(imap.GetFolder(imap.PersonalNamespaces.First()) as ImapFolder);
        }

        private void DownloadFolder(ImapFolder folder)
        {
            foreach (ImapFolder subfolder in folder.GetSubfolders())
                DownloadFolder(subfolder);

            if (folder.Attributes != FolderAttributes.None && (folder.Attributes & FolderAttributes.NonExistent) == 0)
            {
                folder.Open(FolderAccess.ReadOnly);
                string dirPath = System.IO.Path.Combine(account.GetAccountPath(), CurrMailbox.Address, folder.Name);
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                List<string> files = Directory.EnumerateFiles(dirPath, "*.eml").OrderBy(filename => filename).ToList();
                string last = files.Count > 0 ? files.Last().Substring(files.Last().LastIndexOf('\\') + 1) : "0";
                last = System.IO.Path.GetFileNameWithoutExtension(last);
                IList<UniqueId> uids = folder.Search(MailKit.Search.SearchQuery.Uids(
                        new UniqueIdRange(new UniqueId(uint.Parse(last) + 1), UniqueId.MaxValue)));
                foreach (UniqueId uid in uids)
                {
                    MimeMessage message = folder.GetMessage(uid);
                    message.WriteTo(System.IO.Path.Combine(dirPath, uid.ToString().PadLeft(15, '0') + ".eml"));
                }
                folder.Close();
            }
        }

        private void DisplayLetters()
        {
            string dirPath = System.IO.Path.Combine(account.GetAccountPath(), CurrMailbox.Address);
            if (!Directory.Exists(dirPath))
            {
                Utils.ShowError("Этот почтовый ящик не был синхронизирован");
                return;
            }

            DisplayFolder(dirPath, lettersTV.Items);
        }

        private void DisplayFolder(string dirPath, ItemCollection collection)
        {
            foreach (string subdirPath in Directory.GetDirectories(dirPath))
                DisplayFolder(subdirPath, collection);

            TreeViewItem twi = new TreeViewItem();
            string[] messages = Directory.GetFiles(dirPath, "*.eml");
            List<MimeMessage> buf = new List<MimeMessage>();
            foreach (string message in messages)
                buf.Add(MimeMessage.Load(message));
            buf.Reverse();
            foreach (MimeMessage message in buf)
                twi.Items.Add(message);

            twi.Header = (dirPath.Substring(dirPath.LastIndexOf('\\') + 1)) + (twi.Items.Count > 0 ?
                    (" (" + twi.Items.Count + ")") : "");
            twi.ItemTemplate = letterDT;
            
            collection.Add(twi);
        }

        private void lettersTV_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MimeMessage message = lettersTV.SelectedItem as MimeMessage;
            if (message != null)
            {
                // TODO: добавить флаг "прочитано"
                FillLetterForm(message);
            }
        }

        private void FillLetterForm(MimeMessage message)
        {
            CurrMessage = message;
            if (message == null)
            // Если нужно очистить форму
            {
                fromToDatetimeLabel.Content = fromToDatetimeLabel.ToolTip = null;
                subjectLabel.Content = subjectLabel.ToolTip = null;
                encryptionStatusLabel.Content = encryptionStatusLabel.ToolTip = null;
                signatureStatusLabel.Content = signatureStatusLabel.ToolTip = null;
                letterWB.NavigateToString("<html></html>");
                attachmentsPanel.Items.Clear();
                replyBtn.IsEnabled = false;
            }
            else
            // Если нужно открыть письмо
            {
                // Заполнение полей из заголовка
                StringBuilder fromToDatetime = new StringBuilder(message.Date +
                        " от " + message.From.Mailboxes.First().Name +
                        " <" + message.From.Mailboxes.First().Address + "> для ");
                foreach (MailboxAddress receiver in message.To)
                    fromToDatetime.Append(receiver.Name +
                        " <" + receiver.Address + ">, ");
                fromToDatetime.Remove(fromToDatetime.Length - 2, 2);
                if (message.Cc.Count > 0)
                {
                    fromToDatetime.Append("; Копии: ");
                    foreach (MailboxAddress receiver in message.Cc)
                        fromToDatetime.Append(receiver.Name +
                            " <" + receiver.Address + ">, ");
                    fromToDatetime.Remove(fromToDatetime.Length - 2, 2);
                }
                fromToDatetimeLabel.Content = fromToDatetimeLabel.ToolTip = fromToDatetime;
                subjectLabel.Content = subjectLabel.ToolTip = message.Subject;

                string body = message.HtmlBody ?? message.TextBody;
                if (message.Headers.Contains(Cryptography.ENCRYPTION_ID_HEADER))
                // Если письмо зашифровано
                {
                    List<CryptoKey> results = account.keys.Where(k => k.Id.Equals(message.
                            Headers[Cryptography.ENCRYPTION_ID_HEADER]) && !k.PublicOnly).ToList();
                    if (results.Count > 0)
                    {
                        CryptoKey key = results.First();
                        // TODO: обработать неудачу
                        body = Cryptography.Decrypt(body, key);

                        encryptionStatusLabel.Content = encryptionStatusLabel.ToolTip =
                                "Расшифровано с помощью \"" + key + "\"";
                        encryptionStatusLabel.Foreground = Brushes.Green;
                    }
                    else
                    {
                        encryptionStatusLabel.Content = encryptionStatusLabel.ToolTip =
                                "Письмо зашифровано, ключ не найден";
                        encryptionStatusLabel.Foreground = Brushes.DarkRed;
                    }
                }
                else
                {
                    encryptionStatusLabel.Content = encryptionStatusLabel.ToolTip =
                            "Письмо не зашифровано";
                    encryptionStatusLabel.Foreground = Brushes.Black;
                }

                if (message.Headers.Contains(Cryptography.SIGNATURE_ID_HEADER))
                // Если письмо подписано
                {
                    List<CryptoKey> results = account.keys.Where(k => k.Id.Equals(message.
                            Headers[Cryptography.SIGNATURE_ID_HEADER])).ToList();
                    if (results.Count > 0)
                    {
                        CryptoKey key = results.First();
                        if (Cryptography.Verify(body, message.Headers[Cryptography.SIGNATURE_HEADER], key))
                        {
                            signatureStatusLabel.Content = signatureStatusLabel.ToolTip =
                                    "Верифицировано с помощью \"" + key + "\"";
                            if (message.From.Mailboxes.First().Address.Equals(key.OwnerAddress))
                                signatureStatusLabel.Foreground = Brushes.Green;
                            else
                            {
                                signatureStatusLabel.Content = signatureStatusLabel.ToolTip +=
                                        " (отправитель не совпадает)";
                                signatureStatusLabel.Foreground = Brushes.DarkOrange;
                            }
                        }
                        else
                        {
                            signatureStatusLabel.Content = signatureStatusLabel.ToolTip =
                                    "Подпись распознана с помощью\"" + key +
                                    "\", однако целостность письма нарушена";
                            signatureStatusLabel.Foreground = Brushes.DarkRed;
                        }
                    }
                    else
                    {
                        signatureStatusLabel.Content = signatureStatusLabel.ToolTip =
                                "Письмо подписано, но нет подходящего ключа для верификации";
                        signatureStatusLabel.Foreground = Brushes.Black;
                    }
                }
                else
                {
                    signatureStatusLabel.Content = signatureStatusLabel.ToolTip =
                            "Письмо не подписано";
                    signatureStatusLabel.Foreground = Brushes.Black;
                }
                
                // Отображение тела письма
                int index = body.IndexOf("<html>", StringComparison.OrdinalIgnoreCase);
                if (index < 0)
                    body = "<html><meta charset=\"" + App.HTML_CHARSET + "\"><body>" + body + "</body></html>";
                else
                    body = body.Insert(index + 6, "<meta charset=\"" + App.HTML_CHARSET + "\">");
                // TODO: превратить переносы строк в <br>, если письмо не HTML
                letterWB.NavigateToString(body);

                if (message.Headers.Contains(Cryptography.KEY_DELIVERY_HEADER))
                // Если это письмо - доставка ключа
                {
                    // Получить объект ключа
                    MimeEntity attachment = message.Attachments.First();
                    string tmpFile = System.IO.Path.GetTempFileName();
                    SaveAttachment(attachment, tmpFile);
                    CryptoKey key = CryptoKey.DeserializeFromFile(tmpFile);

                    if (Utils.ShowConfirmation("Добавить ключ \"" + key + "\" в библиотеку ключей?") == MessageBoxResult.Yes)
                    {
                        if (KeysManagerWindow.AddKey(account, key))
                            account.Serialize();
                        else
                            Utils.ShowWarning("Такой ключ уже есть в библиотеке ключей");
                    }
                }
                else
                // Если это письмо обыкновенное
                {
                    // Отображение прикреплений
                    attachmentsPanel.Items.Clear();
                    foreach (MimeEntity attachment in message.Attachments)
                        attachmentsPanel.Items.Add(attachment);
                }
                replyBtn.IsEnabled = true;
            }
        }

        private void attachment_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MimeEntity attachment = (sender as FrameworkElement).DataContext as MimeEntity;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Сохранить файл...";
            sfd.FileName = attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name;
            if (sfd.ShowDialog(this).Value)
                SaveAttachment(attachment, sfd.FileName);
        }

        private void SaveAttachment(MimeEntity attachment, string filepath)
        {
            using (var stream = File.Create(filepath))
            {
                if (attachment is MessagePart)
                {
                    var rfc822 = (MessagePart)attachment;
                    rfc822.Message.WriteTo(stream);
                }
                else
                {
                    var part = (MimePart)attachment;
                    part.Content.DecodeTo(stream);
                }
            }
        }

        private void letterBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WriteLetterWindow wlw = new WriteLetterWindow(CurrMailbox, account.keys, account.useSsl);
            wlw.Owner = this;
            wlw.Show();
        }

        private void refreshBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            lettersTV.Items.Clear();
            DownloadLetters();
            DisplayLetters();
        }

        private void keyManagerBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            KeysManagerWindow kmw = new KeysManagerWindow(account);
            kmw.ShowDialog();
        }

        private void logoutBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            AuthWindow aw = new AuthWindow();
            Close();
            aw.Show();
        }

        private void replyBtn_Click(object sender, RoutedEventArgs e)
        {
            WriteLetterWindow wlw = new WriteLetterWindow(CurrMailbox, account.keys, account.useSsl);
            wlw.subjectTB.Text = CurrMessage.Subject;
            StringBuilder replyTo = new StringBuilder(CurrMessage.From.Mailboxes.First().Address + ", ");
            foreach (MailboxAddress receiver in CurrMessage.To)
                if (!receiver.Address.Equals(CurrMailbox.Address) &&
                        !receiver.Address.Equals(CurrMessage.From.Mailboxes.First().Address))
                    replyTo.Append(receiver.Address + ", ");
            replyTo.Remove(replyTo.Length - 2, 2);
            wlw.recipientsTB.Text = replyTo.ToString();
            wlw.Owner = this;
            wlw.Show();
        }

        private void DisposeImap()
        {
            if (imap != null)
            {
                if (imap.IsConnected)
                    imap.Disconnect(false);
                imap.Dispose();
                imap = null;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            DisposeImap();
        }

        public void Dispose(bool dummy) => DisposeImap();

        public void Dispose() => Dispose(true);
    }
}
