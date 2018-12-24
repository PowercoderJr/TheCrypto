using System.Collections.ObjectModel;
using System.Windows;

namespace thecrypto
{
    /// <summary>
    /// Логика взаимодействия для AskItemWindow.xaml
    /// </summary>
    public partial class AskItemWindow : Window
    {
        private bool canBeNull;

        public AskItemWindow(string message = null, ObservableCollection<object> items = null, object value = null, bool canBeNull = false)
        {
            InitializeComponent();

            if (message != null)
                msgLabel.Text = message;

            itemsCB.ItemsSource = items;
            itemsCB.SelectedItem = value;
            this.canBeNull = canBeNull;
            itemsCB.Focus();
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            if (itemsCB.SelectedIndex < 0 && !canBeNull)
            {
                Utils.ShowWarning("Выберите значение");
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
