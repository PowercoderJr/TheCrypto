using System.IO;
using System.Windows;

namespace thecrypto
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        public AuthWindow()
        {
            InitializeComponent();
            loginTB.Focus();
        }

        private void signInBtn_Click(object sender, RoutedEventArgs e)
        {
            statusLabel.Content = "";
            string login = loginTB.Text.Trim();
            if (login.Length == 0 || passTB.Password.Length == 0)
            {
                statusLabel.Content = "Введите логин и пароль";
                return;
            }

            string expectedDigest = GetAccountDigest(login);
            if (expectedDigest.Length == 0)
            {
                statusLabel.Content = "Пользователь не найден";
                return;
            }

            string saltyDigest = Utils.ByteArrayToHexString(Cryptography.
                    GetSha1(passTB.Password + Cryptography.SALT));
            if (!saltyDigest.Equals(expectedDigest))
            {
                statusLabel.Content = "Неправильный пароль";
                return;
            }

            Account account = Account.Deserialize(login);
            Start(account);
        }

        private void signUpBtn_Click(object sender, RoutedEventArgs e)
        {
            statusLabel.Content = "";
            string login = loginTB.Text.Trim();
            if (login.Length == 0 || passTB.Password.Length == 0)
            {
                statusLabel.Content = "Введите логин и пароль";
                return;
            }

            if (GetAccountDigest(login).Length > 0)
            {
                statusLabel.Content = "Пользователь с таким логином уже существует";
                return;
            }

            string saltyDigest = Utils.ByteArrayToHexString(Cryptography.
                    GetSha1(passTB.Password + Cryptography.SALT));
            Account user = new Account(login, saltyDigest);
            user.Serialize();
            using (StreamWriter fs = new StreamWriter(Account.GetAccountsListPath(), true))
            {
                fs.WriteLine(user.login);
                fs.WriteLine(user.digest);
            }
            Start(user);
        }

        private string GetAccountDigest(string login)
        {
            string digest = "";
            if (File.Exists(Account.GetAccountsListPath()))
            {
                string[] info = File.ReadAllLines(Account.GetAccountsListPath());
                for (int i = 0; digest.Length == 0 && i < info.Length; i += 2)
                    if (info[i].Equals(login))
                        digest = info[i + 1];
            }
            return digest;
        }

        private void Start(Account profile)
        {
            MainWindow mw = new MainWindow(profile);
            mw.Show();
            Close();
        }
    }
}
