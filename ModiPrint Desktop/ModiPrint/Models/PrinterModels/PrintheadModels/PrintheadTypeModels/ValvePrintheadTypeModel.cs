using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModiPrint.Models.PrinterModels.MicrocontrollerModels.PinModels;
using ModiPrint.Models.PrintModels.PrintStyleModels;

namespace ModiPrint.Models.PrinterModels.PrintheadModels.PrintheadTypeModels
{
    /// <summary>
    /// Extends PrintheadModel with properties and functions related the operation of valves.
    /// </summary>
    public class ValvePrintheadTypeModel : PrintheadTypeModel
    {
        #region Fields and Properties
        //GPIOPin that signals the valve on this Printhead.
        private GPIOPinModel _attachedValveGPIOPinModel;
        public GPIOPinModel AttachedValveGPIOPinModel
        {
            get { return _attachedValveGPIOPinModel; }
            set
            {
                GPIOPinModelUtilities.AttachGPIOPinModel(value, ref _attachedValveGPIOPinModel, AttachedName + " Valve", PinSetting.Output);
            }
        }
        #endregion

        #region Constructor
        public ValvePrintheadTypeModel(string AttachedName) : base(AttachedName)
        {
            base._possiblePrintStylesList.Add(PrintStyle.Continuous);
            base._possiblePrintStylesList.Add(PrintStyle.Droplet);
        }
        #endregion

        #region Methods
        /// <summary>
        /// If all parameters are set appropriately for a print, then return true.
        /// </summary>
        /// <returns></returns>
        public override bool ReadyToPrint()
        {
            if (_attachedValveGPIOPinModel != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Resets the parameters of all GPIO pins.
        /// Typically, this method is called when the equipment is removed.
        /// </summary>
        public override void UnattachGPIOPins()
        {
            if (_attachedValveGPIOPinModel != null)
            { _attachedValveGPIOPinModel.ResetPinParameters(); }
        }
        #endregion
    }
}
