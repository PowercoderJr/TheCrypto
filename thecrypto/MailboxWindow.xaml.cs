using System.Windows;

namespace thecrypto
{
    /// <summary>
    /// Логика взаимодействия для MailboxWindow.xaml
    /// </summary>
    public partial class MailboxWindow : Window
    {
        public MailboxWindow(string address="")
        {
            InitializeComponent();
            addressTB.Text = address;
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            if (addressTB.Text.Trim().Equals("") || passTB.Password.Equals(""))
            {
                Utils.showWarning("Введите логин и пароль");
                return;
            }

            DialogResult = true;
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
