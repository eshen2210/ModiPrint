using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using ModiPrint.Models;

/// <summary>
/// BaseViewModel is inherited by all ViewModels.
/// BaseViewModel contains common ViewModel functionalities such as INotifyPropertyChanged.
/// </summary>
namespace ModiPrint.ViewModels
{
    public class ViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        /// <summary>
        /// INotifyPropertChanged notifies subscribers in the event that a property has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
