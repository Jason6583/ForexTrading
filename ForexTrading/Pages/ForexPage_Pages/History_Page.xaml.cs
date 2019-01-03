using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ForexTrading.Annotations;

namespace ForexTrading.Pages.ForexPage_Pages
{
    /// <summary>
    /// Page for showing trading history
    /// </summary>
    public partial class History_Page : Page, INotifyPropertyChanged
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
        /// <summary>
        /// Constructor for history page
        /// </summary>
        public History_Page()
        {
            InitializeComponent();
            DataContext = this;
        }
        /// <summary>
        /// Action when history page is loaded
        /// </summary>
        /// <param name="portofolioData"></param>
        public void LoadHistory(KeyValuePair<string[], List<string[]>> portofolioData)
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
                        ContentPresenter contentPresenter = new ContentPresenter
                        {
                            ContentTemplate = FindResource("HistoryAssetsTeplate") as DataTemplate
                        };

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
