using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using ModiPrint.Models.PrinterModels.PrintheadModels.PrintheadTypeModels;
using ModiPrint.ViewModels.PrinterViewModels.MicrocontrollerViewModels;
using ModiPrint.ViewModels.PrinterViewModels.MicrocontrollerViewModels.PinViewModels;

namespace ModiPrint.ViewModels.PrinterViewModels.PrintheadViewModels.PrintheadTypeViewModels
{
    /// <summary>
    /// Interface between CustomPrintheadTypeViewModel and the GUI.
    /// </summary>
    public class CustomPrintheadTypeViewModel : PrintheadTypeViewModel
    {
        #region Fields and Properties
        private CustomPrintheadTypeModel _customPrintheadTypeModel;
        public CustomPrintheadTypeModel CustomPrintheadTypeModel
        {
            get { return _customPrintheadTypeModel; }
        }
        #endregion

        #region Constructor
        public CustomPrintheadTypeViewModel(CustomPrintheadTypeModel CustomPrintheadTypeModel, GPIOPinListsViewModel GPIOPinListsViewModel) : base(CustomPrintheadTypeModel, GPIOPinListsViewModel)
        {
            _customPrintheadTypeModel = CustomPrintheadTypeModel;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Resets the parameters of all GPIO pins.
        /// Typically, this method is called when the equipment is removed.
        /// </summary>
        public override void UnattachGPIOPins()
        {

        }
        #endregion
    }
}
