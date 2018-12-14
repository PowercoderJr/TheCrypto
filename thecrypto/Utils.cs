using System.Windows;

namespace thecrypto
{
    public static class Utils
    {
        public static void showWarning(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public static MessageBoxResult showConfirmation(string message)
        {
            return MessageBox.Show(message, "Подтвердите действие", MessageBoxButton.YesNo, MessageBoxImage.Question);
        }
    }
}
