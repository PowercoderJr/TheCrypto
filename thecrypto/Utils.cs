using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Linq;

namespace thecrypto
{
    public static class Utils
    {
        public static void ShowError(string message)
        {
            MessageBox.Show(message, "Операция успешно завершена неудачей", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void ShowWarning(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public static MessageBoxResult ShowConfirmation(string message)
        {
            return MessageBox.Show(message, "Подтвердите действие", MessageBoxButton.YesNo, MessageBoxImage.Question);
        }

        public static void ShowInfo(string message)
        {
            MessageBox.Show(message, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static bool ValidateEmail(this string s)
        {
            Regex regex = new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
            return regex.IsMatch(s);
        }

        public static string ByteArrayToHexString(byte[] input)
        {
            StringBuilder output = new StringBuilder();
            foreach (byte x in input)
                output.Append(string.Format("{0:x2}", x));
            return output.ToString();
        }

        public static byte[] HexStringToByteArray(string hex)
        {
            if (hex.Length % 2 > 0)
                throw new FormatException("Строка должна иметь чётное число символов");
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static string ColorToHexString(Color c)
        {
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        public static int RemoveByCondition<T>(this ObservableCollection<T> collection, Func<T, bool> condition)
        {
            var itemsToRemove = collection.Where(condition).ToList();
            foreach (var itemToRemove in itemsToRemove)
                collection.Remove(itemToRemove);
            return itemsToRemove.Count;
        }
    }
}
