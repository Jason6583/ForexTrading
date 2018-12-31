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
using ForexTrading.Windows;


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

        private void TextBlock_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var id = ((TextBlock)sender).Tag;
            _mainWindow.Core.SellAsset(Convert.ToInt32(id));
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
            if (System.Convert.ToDouble(((string)values[1]).Replace('.', ',')) > StylesDictionary._mainWindow.Core.GetActualValue((string)values[0]))
                return StylesDictionary._mainWindow.FindResource("Red") as SolidColorBrush;
            else
                return StylesDictionary._mainWindow.FindResource("Green") as SolidColorBrush;
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
            if (value != null)
            {
                if (System.Convert.ToDouble(((string)value).Replace('.', ',')) > 0)
                    return StylesDictionary._mainWindow.FindResource("Green") as SolidColorBrush;
                else
                    return StylesDictionary._mainWindow.FindResource("Red") as SolidColorBrush;
            }

            return Brushes.Transparent;
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


    /// <summary>
    /// Attached properties for passwordbox
    /// </summary>
    //Password box does not have Dependency property for Password
    public class PasswordBoxProperties
    {

        public static readonly DependencyProperty MonitorPasswordProperty
            = DependencyProperty.RegisterAttached(("MonitorPassword"), 
                typeof(bool),
                typeof(PasswordBoxProperties), 
                new PropertyMetadata(false, OnMonitorPasswordChanged));

        private static void OnMonitorPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var passwordBox = (PasswordBox) d;
            
            //remove old event
            passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;

            if ((bool) e.NewValue)
            {
                SetHasText(passwordBox,false);
                passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
            }
        }
        /// <summary>
        /// Event why is typed in passwordbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            SetHasText((PasswordBox)sender, false);
        }

        public static void SetMonitorPassword(PasswordBox passwordBox, bool value)
        {
            passwordBox.SetValue(MonitorPasswordProperty, true);
        }

        public static bool GetMonitorPassword(PasswordBox passwordBox)
        {
            return (bool)passwordBox.GetValue(MonitorPasswordProperty);
        }

        public static readonly DependencyProperty HasTextProperty
            = DependencyProperty.RegisterAttached(("HasText"), 
                typeof(bool), 
                typeof(PasswordBoxProperties), 
                new PropertyMetadata(false));

        public static void SetHasText(PasswordBox passwordBox, bool value)
        {
            passwordBox.SetValue(HasTextProperty, passwordBox.Password.Length > 0);
        }

        public static bool GetHasText(PasswordBox passwordBox)
        {
            return (bool)passwordBox.GetValue(HasTextProperty);
        }
    }
}





