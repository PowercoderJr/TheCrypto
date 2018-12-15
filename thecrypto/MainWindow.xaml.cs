using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static DataTemplate letterDT;

        static MainWindow()
        {
            ImapX.Message m; // DEBUG

            var spFactory = new FrameworkElementFactory(typeof(StackPanel));
            spFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            var senderTbFactory = new FrameworkElementFactory(typeof(TextBlock));
            senderTbFactory.SetValue(TextBlock.TextProperty, new Binding("From.DisplayName"));
            spFactory.AppendChild(senderTbFactory);

            var sep1TbFactory = new FrameworkElementFactory(typeof(TextBlock));
            sep1TbFactory.SetValue(TextBlock.TextProperty, " -> ");
            spFactory.AppendChild(sep1TbFactory);

            var receiverTbFactory = new FrameworkElementFactory(typeof(TextBlock));
            receiverTbFactory.SetValue(TextBlock.TextProperty, new Binding("To[0].Address"));
            spFactory.AppendChild(receiverTbFactory);

            var sep2TbFactory = new FrameworkElementFactory(typeof(TextBlock));
            sep2TbFactory.SetValue(TextBlock.TextProperty, " : ");
            spFactory.AppendChild(sep2TbFactory);

            var subjectTbFactory = new FrameworkElementFactory(typeof(TextBlock));
            subjectTbFactory.SetValue(TextBlock.TextProperty, new Binding("Subject"));
            spFactory.AppendChild(subjectTbFactory);

            var sep3TbFactory = new FrameworkElementFactory(typeof(TextBlock));
            sep3TbFactory.SetValue(TextBlock.TextProperty, " (");
            spFactory.AppendChild(sep3TbFactory);

            var datetimeTbFactory = new FrameworkElementFactory(typeof(TextBlock));
            datetimeTbFactory.SetValue(TextBlock.TextProperty, new Binding("Date"));
            spFactory.AppendChild(datetimeTbFactory);

            var sep4TbFactory = new FrameworkElementFactory(typeof(TextBlock));
            sep4TbFactory.SetValue(TextBlock.TextProperty, ")");
            spFactory.AppendChild(sep4TbFactory);

            letterDT = new DataTemplate();
            letterDT.VisualTree = spFactory;
        }

        private Account account;
        private ImapX.ImapClient imap;

        public MainWindow(Account account)
        {
            InitializeComponent();
            this.account = account;
            nameLabel.Content = account.login;

            DataContext = this;
            mailboxesLB.ItemsSource = this.account.mailboxes;
        }

        private void addMailboxBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MailboxWindow mw = new MailboxWindow();
            if (mw.ShowDialog().Value)
            {
                string address = mw.addressTB.Text.Trim();
                if (account.mailboxes.Any(item => item.address.Equals(address)))
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
                Mailbox mailbox = (Mailbox)mailboxesLB.SelectedItem;
                MailboxWindow mw = new MailboxWindow(mailbox);
                if (mw.ShowDialog().Value)
                {
                    int index = mailboxesLB.SelectedIndex;
                    // TEMP: иначе не обновляется элемент в листбоксе
                    account.mailboxes.RemoveAt(index);
                    account.mailboxes.Insert(index, mw.mailbox);
                    // TEMP/
                    account.Serialize();
                }
            }
        }

        private void removeMailboxBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (mailboxesLB.SelectedItem != null)
            {
                if (Utils.showConfirmation("Вы действительно хотите удалить " + mailboxesLB.SelectedItem +
                    " из списка почтовых ящиков?") == MessageBoxResult.Yes)
                {
                    account.mailboxes.RemoveAt(mailboxesLB.SelectedIndex);
                    account.Serialize();
                }
            }
        }

        private void mailboxesLB_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (mailboxesLB.SelectedItem != null)
            {
                currEmailLabel.Content = "Выполняется подключение...";
                lettersTV.Items.Clear();
                // TODO: выполнять подключение в отдельном потоке
                if (imapConnect((Mailbox)mailboxesLB.SelectedItem))
                {
                    loadLetters();
                    currEmailLabel.Content = mailboxesLB.SelectedItem;
                }
                else
                {
                    currEmailLabel.Content = "";
                }
            }
        }

        private bool imapConnect(Mailbox mailbox)
        {
            if (imap != null && imap.IsConnected)
                imap.Disconnect();
            imap = new ImapX.ImapClient(mailbox.imapDomain, mailbox.imapPort, account.useSsl);
            if (imap.Connect())
                try
                {
                    if (!imap.Login(mailbox.address, mailbox.password))
                        Utils.showWarning("Не удалось выполнить вход в " + mailbox.address + ". Проверьте правильность адреса и пароля.");
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
            foreach (ImapX.Folder folder in imap.Folders)
                refreshFolder(folder, lettersTV.Items);
        }

        private void refreshFolder(ImapX.Folder folder, ItemCollection collection)
        {
            TreeViewItem twi = new TreeViewItem();
            twi.Header = folder.Name;
            twi.ItemTemplate = letterDT;

            foreach (ImapX.Folder subfolder in folder.SubFolders)
                refreshFolder(subfolder, folder.Selectable ? twi.Items : lettersTV.Items);

            if (folder.Selectable)
            {
                folder.Messages.Download();
                foreach (ImapX.Message message in folder.Messages)
                    twi.Items.Add(message);

                collection.Add(twi);
            }
        }
    }
}
