using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;


namespace ForexTrading
{
    /// <summary>
    /// Class for StylesLibrary 
    /// </summary>
    public partial class StylesDictionary
    {
        public static MainWindow _mainWindow;
        private void CloseButt_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Window window = (sender as Button).Tag as Window;
            window.Close();
        }
        /// <summary>
        /// Handling event for windows button , minimize
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MinimizeButt_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Window window = (sender as Button).Tag as Window;
            window.WindowState = System.Windows.WindowState.Minimized;
        }
        /// <summary>
        /// Handling event for windows button, maximize
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// Region for menu template
        /// </summary>
        #region Menu
        TextBlock _acutalMenuItem;
        private HashSet<TextBlock> _menuItems = new HashSet<TextBlock>();
        /// <summary>
        /// Unselect all items in menu
        /// </summary>
        private void UnSellectAll()
        {
            foreach (var item in _menuItems)
            {
                item.Style = _mainWindow.FindResource("MenuTextBlockStyle") as Style;
            }
        }
        /// <summary>
        /// Handling click event for menu item 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// Handling click event for textbox(button) in ActiveAssetsTemplate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBlock_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var id = ((TextBlock)sender).Tag;
            _mainWindow.Core.SellAsset(Convert.ToInt32(id));
        }
    }
    #endregion
    /// <summary>
    /// Converter for active assets
    /// </summary>
    public sealed class IsLessThanConverter : MarkupExtension, IMultiValueConverter
    {
        /// <summary>
        /// Implements method for MarkupExtension
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
        /// <summary>
        /// Converts color for active asset
        /// </summary>
        /// <param name="values"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (System.Convert.ToDouble(((string)values[1]).Replace('.', ',')) > StylesDictionary._mainWindow.Core.GetActualValue((string)values[0]))
                return StylesDictionary._mainWindow.FindResource("Red") as SolidColorBrush;
            else
                return StylesDictionary._mainWindow.FindResource("Green") as SolidColorBrush;
        }
        /// <summary>
        /// Implements method for IMultiValueConverter
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetTypes"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    /// <summary>
    /// Convert for total portfolio 
    /// </summary>
    public sealed class TotalSumConverter : MarkupExtension, IValueConverter
    {
        /// <summary>
        /// Converter for total portfolio sum, changes color of sum 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Implements method for IValueConverter
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
        /// <summary>
        /// Implements method for MarkupExtension
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Event for password changes in passwordbox
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
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
        /// Event when is typed into passwordbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            SetHasText((PasswordBox)sender, false);
        }
        /// <summary>
        /// Sets attached property to passwordbox
        /// </summary>
        /// <param name="passwordBox"></param>
        /// <param name="value"></param>
        public static void SetMonitorPassword(PasswordBox passwordBox, bool value)
        {
            passwordBox.SetValue(MonitorPasswordProperty, true);
        }
        /// <summary>
        /// Getter for monitorpassword
        /// </summary>
        /// <param name="passwordBox"></param>
        /// <returns></returns>
        public static bool GetMonitorPassword(PasswordBox passwordBox)
        {
            return (bool)passwordBox.GetValue(MonitorPasswordProperty);
        }

        public static readonly DependencyProperty HasTextProperty
            = DependencyProperty.RegisterAttached(("HasText"), 
                typeof(bool), 
                typeof(PasswordBoxProperties), 
                new PropertyMetadata(false));
        /// <summary>
        /// Setter for HasText
        /// </summary>
        /// <param name="passwordBox"></param>
        /// <param name="value"></param>
        public static void SetHasText(PasswordBox passwordBox, bool value)
        {
            passwordBox.SetValue(HasTextProperty, passwordBox.Password.Length > 0);
        }
        /// <summary>
        /// Getter for HasText
        /// </summary>
        /// <param name="passwordBox"></param>
        /// <returns></returns>
        public static bool GetHasText(PasswordBox passwordBox)
        {
            return (bool)passwordBox.GetValue(HasTextProperty);
        }
    }
}





