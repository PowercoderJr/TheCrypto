using System;
using System.Collections.Generic;
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
        private Account account;

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
                    Mailbox mailbox = new Mailbox(address, mw.passTB.Password);
                    account.mailboxes.Add(mailbox);
                    account.Serialize();
                }
            }
        }

        private void editMailboxBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mailbox mailbox = (Mailbox)mailboxesLB.SelectedItem;
            MailboxWindow mw = new MailboxWindow(mailbox.address);
            if (mw.ShowDialog().Value)
            {
                mailbox.address = mw.addressTB.Text.Trim();
                mailbox.password = mw.passTB.Password;
                int index = mailboxesLB.SelectedIndex;
                // TEMP: иначе не обновляется элемент в листбоксе
                account.mailboxes.RemoveAt(index);
                account.mailboxes.Insert(index, mailbox);
                // /TEMP
                account.Serialize();
            }
        }

        private void removeMailboxBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Utils.showConfirmation("Вы действительно хотите удалить " + mailboxesLB.SelectedItem +
                    " из списка почтовых ящиков?") == MessageBoxResult.Yes)
            {
                account.mailboxes.RemoveAt(mailboxesLB.SelectedIndex);
                account.Serialize();
            }
        }
    }
}
