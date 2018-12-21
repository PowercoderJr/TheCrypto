using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace thecrypto
{
    public static class Utils
    {
        public static void showError(string message)
        {
            MessageBox.Show(message, "Операция успешно завершена неудачей", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        public static void showWarning(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public static MessageBoxResult showConfirmation(string message)
        {
            return MessageBox.Show(message, "Подтвердите действие", MessageBoxButton.YesNo, MessageBoxImage.Question);
        }

        public static bool validateEmail(this string s)
        {
            Regex regex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
            return regex.IsMatch(s);
        }

        public static string byteArrayToHexString(byte[] input)
        {
            StringBuilder output = new StringBuilder();
            foreach (byte x in input)
                output.Append(string.Format("{0:x2}", x));
            return output.ToString();
        }
    }
}
