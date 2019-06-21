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

namespace ConsoleServer1C
{
    /// <summary>
    /// Логика взаимодействия для ConnectTo1CServerSettingsWindow.xaml
    /// </summary>
    public partial class ConnectTo1CServerSettingsWindow : Window
    {
        public ConnectTo1CServerSettingsWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TextBoxLogin.Focus();
        }

        public string[] Data { get; private set; }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonTick_Click(object sender, RoutedEventArgs e)
        {
            Data = new string[2];
            Data[0] = AppSettings.ConverterToValue(TextBoxLogin.Text);
            Data[1] = AppSettings.ConverterToValue(PasswordBoxPassword.Password);

            Close();
        }

        private void TextBoxLogin_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                PasswordBoxPassword.Focus();
            }
        }

        private void PasswordBoxPassword_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                ButtonTick.Focus();
            }
        }
    }
}
