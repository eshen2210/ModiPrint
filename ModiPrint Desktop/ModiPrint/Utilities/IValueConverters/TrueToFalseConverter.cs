using System;
using System.Windows;
using System.Windows.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModiPrint.Models;

namespace ModiPrint.Utilities.IValueConverters
{
    /// <summary>
    /// For both convert and convertback:
    /// If true return false. If false, return true.
    /// </summary>
    public class TrueToFalseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && value is bool)
            { return (bool)value == true ? false : true; }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && value is bool)
            { return (bool)value == true ? false : true; }
            return false;
        }
    }
}
