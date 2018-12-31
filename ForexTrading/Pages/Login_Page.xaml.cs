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
using System.Windows.Media.Animation;
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
            DataContext = this;
        }

        /// <summary>
        /// Login user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Login_Click(object sender, RoutedEventArgs e)
        {

            var email = TextBox_Login.Text;
            var password = TextBox_Password.Password;

            Task.Run(() =>
            {
                try
                {
                    _mainWindow.Core.LoginUser(email, password);
                    Dispatcher.Invoke(() => { _mainWindow.ShowForexPage(); });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => { CustomMessageBox.Show(ex.Message); });
                }
            });
        }

        /// <summary>
        /// Switch between login and register
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Create_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.ShowRegisterPage();
        }

        private void Login_Page_Loaded(object sender, RoutedEventArgs e)
        {
            Storyboard storyboard = new Storyboard();
            var slideAnmiation = new ThicknessAnimation()
            {
                Duration = new Duration(new TimeSpan(0, 0, 0, 0, 800)),
                From = new Thickness(this.WindowWidth, 0, -this.WindowWidth, 0),
                To = new Thickness(0),
                DecelerationRatio = 0.9f
            };

            var fadeAnmiation = new DoubleAnimation()
            {
                Duration = new Duration(new TimeSpan(0, 0, 0, 0, 800)),
                From =  0,
                To = 1,
            };

            Storyboard.SetTargetProperty(slideAnmiation, new PropertyPath(MarginProperty));
            Storyboard.SetTargetProperty(fadeAnmiation, new PropertyPath(OpacityProperty));

            storyboard.Children.Add(slideAnmiation);
            storyboard.Children.Add(fadeAnmiation);
            storyboard.Begin(this);

            
        }
    }
}
