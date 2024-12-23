using System;
using System.Windows;
using System.Globalization;
using System.Windows.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ModiPrint.Utilities.IValueConverters
{
    /// <summary>
    /// Takes a bool value and returns an image of a green check or red x.
    /// </summary>
    public class BoolToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? new BitmapImage(new Uri("pack://application:,,,/Resources/GreenCheck.png")) : new BitmapImage(new Uri("pack://application:,,,/Resources/RedX.png"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
