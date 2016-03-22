using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ElkNET
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tbLogin.Text = ConnectionSettings.login;
        }

        private void btOk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ConnectionSettings.login = tbLogin.Text.Trim();
                ConnectionSettings.password = tbPassword.Password;
                DBConnection temp_con = new DBConnection(tbLogin.Text.Trim(), tbPassword.Password);
                try
                {
                    temp_con.Open();
                }
                catch (Oracle.ManagedDataAccess.Client.OracleException ex)
                {
                    if (ex.Number == 01017) { MessageBox.Show("Не верный логин/пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
                    temp_con.Close();
                    return;
                }
                this.DialogResult = true;
                this.Close();
            }
            catch
            {
                throw;
            }
        }
    }
}
