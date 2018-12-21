﻿using System.IO;
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

            string expectedDigest = getAccountDigest(login);
            if (expectedDigest.Length == 0)
            {
                statusLabel.Content = "Пользователь не найден";
                return;
            }

            string saltyDigest = Utils.byteArrayToHexString(Cryptography.
                    getSHA1(passTB.Password + Cryptography.SALT));
            if (!saltyDigest.Equals(expectedDigest))
            {
                statusLabel.Content = "Неправильный пароль";
                return;
            }

            Account account = new Account(login);
            account = account.Deserialize();
            start(account);
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

            if (getAccountDigest(login).Length > 0)
            {
                statusLabel.Content = "Пользователь с таким логином уже существует";
                return;
            }

            string saltyDigest = Utils.byteArrayToHexString(Cryptography.
                    getSHA1(passTB.Password + Cryptography.SALT));
            Account user = new Account(login, saltyDigest);
            user.Serialize();
            using (StreamWriter fs = new StreamWriter(Account.getAccountsListPath(), true))
            {
                fs.WriteLine(user.login);
                fs.WriteLine(user.digest);
            }
            start(user);
        }

        private string getAccountDigest(string login)
        {
            string digest = "";
            if (File.Exists(Account.getAccountsListPath()))
            {
                string[] info = File.ReadAllLines(Account.getAccountsListPath());
                for (int i = 0; digest.Length == 0 && i < info.Length; i += 2)
                    if (info[i].Equals(login))
                        digest = info[i + 1];
            }
            return digest;
        }

        private void start(Account profile)
        {
            MainWindow mw = new MainWindow(profile);
            mw.Show();
            Close();
        }
    }
}
