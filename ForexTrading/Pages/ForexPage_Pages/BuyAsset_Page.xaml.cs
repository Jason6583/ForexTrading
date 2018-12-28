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

namespace ForexTrading.Pages.ForexPage_Pages
{
    /// <summary>
    /// Interaction logic for BuyTradingPair.xaml
    /// </summary>
    public partial class BuyAsset_Page : Page
    {
        private MainWindow _mainWindow;
        public BuyAsset_Page(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox_TradingPair.ItemsSource = _mainWindow.Core.GetAllTradingPairs();
            ComboBox_TradingPair.SelectedValue = ComboBox_TradingPair.Items[0];
        }

        private void BUY_CLICK(object sender, MouseButtonEventArgs e)
        {
            try
            {
                double value = Convert.ToDouble(TextBox_Investment.Text);
                string traidingPair = ComboBox_TradingPair.SelectedValue.ToString();
                DateTime serverTime = _mainWindow.Core.GetServerTime();

                Task.Run(() =>
                {
                    _mainWindow.Core.AddAsset(traidingPair, serverTime,value);
                });

                CustomMessageBox.Show("Asset was successfully bought");
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(ex.Message);
            }
        }
    }
}
