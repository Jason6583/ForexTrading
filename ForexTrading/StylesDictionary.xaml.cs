using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using ForexTrading.Properties;


namespace ForexTrading
{
    public partial class StylesDictionary
    {
        public static MainWindow _mainWindow;
        private void CloseButt_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Window window = (sender as Button).Tag as Window;
            window.Close();
        }

        private void MinimizeButt_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Window window = (sender as Button).Tag as Window;
            window.WindowState = System.Windows.WindowState.Minimized;
        }

        private void MaximizeButt_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Window window = (sender as Button).Tag as Window;

            if (window.WindowState != System.Windows.WindowState.Maximized)
            {
                window.WindowState = System.Windows.WindowState.Maximized;
            }
            else
            {
                window.WindowState = System.Windows.WindowState.Normal;
            }
        }

        #region Menu
        TextBlock _acutalMenuItem;
        private HashSet<TextBlock> _menuItems = new HashSet<TextBlock>();
        private void UnSellectAll()
        {
            foreach (var item in _menuItems)
            {
                item.Style = _mainWindow.FindResource("MenuTextBlockStyle") as Style;
            }
        }
        private void LeftMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            UnSellectAll();

            if (_acutalMenuItem != (TextBlock)sender)
            {
                if (_mainWindow == null)
                    _mainWindow = (MainWindow)Window.GetWindow((TextBlock)sender);

                _acutalMenuItem = (TextBlock)sender;
                ((TextBlock)sender).Style = _mainWindow.FindResource("SelectedMenuTextBlockStyle") as Style;
                _menuItems.Add((TextBlock)sender);
            }
            else
            {
                _acutalMenuItem = null;
            }
        }
    }
    #endregion
    public sealed class IsLessThanConverter : MarkupExtension, IMultiValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var tradingPair = ((string)values[0]).Split('/');
            if (System.Convert.ToDouble(((string)values[1]).Replace('.', ',')) > StylesDictionary._mainWindow.Core.GetActualValue(tradingPair[0], tradingPair[1]))
                return Brushes.Red;
            else
                return Brushes.Green;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public sealed class TotalSumConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (System.Convert.ToDouble(((string)value).Replace('.', ',')) > 0)
                return Brushes.Green;
            else
                return Brushes.Red;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}





