using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ModiPrint.Models.PrinterModels.MicrocontrollerModels.PinModels
{
    /// <summary>
    /// Communication GPIO Pin.
    /// </summary>
    public class CommunicationPinModel : GPIOPinModel
    {
        #region Constructor
        public CommunicationPinModel(string Name, int PinID) : base(Name, PinID)
        {
            base._possiblePinSettingList.Add(PinSetting.CommunicationIn);
            base._possiblePinSettingList.Add(PinSetting.CommunicationOut);
        }
        #endregion
    }
}
