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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ForexTrading.Windows;

namespace ForexTrading.Pages
{
    /// <summary>
    /// Page for registering user
    /// </summary>
    public partial class Register_Page : Page
    {
        MainWindow _mainWindow;
        public Register_Page(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        /// <summary>
        /// Register user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Register_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TextBox_Name.Text != "" && TextBox_SureName.Text != "" && TextBox_Login.Text != "" &&
                    TextBox_Password.Password != "")
                {
                    if (TextBox_Password.Password == TextBox_PasswordAgain.Password)
                        _mainWindow.Core.RegisterUser(TextBox_Name.Text, TextBox_SureName.Text, TextBox_Login.Text,
                            TextBox_Password.Password);
                    else
                        throw new ArgumentException("Passwords do not match");

                    CustomMessageBox.Show("You have been successfully registered");

                    _mainWindow.Frame.Content = _mainWindow.Login_Page;
                }
                else
                    throw new ArgumentException("Not every information is filled");

            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Switch between login and register
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Login_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.Frame.Content = _mainWindow.Login_Page;
        }
    }
}
