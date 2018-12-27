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
using ForexTrading.Core;

namespace ForexTrading.Pages.ForexPage_Pages
{
    /// <summary>
    /// Interaction logic for TotalPortfolio_Page.xaml
    /// </summary>
    public partial class TotalPortfolio_Page : Page
    {
        ForexPage _forexPage;
        public TotalPortfolio_Page(ForexPage forexPage)
        {
            InitializeComponent();
            _forexPage = forexPage;
        }

        private void TextBlock_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult result = CustomMessageBox.Show("Purchase asset",
                "Do you really want to purchase actual asset?", MessageBoxButton.YesNoCancel);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    _forexPage.BuyActualAsset();
                    break;
            }
        }

        public void LoadPortfolio(List<string[]> portofolioData)
        {
            Dispatcher.Invoke(() =>
            {
                StackPanel_PortFolio.Children.Clear();


                int i = 1;
                foreach (var item in portofolioData)
                {
                    ContentPresenter contentPresenter = new ContentPresenter();
                    contentPresenter.Content = item;
                    contentPresenter.ContentTemplate = FindResource("ActiveAssetsTemplate") as DataTemplate;
                    contentPresenter.Margin = new Thickness(5);

                    if (i != portofolioData.Count)
                    {
                        Separator separator = new Separator();
                        StackPanel_PortFolio.Children.Add(separator);
                    }

                    StackPanel_PortFolio.Children.Add(contentPresenter);
                    i++;
                }
            });
        }
    }
}
