using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
    /// Логика взаимодействия для KeysManagerWindow.xaml
    /// </summary>
    public partial class KeysManagerWindow : Window
    {
        private Account account;
        private ObservableCollection<CryptoKey> filteredKeys;

        public KeysManagerWindow(Account account)
        {
            InitializeComponent();
            this.account = account;
            this.filteredKeys = new ObservableCollection<CryptoKey>();
            this.keysLB.ItemsSource = this.filteredKeys;
            filterKeys();
        }

        private void addKeyBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CryptoKeyWindow ckw = new CryptoKeyWindow(account.mailboxes);
            if (ckw.ShowDialog().Value)
            {
                account.keys.Add(ckw.key);
                filterKeys();
                account.Serialize();
            }
        }

        private void removeKeyBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CryptoKey key = keysLB.SelectedItem as CryptoKey;
            if (Utils.showConfirmation("Вы действительно хотите удалить ключ \"" + 
                    key.Name + "\"?") == MessageBoxResult.Yes)
            {
                account.keys.Remove(key);
                filteredKeys.Remove(key);
                account.Serialize();
            }
        }

        private void renameKeyBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CryptoKey key = keysLB.SelectedItem as CryptoKey;
            AskStringWindow asw = new AskStringWindow("Имя ключа:", key.Name);
            if (asw.ShowDialog().Value)
            {
                key.Name = asw.valueTB.Text.Trim();
                filterKeys();
                account.Serialize();
            }
        }

        private void saveKeyBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CryptoKey key = keysLB.SelectedItem as CryptoKey;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Сохранить ключ...";
            sfd.FileName = key.Name + ".key";
            sfd.DefaultExt = ".key";
            if (sfd.ShowDialog().Value)
                using (FileStream fstream = File.Open(sfd.FileName, FileMode.Create))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(fstream, key);
                    fstream.Flush();
                }
        }

        private void sendKeyBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //string filename = System.IO.Path.GetTempFileName();
            throw new NotImplementedException();
        }

        private void encryptBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void decryptBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void filterTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            filterKeys();
        }

        private void filterKeys()
        {
            filteredKeys.Clear();
            foreach (CryptoKey key in account.keys)
                if (filterTB.Text.Length == 0 ||
                        key.Name.IndexOf(filterTB.Text, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        key.OwnerAddress.IndexOf(filterTB.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                    filteredKeys.Add(key);
        }
    }
}
