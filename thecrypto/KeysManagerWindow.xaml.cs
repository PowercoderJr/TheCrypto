using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
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
            FilterKeys();
        }

        public static bool AddKey(Account account, CryptoKey key)
        {
            // TODO: вдруг ID разных ключей совпадут
            List<CryptoKey> results = account.keys.Where(k => k.Id == key.Id).ToList();
            if (results.Count == 0)
            {
                account.keys.Add(key);
                return true;
            }
            else
            {
                CryptoKey existing = results.First();
                if (existing.PublicOnly && !key.PublicOnly)
                {
                    int index = account.keys.IndexOf(existing);
                    account.keys.RemoveAt(index);
                    account.keys.Insert(index, key);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private void addKeyBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CryptoKeyWindow ckw = new CryptoKeyWindow(account.mailboxes);
            if (ckw.ShowDialog().Value)
            {
                if (AddKey(account, ckw.key))
                {
                    FilterKeys();
                    account.Serialize();
                }
                else
                {
                    Utils.ShowWarning("Такой ключ уже есть в библиотеке ключей");
                }
            }
        }

        private void removeKeyBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CryptoKey key = keysLB.SelectedItem as CryptoKey;
            if (Utils.ShowConfirmation("Вы действительно хотите удалить ключ \"" + 
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
                FilterKeys();
                account.Serialize();
            }
        }

        private void saveKeyBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CryptoKey key = keysLB.SelectedItem as CryptoKey;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Title = "Сохранить ключ...";
            sfd.FileName = key.Name + CryptoKey.DEFAULT_KEY_EXT;
            sfd.DefaultExt = CryptoKey.DEFAULT_KEY_EXT;
            if (sfd.ShowDialog().Value)
                key.SerializeToFile(sfd.FileName);
        }

        private void sendKeyBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            AskItemWindow aiw = new AskItemWindow("Выберите адрес отправителя:",
                    new ObservableCollection<object>(account.mailboxes));
            if (aiw.ShowDialog().Value)
            {
                WriteLetterWindow wlw = new WriteLetterWindow(aiw.itemsCB.SelectedItem as Mailbox,
                        account.keys, account.useSsl);
                wlw.bodyHtmlEditor.IsEnabled =
                        wlw.attachBtn.IsEnabled =
                        wlw.encryptChb.IsEnabled =
                        wlw.encryptionCB.IsEnabled =
                        wlw.signChb.IsEnabled =
                        wlw.signatureCB.IsEnabled = false;

                CryptoKey key = keysLB.SelectedItem as CryptoKey;
                wlw.KeyToDeliver = key.GetPublicCryptoKey();

                wlw.Show();
            }
        }

        // TODO: шифрование-дешифрование бинарных файлов заменяет некоторые байты на fdff
        private void encryptBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CryptoKey key = keysLB.SelectedItem as CryptoKey;
            if (key.KeyPurpose == CryptoKey.Purpose.Encryption)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = "Зашифровать файл...";
                if (ofd.ShowDialog().Value)
                {
                    byte[] input = File.ReadAllBytes(ofd.FileName);
                    string output = Cryptography.Encrypt(Cryptography.E.GetString(input), key);

                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.Title = "Сохранить зашифрованный файл...";
                    if (sfd.ShowDialog().Value)
                        File.WriteAllText(sfd.FileName, output);
                }
            }
            else if(key.KeyPurpose == CryptoKey.Purpose.Signature)
            {
                if (key.PublicOnly)
                {
                    Utils.ShowWarning("Для создания подписи необходимо иметь закрытый ключ");
                    return;
                }

                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = "Подписать файл...";
                if (ofd.ShowDialog().Value)
                {
                    byte[] input = File.ReadAllBytes(ofd.FileName);
                    string output = Cryptography.Sign(Cryptography.E.GetString(input), key);

                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.Title = "Сохранить подпись файла...";
                    sfd.DefaultExt = CryptoKey.DEFAULT_SIGNATURE_EXT;
                    if (sfd.ShowDialog().Value)
                        File.WriteAllText(sfd.FileName, output);
                }
            }
            else
            {
                throw new NotImplementedException("Как Вы здесь оказались?");
            }
        }

        private void decryptBtn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            CryptoKey key = keysLB.SelectedItem as CryptoKey;
            if (key.KeyPurpose == CryptoKey.Purpose.Encryption)
            {
                if (key.PublicOnly)
                {
                    Utils.ShowWarning("Для расшифрования файла необходимо иметь закрытый ключ");
                    return;
                }

                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = "Расшифровать файл...";
                if (ofd.ShowDialog().Value)
                {
                    string input = File.ReadAllText(ofd.FileName);
                    byte[] output;
                    try
                    {
                        output = Cryptography.E.GetBytes(Cryptography.Decrypt(input, key));
                    }
                    catch (Exception ex)
                    {
                        output = null;
                    }

                    if (output == null)
                        Utils.ShowError("Не удалось расшифровать файл");
                    else
                    {
                        SaveFileDialog sfd = new SaveFileDialog();
                        sfd.Title = "Сохранить расшифрованный файл...";
                        if (sfd.ShowDialog().Value)
                            File.WriteAllBytes(sfd.FileName, output);
                    }
                }
            }
            else if (key.KeyPurpose == CryptoKey.Purpose.Signature)
            {
                OpenFileDialog ofd1 = new OpenFileDialog();
                ofd1.Title = "Открыть файл для проверки подписи...";
                if (ofd1.ShowDialog().Value)
                {
                    OpenFileDialog ofd2 = new OpenFileDialog();
                    ofd2.Title = "Открыть файл подписи...";
                    ofd2.DefaultExt = CryptoKey.DEFAULT_SIGNATURE_EXT;
                    if (ofd2.ShowDialog().Value)
                    {
                        byte[] input = File.ReadAllBytes(ofd1.FileName);
                        string signature = File.ReadAllText(ofd2.FileName);
                        bool? result;
                        try
                        {
                            result = Cryptography.Verify(Cryptography.E.GetString(input), signature, key);
                        }
                        catch (Exception ex)
                        {
                            result = null;
                        }

                        if (result == null)
                            Utils.ShowError("Не удалось проверить подпись файла");
                        else
                        {
                            if (result.Value)
                                Utils.ShowInfo("Файл успешно прошёл верификацию");
                            else
                                Utils.ShowError("Верификация не пройдена");
                        }
                    }
                }
            }
            else
            {
                throw new NotImplementedException("Как Вы здесь оказались?");
            }
        }

        private void filterTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterKeys();
        }

        private void FilterKeys()
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
