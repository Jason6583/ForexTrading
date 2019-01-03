using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ForexTrading.Windows;

namespace ForexTrading.Pages.ForexPage_Pages
{
    /// <summary>
    /// Page for buying assets
    /// </summary>
    public partial class BuyAsset_Page : Page
    {
        private Core.Core _core;
        /// <summary>
        /// Constructor for buying page
        /// </summary>
        /// <param name="core"></param>
        public BuyAsset_Page(Core.Core core)
        {
            InitializeComponent();
            _core = core;
        }
        /// <summary>
        /// Action when page is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox_TradingPair.ItemsSource = _core.GetAllTradingPairs();
            ComboBox_TradingPair.SelectedValue = ComboBox_TradingPair.Items[0];
        }
        /// <summary>
        /// Action when assest is bought
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BUY_CLICK(object sender, MouseButtonEventArgs e)
        {
            try
            {
                double value = Convert.ToDouble(TextBox_Investment.Text);
                string traidingPair = ComboBox_TradingPair.SelectedValue.ToString();
                DateTime serverTime = _core.GetServerTime();

                Task.Run(() =>
                {
                    _core.AddAsset(traidingPair, serverTime,value);
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
