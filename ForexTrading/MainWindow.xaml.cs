using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using ForexTrading.Annotations;
using ForexTrading.Pages;

namespace ForexTrading
{
    /// <summary>
    /// Main widow of client aplication
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        //Pages
        private Login_Page Login_Page;
        private Register_Page Register_Page;
        private ForexPage Forex_Page;

        //Button context for minimize and maximize
        private string _isMaxed = "[ ]";
        public Core.Core Core { get; }
        public string WindowTitle { get; set; }

        private string _otherPage;
        public string OtherPage
        {
            get { return _otherPage; }
            set
            {
                _otherPage = value;
                OnPropertyChanged(nameof(OtherPage));
            }
        }

        public string IsMaxed
        {
            get { return _isMaxed; }
            set
            {
                _isMaxed = value;
                OnPropertyChanged("IsMaxed");
            }
        }
        /// <summary>
        /// Constructor for main window
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Core = new Core.Core();
            Core.LoginUser("pecho4@gmail.com", "1111");

            //Iniciliazing pages
            Login_Page = new Login_Page(this);
            Register_Page = new Register_Page(this);
          

            //Iniciliazing frame
            //ShowLoginPage();
            ShowForexPage();
            WindowTitle = "Trading forex";

        }

        /// <summary>
        /// Shows login page
        /// </summary>
        public void ShowLoginPage()
        {
            OtherPage = "Register";
            Frame_Main.Content = Login_Page;
        }
        /// <summary>
        /// Shows forex page
        /// </summary>
        public void ShowForexPage()
        {
            Forex_Page = new ForexPage(Core);
            Grid_Main.Children.Remove(TextBlock_OtherPage);
            Grid_Main.RowDefinitions.RemoveAt(0);
            Frame_Main.Content = Forex_Page;
           
        }
        /// <summary>
        /// Shows register page
        /// </summary>
        public void ShowRegisterPage()
        {
            OtherPage = "Log in";
            Frame_Main.Content = Register_Page;
        }

        /// <summary>
        /// Method for changing icon for window state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState != System.Windows.WindowState.Maximized)
            {
                IsMaxed = "[ ]";
            }
            else
            {
                IsMaxed = "[]]";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>
        /// Action for closing application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, EventArgs e)
        {
            Core.CloseConnection();
        }
        /// <summary>
        /// Changes login and register page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OtherPage_Click(object sender, MouseButtonEventArgs e)
        {
            if (OtherPage == "Register")
                ShowRegisterPage();
            else if(OtherPage == "Log in")
                ShowLoginPage();
        }
    }
}
