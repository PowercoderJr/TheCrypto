using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private static DataTemplate letterDT;

        static MainWindow()
        {
            ImapX.Message m; // DEBUG

            var spFactory = new FrameworkElementFactory(typeof(StackPanel));
            spFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            var senderTbFactory = new FrameworkElementFactory(typeof(TextBlock));
            senderTbFactory.SetBinding(TextBlock.TextProperty, new Binding("From.DisplayName"));
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
        private ImapX.Message currMessage;
        public ImapX.Message CurrMessage
        {
            get => currMessage;
            set
            {
                currMessage = value;
                NotifyPropertyChanged("CurrMessage");
            }
        }
        private ImapX.ImapClient imap;

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
            MailboxWindow mw = new MailboxWindow();
            mw.Owner = this;
            if (mw.ShowDialog().Value)
            {
                string address = mw.addressTB.Text.Trim();
                if (account.mailboxes.Any(item => item.Address.Equals(address)))
                    Utils.showWarning(address + " уже есть в списке почтовых ящиков");
                else
                {
                    account.mailboxes.Add(mw.mailbox);
                    account.Serialize();
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
            // TODO: закрыть текущий ящик, если его удалили
            Mailbox mailbox = mailboxesLB.SelectedItem as Mailbox;
            if (mailbox != null)
            {
                if (Utils.showConfirmation("Вы действительно хотите удалить " + mailboxesLB.SelectedItem +
                    " из списка почтовых ящиков?") == MessageBoxResult.Yes)
                {
                    account.mailboxes.RemoveAt(mailboxesLB.SelectedIndex);
                    account.Serialize();

                    if (mailbox == CurrMailbox)
                    {
                        currEmailLabel.Content = null;
                        lettersTV.Items.Clear();
                        fillLetterForm(null);
                        CurrMailbox = null;
                    }
                }
            }
        }

        private void checkoutMailbox(Mailbox mailbox)
        {
            currEmailLabel.Content = "Выполняется подключение...";
            lettersTV.Items.Clear();
            fillLetterForm(null);
            // TODO: выполнять подключение и загрузку писем в отдельном потоке
            if (imapConnect(mailbox))
            {
                loadLetters();
                this.CurrMailbox = mailbox;
                currEmailLabel.Content = mailbox;
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
                checkoutMailbox(mailbox);
        }

        private bool imapConnect(Mailbox mailbox)
        {
            disposeImap();
            imap = new ImapX.ImapClient(mailbox.ImapDomain, mailbox.ImapPort, account.useSsl);
            if (imap.Connect())
                try
                {
                    if (!imap.Login(mailbox.Address, mailbox.Password))
                        Utils.showWarning("Не удалось выполнить вход в " + mailbox.Address + ". Проверьте правильность адреса и пароля.");
                    else
                        return true;
                }
                catch (Exception e)
                {
                    Utils.showWarning(e.Message);
                }
            else
                Utils.showWarning("Ошибка соединения IMAP");
            return false;
        }

        public void loadLetters()
        {
            // TEMP: загружать письма с MessageFetchMode.Tiny, дозагружать с MessageFetchMode.Full перед чтением
            //imap.Behavior.MessageFetchMode = ImapX.Enums.MessageFetchMode.Flags | ImapX.Enums.MessageFetchMode.Headers;
            imap.Behavior.MessageFetchMode = ImapX.Enums.MessageFetchMode.Full;
            foreach (ImapX.Folder folder in imap.Folders)
                refreshFolder(folder, lettersTV.Items);
        }

        private void refreshFolder(ImapX.Folder folder, ItemCollection collection)
        {
            TreeViewItem twi = new TreeViewItem();

            foreach (ImapX.Folder subfolder in folder.SubFolders)
                refreshFolder(subfolder, folder.Selectable ? twi.Items : lettersTV.Items);

            if (folder.Selectable)
            {
                folder.Messages.Download();
                foreach (ImapX.Message message in folder.Messages)
                    twi.Items.Add(message);

                twi.Header = folder.Name + (twi.Items.Count > 0 ? (" (" + twi.Items.Count + ")") : "");
                twi.ItemTemplate = letterDT;
                collection.Add(twi);
            }
        }

        private void lettersTV_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ImapX.Message message = lettersTV.SelectedItem as ImapX.Message;
            if (message != null)
            //if (message.Download(ImapX.Enums.MessageFetchMode.Full, true))
            {
                message.Seen = true;
                fillLetterForm(message);
            }
        }

        private void fillLetterForm(ImapX.Message message)
        {
            CurrMessage = message;
            if (message == null)
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
            {
                StringBuilder fromToDatetime = new StringBuilder(message.Date + 
                        " от " + message.From.DisplayName + 
                        " <" + message.From.Address + "> для ");
                foreach (ImapX.MailAddress receiver in message.To)
                    fromToDatetime.Append(receiver.DisplayName +
                        " <" + receiver.Address + ">, ");
                fromToDatetime.Remove(fromToDatetime.Length - 2, 2);
                if (message.Cc.Count > 0)
                {
                    fromToDatetime.Append("; Копии: ");
                    foreach (ImapX.MailAddress receiver in message.Cc)
                        fromToDatetime.Append(receiver.DisplayName +
                            " <" + receiver.Address + ">, ");
                    fromToDatetime.Remove(fromToDatetime.Length - 2, 2);
                }
                fromToDatetimeLabel.Content = fromToDatetimeLabel.ToolTip = fromToDatetime;
                subjectLabel.Content = subjectLabel.ToolTip = message.Subject;
                encryptionStatusLabel.Content = encryptionStatusLabel.ToolTip = "Hello";
                signatureStatusLabel.Content = signatureStatusLabel.ToolTip = "World";

                string body = message.Body.Html;
                int index = body.IndexOf("<html>", StringComparison.OrdinalIgnoreCase);
                if (index < 0)
                    body = "<html><meta charset=\"utf-8\"><body>" + body + "</body></html>";
                else
                    body = body.Insert(index + 6, "<meta charset=\"" + App.HTML_CHARSET + "\">");
                letterWB.NavigateToString(body);

                attachmentsPanel.Items.Clear();
                foreach (ImapX.Attachment attachment in message.Attachments)
                    attachmentsPanel.Items.Add(attachment);
                replyBtn.IsEnabled = true;
            }
        }

        private void attachment_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ImapX.Attachment attachment = (sender as FrameworkElement).DataContext as ImapX.Attachment;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Сохранить файл...";
            sfd.FileName = attachment.FileName;
            if (sfd.ShowDialog(this).Value)
                attachment.Save(System.IO.Path.GetDirectoryName(sfd.FileName), System.IO.Path.GetFileName(sfd.FileName));
        }

        private void letterBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            WriteLetterWindow wlw = new WriteLetterWindow(CurrMailbox, account.useSsl);
            wlw.Owner = this;
            wlw.Show();
        }

        private void refreshBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            lettersTV.Items.Clear();
            loadLetters();
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
            WriteLetterWindow wlw = new WriteLetterWindow(CurrMailbox, account.useSsl);
            wlw.subjectTB.Text = CurrMessage.Subject;
            StringBuilder replyTo = new StringBuilder(CurrMessage.From.Address + ", ");
            foreach (ImapX.MailAddress receiver in CurrMessage.To)
                if (!receiver.Address.Equals(CurrMailbox.Address) &&
                        !receiver.Address.Equals(CurrMessage.From.Address))
                    replyTo.Append(receiver.Address + ", ");
            replyTo.Remove(replyTo.Length - 2, 2);
            wlw.recipientsTB.Text = replyTo.ToString();
            wlw.Owner = this;
            wlw.Show();
        }

        private void disposeImap()
        {
            if (imap != null)
            {
                if (imap.IsConnected)
                    imap.Disconnect();
                imap.Dispose();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            disposeImap();
        }
    }
}
