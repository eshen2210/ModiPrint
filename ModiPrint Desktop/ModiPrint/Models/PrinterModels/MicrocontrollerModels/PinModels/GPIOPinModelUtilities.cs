using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModiPrint.Models.PrinterModels.MicrocontrollerModels.PinModels
{
    public static class GPIOPinModelUtilities
    {
        /// <summary>
        /// Attaches a GPIOPinModel while resetting the previously attached Pin.
        /// </summary>
        /// <param name="attachingPinModel"></param>
        /// <param name="pinModelField"></param>
        /// <param name="pinSetting"></param>
        public static void AttachGPIOPinModel(GPIOPinModel attachingPinModel, ref GPIOPinModel pinModelField, string attachedEquipmentName, PinSetting pinSetting)
        {
            //Detach the current pin.
            if (pinModelField != null)
            {
                pinModelField.ResetPinParameters();
            }

            //Detach the new pin.
            if (attachingPinModel != null)
            {
                attachingPinModel.ResetPinParameters();
            }

            //Attach the new pin.
            if (attachingPinModel == null)
            {
                pinModelField = null;
            }
            else if (attachingPinModel.IsPinSettingPossible(pinSetting) == true)
            {
                pinModelField = attachingPinModel;
                pinModelField.SetPinParameters(pinSetting, attachedEquipmentName);
            }
        }

    }
}
