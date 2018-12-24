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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        //Pages
        public Login_Page Login_Page { get; }
        public Register_Page Register_Page { get; }
        public ForexPage Forex_Page { get; }

        //Button context for minimize and maximize
        private string _isMaxed = "[ ]";
        public Core.Core Core { get; }
        public Frame Frame { get; }
        public string WindowTitle { get; set; }
        ObservableCollection<KeyValuePair<DateTime, double>> _list;

        public ObservableCollection<KeyValuePair<DateTime, double>> List
        {
            get => _list;
            set
            {
                OnPropertyChanged("List");
                _list = value;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            Core = new Core.Core();
            
            //Iniciliazing pages
            Login_Page = new Login_Page(this);
            Register_Page = new Register_Page(this);
            Forex_Page = new ForexPage(this);

            //Iniciliazing frame
            Frame = new Frame();
            Frame.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            Grid_Main.Children.Add(Frame);

            Frame.Content = Forex_Page;

            Width = 1000;
            Height = 600;

            DataContext = this;
            WindowTitle = "Trading forex";

            InitializeComponent();
            DataContext = this;

            List = new ObservableCollection<KeyValuePair<DateTime, double>>();


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
    }
}
