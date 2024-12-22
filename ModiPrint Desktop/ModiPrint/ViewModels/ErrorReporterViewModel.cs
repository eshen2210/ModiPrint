using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ModiPrint.ViewModels
{
    /// <summary>
    /// Sends error messages to the GUI with Invoke.
    /// Essentially the ErrorListViewModel that can be used with multithreading.
    /// Create a new instance of this class for each thread.
    /// </summary>
    public class ErrorReporterViewModel
    {
        #region Fields and Properties
        //The ViewModel that displays the errors to the GUI. 
        private ErrorListViewModel _errorListViewModel;
        #endregion

        #region Constructor
        public ErrorReporterViewModel(ErrorListViewModel ErrorListViewModel)
        {
            _errorListViewModel = ErrorListViewModel;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Reports errors to the ErrorListViewModel with Dispatcher.Invoke.
        /// </summary>
        /// <param name="errorType"></param>
        /// <param name="errorMessage"></param>
        public void ReportError(string errorType, string errorMessage)
        {
            Application.Current.Dispatcher.Invoke(() =>
            _errorListViewModel.AddError(errorType, errorMessage));
        }
        #endregion
    }
}
