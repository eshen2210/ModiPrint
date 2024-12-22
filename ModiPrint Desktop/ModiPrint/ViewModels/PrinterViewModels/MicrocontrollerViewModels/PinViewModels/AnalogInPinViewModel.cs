using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ModiPrint.Models.PrinterModels.MicrocontrollerModels.PinModels;

namespace ModiPrint.ViewModels.PrinterViewModels.MicrocontrollerViewModels.PinViewModels
{
    /// <summary>
    /// Interface between the AnalogInPinModel and the GUI.
    /// </summary>
    public class AnalogInPinViewModel : GPIOPinViewModel
    {
        #region Fields and Properties
        private AnalogInPinModel _analogInPinModel;
        #endregion

        #region Constructor
        public AnalogInPinViewModel(AnalogInPinModel AnalogInPinModel) : base(AnalogInPinModel)
        {
            _analogInPinModel = AnalogInPinModel;
        }
        #endregion
    }
}
