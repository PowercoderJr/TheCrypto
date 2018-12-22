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
using System.Security.Cryptography;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Win32;
using System.Runtime.Serialization.Formatters.Binary;

namespace thecrypto
{
    /// <summary>
    /// Логика взаимодействия для CryptoKeyWindow.xaml
    /// </summary>
    public partial class CryptoKeyWindow : Window
    {
        internal CryptoKey key;

        public CryptoKeyWindow(ObservableCollection<Mailbox> mailboxes)
        {
            InitializeComponent();
            ownerCB.ItemsSource = mailboxes;
            nameTB.Focus();
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            if (loadKeyRB.IsChecked.Value)
            {
                if (filepathTB.Text.Length == 0)
                {
                    Utils.ShowWarning("Укажите путь к файлу");
                    return;
                }

                try
                {
                    using (FileStream fstream = File.Open(filepathTB.Text, FileMode.Open))
                    {
                        BinaryFormatter binaryFormatter = new BinaryFormatter();
                        key = binaryFormatter.Deserialize(fstream) as CryptoKey;
                    }
                }
                catch (Exception ex)
                {
                    Utils.ShowError(ex.Message);
                    return;
                }
            }
            else if (createKeyRB.IsChecked.Value)
            {
                string name = nameTB.Text.Trim();
                if (name.Length == 0 || ownerCB.SelectedItem == null)
                {
                    Utils.ShowWarning("Заполните все поля");
                    return;
                }

                if (encryptionPurposeRB.IsChecked.Value)
                    using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                        key = new CryptoKey(rsa, name, (ownerCB.SelectedItem as Mailbox).Address);
                else
                    using (DSACryptoServiceProvider dsa = new DSACryptoServiceProvider())
                        key = new CryptoKey(dsa, name, (ownerCB.SelectedItem as Mailbox).Address);
            }
            else
            {
                throw new NotImplementedException("Как Вы здесь оказались?");
            }
            DialogResult = true;
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void browseBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Загрузить ключ...";
            ofd.DefaultExt = CryptoKey.DEFAULT_KEY_EXT;
            if (ofd.ShowDialog().Value)
                filepathTB.Text = ofd.FileName;
        }
    }
}
