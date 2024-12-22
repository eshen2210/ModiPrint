using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModiPrint.Models.PrinterModels.MicrocontrollerModels.PinModels;

namespace ModiPrint.ViewModels.PrinterViewModels.MicrocontrollerViewModels.PinViewModels
{
    public static class GPIOPinViewModelUtilities
    {
        /// <summary>
        /// Attaches a GPIOPinViewModel while resetting the previously attached Pin.
        /// </summary>
        /// <param name="attachingPinViewModel"></param>
        /// <param name="pinViewModelField"></param>
        /// <param name="pinSetting"></param>
        public static void AttachGPIOPinViewModel(GPIOPinViewModel attachingPinViewModel, ref GPIOPinViewModel pinViewModelField, PinSetting pinSetting, string attachingEquipmentName)
        {
            //Detach the previous pin.
            if (pinViewModelField != null)
            {
                pinViewModelField.ResetPinParameters();
            }

            //Attach the new pin.
            if (attachingPinViewModel == null)
            {
                pinViewModelField = null;
            }
            else if (attachingPinViewModel.IsPinSettingPossible(pinSetting))
            {
                pinViewModelField = attachingPinViewModel;
                pinViewModelField.UpdateProperties();
            }
        }
    }
}
