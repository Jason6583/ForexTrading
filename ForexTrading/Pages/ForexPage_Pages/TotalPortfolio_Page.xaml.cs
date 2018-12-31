using System;
using System.Collections.Generic;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ForexTrading.Annotations;
using ForexTrading.Windows;
using ForexTrading.Core;

namespace ForexTrading.Pages.ForexPage_Pages
{
    /// <summary>
    /// Interaction logic for TotalPortfolio_Page.xaml
    /// </summary>
    public partial class TotalPortfolio_Page : Page, INotifyPropertyChanged
    {
        string[] _summaryStats;
        private List<ContentPresenter> contentPresenters = new List<ContentPresenter>();
        public string[] SummaryStats
        {
            get { return _summaryStats; }
            set
            {
                _summaryStats = value;
                OnPropertyChanged("SummaryStats");
            }
        }
        public TotalPortfolio_Page()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void LoadPortfolio(KeyValuePair<string[], List<string[]>> portofolioData)
        {
            Dispatcher.Invoke(() =>
            {
                if (portofolioData.Key != null)
                {
                    //Parameters have to be invoked by Task.run

                    SummaryStats = portofolioData.Key;

                    //if asset was removed
                    while (contentPresenters.Count > portofolioData.Value.Count)
                    {
                        var contentePresenter = contentPresenters[contentPresenters.Count - 1];
                        contentPresenters.Remove(contentePresenter);
                        StackPanel_PortFolio.Children.Remove(contentePresenter);
                        //Remove separator
                        StackPanel_PortFolio.Children.RemoveAt(StackPanel_PortFolio.Children.Count - 1);
                    }

                    //If new asset was added
                    while (contentPresenters.Count < portofolioData.Value.Count)
                    {
                        ContentPresenter contentPresenter = new ContentPresenter();
                        contentPresenter.ContentTemplate = FindResource("ActiveAssetsTemplate") as DataTemplate;

                        StackPanel_PortFolio.Children.Add(contentPresenter);

                        Separator separator = new Separator()
                        {
                            Margin = new Thickness(10, 5, 10, 5),
                            Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#AF252525"))
                        };

                        StackPanel_PortFolio.Children.Add(separator);
                        contentPresenters.Add(contentPresenter);
                    }

                    int i = 0;
                    foreach (var item in portofolioData.Value)
                    {
                        (contentPresenters[i]).Content = item;
                        i++;
                    }

                }
                else if (contentPresenters.Count != 0)
                {
                    contentPresenters.Clear();
                    StackPanel_PortFolio.Children.Clear();
                    SummaryStats = new string[6];
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
