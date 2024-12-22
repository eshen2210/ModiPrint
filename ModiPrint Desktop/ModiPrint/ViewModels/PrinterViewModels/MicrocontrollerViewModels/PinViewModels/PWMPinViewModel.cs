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
    public class PWMPinViewModel : GPIOPinViewModel
    {
        #region Fields and Properties
        private PWMPinModel _pWMPinModel;
        #endregion

        #region Constructor
        public PWMPinViewModel(PWMPinModel PWMPinModel) : base(PWMPinModel)
        {
            _pWMPinModel = PWMPinModel;
        }
        #endregion
    }
}
