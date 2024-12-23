using System;
using System.Windows;
using System.Globalization;
using System.Windows.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModiPrint.Utilities.IValueConverters
{
    /// <summary>
    /// Used for GUI logic with ManualControlViewModel.Menu property.
    /// Return true if the string value is not "Base".
    /// </summary>
    public class IsNotBaseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((string)value == "Base") ? false : true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
