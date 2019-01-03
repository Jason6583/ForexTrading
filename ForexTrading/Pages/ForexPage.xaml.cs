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
    /// Main page for forex trading
    /// </summary>
    public partial class ForexPage : Page, INotifyPropertyChanged
    {
        /// <summary>
        /// Enum for side menu items
        /// </summary>
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
        private ActivePortfolio_Page _activePortfolio_Page;
        private History_Page _history_Page;

        bool _isSideMenuUp;
        bool _isTraidingPairsMenuUp;

        Thickness _marginOfTradingPair;
        string _userEmail;
        #region Properties
        public ObservableCollection<KeyValuePair<DateTime, double>> ForexData
        {
            get => _eurUsd;
            set
            {
                OnPropertyChanged(nameof(ForexData));
                _eurUsd = value;
            }
        }

        public int DataCount
        {
            get { return _dataCount; }
            set
            {
                _dataCount = value;
                ForexChart.DataCount = _dataCount;
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
        /// <summary>
        /// Constructor for forex page
        /// </summary>
        /// <param name="core"></param>
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

            ForexData = new ObservableCollection<KeyValuePair<DateTime, double>>();
            ActualTradingPair = "EUR/USD";
            LoadTradingPairs();

            _core.Client.ReceiveDataEvent += Client_ReceiveDataEvent;
            _core.SoldAssetEvent += _core_SoldAssetEvent;

            _activePortfolio_Page = new ActivePortfolio_Page();
        }
        /// <summary>
        /// Event when asset was sold
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        private void _core_SoldAssetEvent(object source, EventArgs args)
        {
            _history_Page.LoadHistory(_core.GetPortFolioHistory());
        }
        /// <summary>
        /// Event when asset data was received from service
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        private void Client_ReceiveDataEvent(object source, Core.Client.ReceiveDataArgs args)
        {
            if (args.Name == ActualTradingPair)
            {
                foreach (var data in args.Data)
                {
                    ForexData.Add(data);
                }
            }

            if (_isSideMenuUp)
                Task.Run(() =>
                {
                    _activePortfolio_Page.LoadPortfolio(_core.GetPortFolio());
                });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Event for property changed
        /// </summary>
        /// <param name="propertyName"></param>
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Event when 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ForexChart_Loaded(object sender, RoutedEventArgs e)
        {
            var data = _core.GetData(DataCount, "EUR/USD");
            ForexChart.Load(new ObservableCollection<KeyValuePair<DateTime, double>>(data));

        }

        /// <summary>
        /// Region for top menu
        /// </summary>
        #region Top menu region
        double _menuHeight = 100;
        /// <summary>
        /// Shows top menu when mouse enters
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {

            DoubleAnimation da = new DoubleAnimation(0, _menuHeight, TimeSpan.FromMilliseconds(300));
            Grid_TraidingPairs.BeginAnimation(FrameworkElement.HeightProperty, da);
            _isTraidingPairsMenuUp = true;

            Border_TradingPairs.BorderThickness = new Thickness(2);


        }
        /// <summary>
        /// Hides top menu when mouse leaves
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_isTraidingPairsMenuUp)
            {
                DoubleAnimation da = new DoubleAnimation(_menuHeight, 0.0, TimeSpan.FromMilliseconds(300));
                Grid_TraidingPairs.BeginAnimation(FrameworkElement.HeightProperty, da);
                _isTraidingPairsMenuUp = false;

                Border_TradingPairs.BorderThickness = new Thickness(0);
            }
        }
        /// <summary>
        /// Keeping top menu visibile when enter menu grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_TraidingPairs_MouseEnter(object sender, MouseEventArgs e)
        {
            DoubleAnimation da = new DoubleAnimation(0, _menuHeight, TimeSpan.FromMilliseconds(0));
            Grid_TraidingPairs.BeginAnimation(FrameworkElement.HeightProperty, da);
            _isTraidingPairsMenuUp = true;

            Border_TradingPairs.BorderThickness = new Thickness(2);
        }
        /// <summary>
        /// Hides menu when mouse leave from menu grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_TraidingPairs_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_isTraidingPairsMenuUp)
            {
                DoubleAnimation da = new DoubleAnimation(_menuHeight, 0.0, TimeSpan.FromMilliseconds(300));
                Grid_TraidingPairs.BeginAnimation(FrameworkElement.HeightProperty, da);
                _isTraidingPairsMenuUp = false;

                Border_TradingPairs.BorderThickness = new Thickness(0);
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
        /// Creates row and column definitions for traidingPair menu
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
        /// Creates menu item for trading pair menu
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
                var forexData = _core.GetData(DataCount, ActualTradingPair);
              
                ForexChart.MaxValue = forexData[0].Value * 0.005;
                ForexChart.MinValue = forexData[0].Value * 0.002;
                ForexChart.Clear();


                ForexChart.Load(new ObservableCollection<KeyValuePair<DateTime, double>>(forexData));
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
                    Frame_Menu.Content = _activePortfolio_Page;

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


        /// <summary>
        /// Switches side menu item to active portfolio page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SideMenu_ActivePortfolio_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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

            _activePortfolio_Page.LoadPortfolio(_core.GetPortFolio());


        }
        /// <summary>
        /// Switches side menu item to history page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// Switches side menu item to buying page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// Creates animation for side menu 
        /// </summary>
        /// <param name="sideMenuItem"></param>
        private void ChangeSizeOfSideMenu(SideMenuItem sideMenuItem)
        {
            if (sideMenuItem == SideMenuItem.ActivePortfolio || sideMenuItem == SideMenuItem.History)
            {
                if (_acutalSideMenuItem != SideMenu_ActivePortfolio && _acutalSideMenuItem != SideMenu_History)
                {
                    DoubleAnimation doubleAnimationWidth = new DoubleAnimation(_widhtOfSideMenu, _widhtOfActivePortfolio,
                        TimeSpan.FromMilliseconds(500));
                    doubleAnimationWidth.Completed += DoubleAnimationWidth_Completed;

                    Frame_Menu.BeginAnimation(FrameworkElement.WidthProperty, doubleAnimationWidth);
                    _isSideMenuUp = true;
                }
            }
            else
            {
                DoubleAnimation doubleAnimationWidth = new DoubleAnimation(_widhtOfActivePortfolio, _widhtOfSideMenu, TimeSpan.FromMilliseconds(500));
                doubleAnimationWidth.Completed += DoubleAnimationWidth_Completed;

                Frame_Menu.BeginAnimation(FrameworkElement.WidthProperty, doubleAnimationWidth);
                _isSideMenuUp = true;
            }
        }
        /// <summary>
        /// Event when animation for side menu is completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoubleAnimationWidth_Completed(object sender, EventArgs e)
        {
            ForexChart.ForceResize();
        }
        /// <summary>
        /// Creates animation for side menu and showing it
        /// </summary>
        /// <param name="width"></param>
        private void ShowSideMenu(double width)
        {
            DoubleAnimation doubleAnimationWidth = new DoubleAnimation(0, width, TimeSpan.FromMilliseconds(500));
            doubleAnimationWidth.Completed += DoubleAnimationWidth_Completed;

            Frame_Menu.BeginAnimation(FrameworkElement.WidthProperty, doubleAnimationWidth);
            _isSideMenuUp = true;
        }
        /// <summary>
        /// Hiding side menu
        /// </summary>
        /// <param name="width"></param>
        private void HideMenu(double width)
        {
            DoubleAnimation doubleAnimationWidth = new DoubleAnimation(width, 0.0, TimeSpan.FromMilliseconds(500));

            doubleAnimationWidth.Completed += DoubleAnimationWidth_Completed;
            Frame_Menu.BeginAnimation(FrameworkElement.WidthProperty, doubleAnimationWidth);
            _isSideMenuUp = false;
        }

        #endregion
        /// <summary>
        /// Event when forex page is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MarginOfTradingPair = new Thickness(TextBlock_UserEmail.ActualWidth, 0, 0, 0);

            var slideAnmiation = new ThicknessAnimation()
            {
                Duration = new Duration(new TimeSpan(0, 0, 0, 0, 500)),
                From = new Thickness(this.WindowWidth, 0, -this.WindowWidth, 0),
                To = new Thickness(0),
                DecelerationRatio = 0.9f
            };

            this.BeginAnimation(MarginProperty, slideAnmiation);
        }
    }
}
