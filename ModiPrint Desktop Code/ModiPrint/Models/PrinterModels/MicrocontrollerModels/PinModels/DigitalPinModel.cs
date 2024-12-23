using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModiPrint.Models.PrinterModels.MicrocontrollerModels.PinModels
{
    /// <summary>
    /// Digital GPIO Pin.
    /// </summary>
    public class DigitalPinModel : GPIOPinModel
    {
        #region Constructor
        public DigitalPinModel(string Name, int PinID) : base(Name, PinID)
        {

        }
        #endregion
    }
}
