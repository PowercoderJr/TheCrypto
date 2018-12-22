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
    /// Логика взаимодействия для AskStringWindow.xaml
    /// </summary>
    public partial class AskStringWindow : Window
    {
        private bool canBeNull;

        public AskStringWindow(string message = null, string value = null, bool canBeNull = false)
        {
            InitializeComponent();

            if (message != null)
                msgLabel.Text = message;

            if (value != null)
                valueTB.Text = value;

            this.canBeNull = canBeNull;
            valueTB.Focus();
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            if (valueTB.Text.Trim().Length == 0 && !canBeNull)
            {
                Utils.showWarning("Введите значение");
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
