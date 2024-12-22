using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ModiPrint.Models.PrinterModels.MicrocontrollerModels.PinModels
{
    /// <summary>
    /// PWM GPIO Pin.
    /// </summary>
    public class PWMPinModel : GPIOPinModel
    {
        #region Constructor
        public PWMPinModel(string Name, int PinID) : base(Name, PinID)
        {
            base._possiblePinSettingList.Add(PinSetting.PWM);
        }
        #endregion
    }
}
