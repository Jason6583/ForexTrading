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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
        MainWindow _mainWindow;
        ObservableCollection<KeyValuePair<DateTime, double>> _eurUsd;

        //Initializing menu pages
        private TotalPortfolio_Page _totalPortfolio_Page;

        bool _isSideMenuUp;
        bool _isTraidingPairsMenuUp;
        public ObservableCollection<KeyValuePair<DateTime, double>> EurUsD
        {
            get => _eurUsd;
            set
            {
                OnPropertyChanged(nameof(EurUsD));
                _eurUsd = value;
            }
        }

        public ForexPage(MainWindow mainWindow)
        {
            InitializeComponent();
            DataContext = this;

            //Intializing datasources for chart
            EurUsD = new ObservableCollection<KeyValuePair<DateTime, double>>();
            _mainWindow = mainWindow;

            _totalPortfolio_Page = new TotalPortfolio_Page();
            LoadTradingPairs();

            mainWindow.Core.Client.ReceiveDataEvent += Client_ReceiveDataEvent;
        }

        private void Client_ReceiveDataEvent(object source, Core.Client.ReceiveDataArgs args)
        {
            if (args.Name == "EURUSD")
            {
                foreach (var data in args.Data)
                {
                    EurUsD.Add(data);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void TextBlock_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isSideMenuUp)
            {
                DoubleAnimation da = new DoubleAnimation(0, 150.0, TimeSpan.FromMilliseconds(500));
                Frame_Menu.BeginAnimation(FrameworkElement.WidthProperty, da);
                _isSideMenuUp = true;

                Frame_Menu.Content = _totalPortfolio_Page;
            }
            else
            {
                DoubleAnimation da = new DoubleAnimation(150, 0.0, TimeSpan.FromMilliseconds(500));
                Frame_Menu.BeginAnimation(FrameworkElement.WidthProperty, da);
                _isSideMenuUp = false;
            }

        }
        /// <summary>
        /// Animation for showing trading menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBlock_Trading_Pairs_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            double menuHeight = 100;
            if (!_isTraidingPairsMenuUp)
            {
                DoubleAnimation da = new DoubleAnimation(0, menuHeight, TimeSpan.FromMilliseconds(300));
                Grid_TraidingPairs.BeginAnimation(FrameworkElement.HeightProperty, da);
                _isTraidingPairsMenuUp = true;
            }
            else
            {
                DoubleAnimation da = new DoubleAnimation(menuHeight, 0.0, TimeSpan.FromMilliseconds(300));
                Grid_TraidingPairs.BeginAnimation(FrameworkElement.HeightProperty, da);
                _isTraidingPairsMenuUp = false;
            }
        }
        /// <summary>
        /// Loading trading pairs for traidingPair menu
        /// </summary>
        private void LoadTradingPairs()
        {
            //var traidingPairs = _mainWindow.Core.GetAllTradingPairs();

            //CreateMenuTP(traidingPairs.Count);

            //int i = 0;
            //foreach (var item in traidingPairs)
            //{
            //    CreateMenuItemTP($"{item.FirstAsset.Name}/{item.SecondAsset.Name}", i);
            //    i++;
            //}
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
            var a = ((TextBlock)sender).Text;
        }
    }
}
