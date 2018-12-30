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
            BuyAsset,
            ActivePortfolio,
            History
        }

        Core.Core _core;
        ObservableCollection<KeyValuePair<DateTime, double>> _eurUsd;

        int _dataCount;

        //Side menu items names
        private string[] _buyAsset;
        private string[] _activePortfolio;
        private string[] _history;

        //Initializing menu pages
        private ContentPresenter _acutalSideMenuItem;
        private List<ContentPresenter> _sideMenuItems;

        private BuyAsset_Page _buyAsset_Page;
        private TotalPortfolio_Page _totalPortfolio_Page;
        private History_Page _history_Page;

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

        public string[] BuyAsset
        {
            get { return _buyAsset; }
            set
            {
                _buyAsset = value;
                OnPropertyChanged(nameof(BuyAsset));
            }
        }

        #endregion
        string _actualTradingPair;

        public ForexPage(Core.Core core)
        {
            InitializeComponent();
            _core = core;
            DataContext = this;
            DataCount = 500;
            UserEmail = core.UserEmail;

            //Initializing headers for side menu
            ActivePortfolio = new string[2];
            ActivePortfolio[0] = "Active";
            ActivePortfolio[1] = "PORTFOLIO";

            History = new string[2];
            History[0] = "HISTORY";
            History[1] = "";

            BuyAsset = new string[2];
            BuyAsset[0] = "BUY";
            BuyAsset[1] = "ASSET";

            _sideMenuItems = new List<ContentPresenter>();
            _sideMenuItems.Add(SideMenu_ActivePortfolio);
            _sideMenuItems.Add(SideMenu_History);

            //Inicializing pages

            _buyAsset_Page = new BuyAsset_Page(core);
            _history_Page = new History_Page();
            //Intializing datasources for chart

            EurUsD = new ObservableCollection<KeyValuePair<DateTime, double>>();
            ActualTradingPair = "EUR/USD";
            LoadTradingPairs();

            _core.Client.ReceiveDataEvent += Client_ReceiveDataEvent;
            _core.SoldAssetEvent += _core_SoldAssetEvent;

            _totalPortfolio_Page = new TotalPortfolio_Page();
        }

        private void _core_SoldAssetEvent(object source, EventArgs args)
        {
            _history_Page.LoadHistory(_core.GetPortFolioHistory());
        }

        private void Client_ReceiveDataEvent(object source, Core.Client.ReceiveDataArgs args)
        {
            if (args.Name == ActualTradingPair)
            {
                foreach (var data in args.Data)
                {
                    EurUsD.Add(data);
                }
            }

            if (_isSideMenuUp)
                Task.Run(() =>
                {
                    _totalPortfolio_Page.LoadPortfolio(_core.GetPortFolio());
                });
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
            var data = _core.GetData(DataCount, "EUR/USD");
            EurUsdChart.Load(new ObservableCollection<KeyValuePair<DateTime, double>>(data));

        }

        /// <summary>
        /// Region for top menu
        /// </summary>
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
            var traidingPairs = _core.GetAllTradingPairs();

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
        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Tries open same trading pairs as it is shown
            if (ActualTradingPair != ((TextBlock)sender).Text)
            {
                ActualTradingPair = ((TextBlock)sender).Text;
                EurUsdChart.Clear();

                var forexData = _core.GetData(DataCount, ActualTradingPair);
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
                case SideMenuItem.ActivePortfolio:
                    if (_acutalSideMenuItem == SideMenu_ActivePortfolio && _isSideMenuUp)
                    {
                        HideMenu(_widhtOfActivePortfolio);
                        break;
                    }

                    ChangeSizeOfSideMenu(SideMenuItem.ActivePortfolio);

                    _acutalSideMenuItem = SideMenu_ActivePortfolio;
                    Frame_Menu.Content = _totalPortfolio_Page;

                    break;
                case SideMenuItem.History:

                    if (_acutalSideMenuItem == SideMenu_History && _isSideMenuUp)
                    {
                        HideMenu(_widhtOfSideMenu);
                        break;
                    }

                    ChangeSizeOfSideMenu(SideMenuItem.History);

                    _acutalSideMenuItem = SideMenu_History;
                    Frame_Menu.Content = _history_Page;
                    break;
                case SideMenuItem.BuyAsset:

                    if (_acutalSideMenuItem == SideMenu_BuyAsset && _isSideMenuUp)
                    {
                        HideMenu(_widhtOfSideMenu);
                        break;
                    }

                    ChangeSizeOfSideMenu(SideMenuItem.BuyAsset);

                    _acutalSideMenuItem = SideMenu_BuyAsset;
                    Frame_Menu.Content = _buyAsset_Page;
                    break;
            }
        }

        KeyValuePair<string[], List<string[]>> totalPortfolio;
        private void SideMenu_TotalPortfolio_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isSideMenuUp)
            {
                SwitchMenu(SideMenuItem.ActivePortfolio);
                ShowSideMenu(_widhtOfActivePortfolio);
            }
            else
            {
                SwitchMenu(SideMenuItem.ActivePortfolio);
            }

            _totalPortfolio_Page.LoadPortfolio(_core.GetPortFolio());


        }

        private void SideMenu_History_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isSideMenuUp)
            {
                SwitchMenu(SideMenuItem.History);
                ShowSideMenu(_widhtOfActivePortfolio);
            }
            else
            {
                SwitchMenu(SideMenuItem.History);
            }

            Task.Run(() => { _history_Page.LoadHistory(_core.GetPortFolioHistory()); });

        }
        private void SideMenu_BuyAsset_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isSideMenuUp)
            {
                SwitchMenu(SideMenuItem.BuyAsset);
                ShowSideMenu(_widhtOfSideMenu);
            }
            else
            {
                SwitchMenu(SideMenuItem.BuyAsset);
            }
        }


        private double _widhtOfSideMenu = 150;
        private double _widhtOfActivePortfolio = 350;

        private void ChangeSizeOfSideMenu(SideMenuItem sideMenuItem)
        {
            if (sideMenuItem == SideMenuItem.ActivePortfolio || sideMenuItem == SideMenuItem.History)
            {
                if (_acutalSideMenuItem != SideMenu_ActivePortfolio && _acutalSideMenuItem != SideMenu_History)
                {
                    DoubleAnimation da = new DoubleAnimation(_widhtOfSideMenu, _widhtOfActivePortfolio,
                        TimeSpan.FromMilliseconds(500));
                    da.Completed += Da_Completed;

                    Frame_Menu.BeginAnimation(FrameworkElement.WidthProperty, da);
                    _isSideMenuUp = true;
                }
            }
            else
            {
                DoubleAnimation da = new DoubleAnimation(_widhtOfActivePortfolio, _widhtOfSideMenu, TimeSpan.FromMilliseconds(500));
                da.Completed += Da_Completed;

                Frame_Menu.BeginAnimation(FrameworkElement.WidthProperty, da);
                _isSideMenuUp = true;
            }
        }
        private void ShowSideMenu(double width)
        {
            DoubleAnimation da = new DoubleAnimation(0, width, TimeSpan.FromMilliseconds(500));
            da.Completed += Da_Completed;

            Frame_Menu.BeginAnimation(FrameworkElement.WidthProperty, da);
            _isSideMenuUp = true;
        }

        private void HideMenu(double width)
        {
            DoubleAnimation da = new DoubleAnimation(width, 0.0, TimeSpan.FromMilliseconds(500));

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
