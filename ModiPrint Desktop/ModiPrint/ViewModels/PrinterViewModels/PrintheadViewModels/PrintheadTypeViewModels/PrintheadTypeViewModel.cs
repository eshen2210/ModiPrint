using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ModiPrint.Models.PrinterModels.PrintheadModels.PrintheadTypeModels;
using ModiPrint.ViewModels.PrinterViewModels.MicrocontrollerViewModels;

namespace ModiPrint.ViewModels.PrinterViewModels.PrintheadViewModels.PrintheadTypeViewModels
{
    /// <summary>
    /// Interface between PrintheadModel and the GUI.
    /// </summary>
    public abstract class PrintheadTypeViewModel : ViewModel
    {
        #region Fields and Properties
        //Model counterpart.
        protected PrintheadTypeModel _printheadTypeModel;
        public PrintheadTypeModel PrintheadTypeModel
        {
            get { return _printheadTypeModel; }
        }

        //Name of the Printhead that this class is extending.
        public string AttachedName
        {
            get { return _printheadTypeModel.AttachedName; }
        }
        
        //Provides lists of GPIOPins sorted by function.
        protected GPIOPinListsViewModel _gPIOPinListsViewModel;
        #endregion

        #region Constructor
        public PrintheadTypeViewModel(PrintheadTypeModel PrintheadTypeModel, GPIOPinListsViewModel GPIOPinListsViewModel)
        {
            _printheadTypeModel = PrintheadTypeModel;
            _gPIOPinListsViewModel = GPIOPinListsViewModel;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Resets the parameters of all GPIO pins.
        /// Typically, this method is called when the equipment is removed.
        /// </summary>
        public virtual void UnattachGPIOPins()
        {
            //Meant to be overriden.
        }
        #endregion
    }
}
