using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ModiPrint.Models.PrinterModels.MicrocontrollerModels.PinModels;

namespace ModiPrint.ViewModels.PrinterViewModels.MicrocontrollerViewModels.PinViewModels
{
    /// <summary>
    /// Interface between the CommunicationPinModel and the GUI.
    /// </summary>
    public class CommunicationPinViewModel : GPIOPinViewModel
    {
        #region Fields and Properties
        private CommunicationPinModel _communicationPinModel;
        #endregion

        #region Constructor
        public CommunicationPinViewModel(CommunicationPinModel CommunicationPinModel) : base(CommunicationPinModel)
        {
            _communicationPinModel = CommunicationPinModel;
        }
        #endregion
    }
}
