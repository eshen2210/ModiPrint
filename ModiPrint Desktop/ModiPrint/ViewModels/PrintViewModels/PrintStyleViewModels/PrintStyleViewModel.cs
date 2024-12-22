using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModiPrint.Models.PrintModels.PrintStyleModels;

namespace ModiPrint.ViewModels.PrintViewModels.PrintStyleViewModels
{
    //Interfaces PrintStyleModel with the GUI.
    public abstract class PrintStyleViewModel : ViewModel
    {
        #region Fields and Properties
        //Model counterpart.
        private PrintStyleModel _printStyleModel;
        public PrintStyleModel PrintStyleModel
        {
            get { return _printStyleModel; }
            set { _printStyleModel = value; }
        }
        #endregion

        #region Constructor
        public PrintStyleViewModel(PrintStyleModel PrintStyleModel)
        {
            _printStyleModel = PrintStyleModel;
        }
        #endregion
    }
}
