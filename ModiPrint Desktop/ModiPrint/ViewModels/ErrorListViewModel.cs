using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace ModiPrint.ViewModels
{
    /// <summary>
    /// Keeps track of all errors that occur.
    /// </summary>
    public class ErrorListViewModel : ViewModel
    {
        //Automatically set to true when a new error is listed.
        //Automatically set to false when the user views the Error List.
        private bool _attention = false;
        public bool Attention
        {
            get { return _attention; }
            set
            {
                _attention = value;
                OnPropertyChanged("Attention");
            }
        }
        
        //What type of error is it? (Serial Communication, GCode Converter, etc.)
        private ObservableCollection<string> _errorTypeList = new ObservableCollection<string>();
        public ObservableCollection<string> ErrorTypeList
        {
            get { return _errorTypeList; }
        }

        //What is the error message? (Timeout, Unknown Command, etc.)
        private ObservableCollection<string> _errorMessageList = new ObservableCollection<string>();
        public ObservableCollection<string> ErrorMessageList
        {
            get { return _errorMessageList; }
        }

        /// <summary>
        /// Adds an entry to both error lists.
        /// </summary>
        public void AddError(string errorType, string errorMessage)
        {
            _errorTypeList.Add(errorType);
            _errorMessageList.Add(errorMessage);
            _attention = true;
            OnPropertyChanged("ErrorTypeList");
            OnPropertyChanged("ErrorMessageList");
            OnPropertyChanged("Attention");
        }
    }
}
