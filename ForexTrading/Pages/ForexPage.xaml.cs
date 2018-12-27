using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using ForexTrading.Annotations;
using ForexTrading.Windows;
using ForexTrading.Pages.ForexPage_Pages;


namespace ForexTrading.Pages
{
    /// <summary>
    /// Interaction logic for ForexPage.xaml
    /// </summary>
    public partial class ForexPage : Page, INotifyPropertyChanged
    {
        private enum SideMenuItem
        {
            TotalPortfolio,
            History
        }

        MainWindow _mainWindow;
        ObservableCollection<KeyValuePair<DateTime, double>> _eurUsd;

        int _dataCount;

        //Side menu items names
        private string[] _activePortfolio;
        private string[] _history;

        //Initializing menu pages
        private ContentPresenter _acutalSideMenuItem;
        private List<ContentPresenter> _sideMenuItems;
        private TotalPortfolio_Page _totalPortfolio_Page;
        private History_Page _historyPage_Page;

        bool _isSideMenuUp;
        bool _isTraidingPairsMenuUp;

        Thickness _marginOfTradingPair;
        string _userEmail;
        #region Properties
        public ObservableCollection<KeyValuePair<DateTime, double>> EurUsD
        {
            get => _eurUsd;
            set
            {
                OnPropertyChanged(nameof(EurUsD));
                _eurUsd = value;
            }
        }

        public int DataCount
        {
            get { return _dataCount; }
            set
            {
                _dataCount = value;
                EurUsdChart.DataCount = _dataCount;
            }
        }

        public string ActualTradingPair
        {
            get { return _actualTradingPair; }
            set
            {
                _actualTradingPair = value;
                OnPropertyChanged(nameof(ActualTradingPair));
            }
        }

        public Thickness MarginOfTradingPair
        {
            get { return _marginOfTradingPair; }
            set
            {
                _marginOfTradingPair = value;
                OnPropertyChanged(nameof(MarginOfTradingPair));
            }
        }

        public string UserEmail
        {
            get { return _userEmail; }
            set
            {
                _userEmail = value;
                OnPropertyChanged(nameof(UserEmail));
            }
        }

        public string[] ActivePortfolio
        {
            get { return _activePortfolio; }
            set
            {
                _activePortfolio = value;
                OnPropertyChanged(nameof(ActivePortfolio));
            }
        }

        public string[] History
        {
            get { return _history; }
            set
            {
                _history = value;
                OnPropertyChanged(nameof(History));
            }
        }

        #endregion
        string _actualTradingPair;

        public ForexPage(MainWindow mainWindow)
        {
            InitializeComponent();
            DataContext = this;
            DataCount = 200;
            UserEmail = mainWindow.Core.UserEmail;

            ActivePortfolio = new string[2];
            ActivePortfolio[0] = "Active";
            ActivePortfolio[1] = "PORTFOLIO";

            History = new string[2];
            History[0] = "HISTORY";
            History[1] = "";

            _sideMenuItems = new List<ContentPresenter>();
            _sideMenuItems.Add(SideMenu_ActivePortfolio);
            _sideMenuItems.Add(SideMenu_History);

            //Inicializing pages
            _totalPortfolio_Page = new TotalPortfolio_Page(this);
            //Intializing datasources for chart
            _mainWindow = mainWindow;

            EurUsD = new ObservableCollection<KeyValuePair<DateTime, double>>();
            ActualTradingPair = "EUR/USD";
            LoadTradingPairs();

            mainWindow.Core.Client.ReceiveDataEvent += Client_ReceiveDataEvent;
        }

        private void Client_ReceiveDataEvent(object source, Core.Client.ReceiveDataArgs args)
        {
            if (args.Name == ActualTradingPair)
            {
                foreach (var data in args.Data)
                {
                    EurUsD.Add(data);
                    Console.WriteLine(data);
                }
            }

            if (_isSideMenuUp)
            {
                Thread thread = new Thread(() => _totalPortfolio_Page.LoadPortfolio(_mainWindow.Core.GetPortFolio()));
                thread.Start();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Da_Completed(object sender, EventArgs e)
        {
            EurUsdChart.ForceResize();
        }

        private void EurUsdChart_Loaded(object sender, RoutedEventArgs e)
        {
            var data = _mainWindow.Core.GetData(DataCount, "EUR/USD");
            EurUsdChart.Load(new ObservableCollection<KeyValuePair<DateTime, double>>(data));

        }

        public void BuyActualAsset()
        {
            Action action = () =>
            {
                _mainWindow.Core.AddAsset(ActualTradingPair, _mainWindow.Core.GetServerTime());
                CustomMessageBox.Show("Asset was purchased successfully");
                _totalPortfolio_Page.LoadPortfolio(_mainWindow.Core.GetPortFolio());

            };
            action.Invoke();
        }

        //Region for top menu events
        #region Top menu region
        double _menuHeight = 100;
        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!_isTraidingPairsMenuUp)
            {
                DoubleAnimation da = new DoubleAnimation(0, _menuHeight, TimeSpan.FromMilliseconds(300));
                Grid_TraidingPairs.BeginAnimation(FrameworkElement.HeightProperty, da);
                _isTraidingPairsMenuUp = true;
            }
            else
            {
                DoubleAnimation da = new DoubleAnimation(_menuHeight, 0.0, TimeSpan.FromMilliseconds(300));
                Grid_TraidingPairs.BeginAnimation(FrameworkElement.HeightProperty, da);
                _isTraidingPairsMenuUp = false;
            }
        }

        private void TextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_isTraidingPairsMenuUp)
            {
                DoubleAnimation da = new DoubleAnimation(_menuHeight, 0.0, TimeSpan.FromMilliseconds(300));
                Grid_TraidingPairs.BeginAnimation(FrameworkElement.HeightProperty, da);
                _isTraidingPairsMenuUp = false;
            }
        }

        private void Grid_TraidingPairs_MouseEnter(object sender, MouseEventArgs e)
        {
            DoubleAnimation da = new DoubleAnimation(0, _menuHeight, TimeSpan.FromMilliseconds(0));
            Grid_TraidingPairs.BeginAnimation(FrameworkElement.HeightProperty, da);
            _isTraidingPairsMenuUp = true;
        }

        private void Grid_TraidingPairs_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_isTraidingPairsMenuUp)
            {
                DoubleAnimation da = new DoubleAnimation(_menuHeight, 0.0, TimeSpan.FromMilliseconds(300));
                Grid_TraidingPairs.BeginAnimation(FrameworkElement.HeightProperty, da);
                _isTraidingPairsMenuUp = false;
            }
        }
        /// <summary>
        /// Loading trading pairs for traidingPair menu
        /// </summary>
        private void LoadTradingPairs()
        {
            var traidingPairs = _mainWindow.Core.GetAllTradingPairs();

            CreateMenuTP(traidingPairs.Count);

            int i = 0;
            foreach (var item in traidingPairs)
            {
                CreateMenuItemTP($"{item}", i);
                i++;
            }
        }

        /// <summary>
        /// Create row and column definitions for traidingPair menu
        /// </summary>
        private void CreateMenuTP(int rows)
        {
            Grid_TraidingPairs.Children.Clear();
            Grid_TraidingPairs.RowDefinitions.Clear();

            for (int j = 0; j < rows; j++)
            {
                RowDefinition c1 = new RowDefinition();
                c1.Height = new GridLength(25);
                Grid_TraidingPairs.RowDefinitions.Add(c1);
            }
        }

        /// <summary>
        /// Create menu item for trading pair menu
        /// </summary>
        private void CreateMenuItemTP(string text, int index)
        {
            TextBlock textBlock = new TextBlock
            {
                Style = this.FindResource("MenuTextBlockStyle") as Style,
                Text = text,
                FontSize = 15,

            };

            textBlock.MouseLeftButtonDown += TextBlock_MouseLeftButtonDown;

            textBlock.HorizontalAlignment = HorizontalAlignment.Center;

            textBlock.SetValue(Grid.RowProperty, index);
            Grid_TraidingPairs.Children.Add(textBlock);
        }

        /// <summary>
        /// Click event for trading menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private volatile Semaphore semaphore = new Semaphore(1, 10);

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Tries open same trading pairs as it is shown
            if (ActualTradingPair != ((TextBlock)sender).Text)
            {
                ActualTradingPair = ((TextBlock)sender).Text;
                EurUsdChart.Clear();
                var forexData = _mainWindow.Core.GetData(DataCount, ActualTradingPair);
                EurUsdChart.Load(new ObservableCollection<KeyValuePair<DateTime, double>>(forexData));
            }
        }

        #endregion

        /// <summary>
        /// Region for side menu events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        #region Side menu region


        private void SwitchMenu(SideMenuItem sideMenuItem)
        {
            switch (sideMenuItem)
            {
                case SideMenuItem.TotalPortfolio:
                    if (_acutalSideMenuItem == SideMenu_ActivePortfolio && _isSideMenuUp)
                    {
                        HideMenu();
                        break;
                    }

                    _acutalSideMenuItem = SideMenu_ActivePortfolio;
                    Frame_Menu.Content = _totalPortfolio_Page;
                    break;
                case SideMenuItem.History:

                    if (_acutalSideMenuItem == SideMenu_History && _isSideMenuUp)
                    {
                        HideMenu();
                        break;
                    }

                    _acutalSideMenuItem = SideMenu_History;
                    Frame_Menu.Content = _historyPage_Page;
                    break;
            }
        }


        private void SideMenu_TotalPortfolio_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isSideMenuUp)
            {
                SwitchMenu(SideMenuItem.TotalPortfolio);
                ShowSideMenu();
            }
            else
            {
                SwitchMenu(SideMenuItem.TotalPortfolio);
            }

            _totalPortfolio_Page.LoadPortfolio(_mainWindow.Core.GetPortFolio());
        }

        private void SideMenu_History_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isSideMenuUp)
            {
                SwitchMenu(SideMenuItem.History);
                ShowSideMenu();
            }
            else
            {
                SwitchMenu(SideMenuItem.History);
            }
        }

        private void ShowSideMenu()
        {
            DoubleAnimation da = new DoubleAnimation(0, 150, TimeSpan.FromMilliseconds(500));
            da.Completed += Da_Completed;

            Frame_Menu.BeginAnimation(FrameworkElement.WidthProperty, da);
            _isSideMenuUp = true;
        }

        private void HideMenu()
        {
            DoubleAnimation da = new DoubleAnimation(150, 0.0, TimeSpan.FromMilliseconds(500));

            da.Completed += Da_Completed;
            Frame_Menu.BeginAnimation(FrameworkElement.WidthProperty, da);
            _isSideMenuUp = false;
        }

        #endregion

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MarginOfTradingPair = new Thickness(TextBlock_UserEmail.ActualWidth, 0, 0, 0);
        }
    }
}
