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
using ForexTrading.Core;
using ForexTrading.Windows;

namespace ForexTrading.Pages
{
    /// <summary>
    /// Page for logging user
    /// </summary>
    public partial class Login_Page : Page
    {
        private MainWindow _mainWindow;
        public Login_Page(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        /// <summary>
        /// Login user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Login_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _mainWindow.Core.LoginUser(TextBox_Login.Text, TextBox_Password.Password);

                CustomMessageBox.Show("You have been successfully loged in");

                _mainWindow.Frame.Content = _mainWindow.Forex_Page;

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
        private void Button_Create_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.Frame.Content = _mainWindow.Register_Page;
        }
    }
}
