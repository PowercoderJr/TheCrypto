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
    /// Логика взаимодействия для KeysManagerWindow.xaml
    /// </summary>
    public partial class KeysManagerWindow : Window
    {
        public KeysManagerWindow(Account account)
        {
            InitializeComponent();
            keysLB.Items.Add(new CryptoKey("public", "private", "Мой закрытый ключ для шифрования", "my@mail.ru", CryptoKey.Purpose.Encryption));
            keysLB.Items.Add(new CryptoKey("public", null, "Чей-то открытый ключ для шифрования", "smbd@mail.ru", CryptoKey.Purpose.Encryption));
            keysLB.Items.Add(new CryptoKey("public", "private", "Мой закрытый ключ для подписи", "my@mail.ru", CryptoKey.Purpose.Signature));
            keysLB.Items.Add(new CryptoKey("public", null, "Чей-то открытый ключ для подписи (это всё имена если что)", "sbmd@mail.ru", CryptoKey.Purpose.Signature));
        }
    }
}
