using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ModiPrint.Models.PrinterModels.MicrocontrollerModels.PinModels;

namespace ModiPrint.ViewModels.PrinterViewModels.MicrocontrollerViewModels.PinViewModels
{
    /// <summary>
    /// Interface between the DigitalPinModel and the GUI.
    /// </summary>
    public class DigitalPinViewModel : GPIOPinViewModel
    {
        #region Fields and Properties
        private DigitalPinModel _digitalPinModel;
        #endregion

        #region Constructor
        public DigitalPinViewModel(DigitalPinModel DigitalPinModel) : base(DigitalPinModel)
        {
            _digitalPinModel = DigitalPinModel;
        }
        #endregion
    }
}
