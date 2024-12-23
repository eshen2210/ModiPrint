using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ModiPrint.Models.PrinterModels.MicrocontrollerModels.PinModels
{
    /// <summary>
    /// AnalogIn GPIO Pin.
    /// </summary>
    public class AnalogInPinModel : GPIOPinModel
    {
        #region Constructor
        public AnalogInPinModel(string Name, int PinID) : base(Name, PinID)
        {
            base._possiblePinSettingList.Add(PinSetting.AnalogIn);
        }
        #endregion
    }
}
