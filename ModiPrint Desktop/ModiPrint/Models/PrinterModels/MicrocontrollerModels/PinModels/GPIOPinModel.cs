using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModiPrint.Models.PrinterModels.MicrocontrollerModels.PinModels
{
    //Associated with the derivatives of the GPIOPinModel class.
    //States the function a GPIOPinMode is set to perform.
    public enum PinSetting
    {
        Unset,
        Input,
        Output,
        PWM,
        CommunicationIn,
        CommunicationOut,
        AnalogIn
    }

    public abstract class GPIOPinModel
    {
        #region Fields and Properties
        //The name of the GPIO Pin.
        private string _name;
        public string Name
        {
            get { return _name; }
        }

        //The pin number on the Arduino Mega.
        private int _pinID;
        public int PinID
        {
            get { return _pinID; }
        }

        //What kind of functionality does this Pin perform?
        //PinSetting is determined by what kind of equipment this Pin is attached to.
        private PinSetting _pinSetting = PinSetting.Unset;
        public PinSetting PinSetting
        {
            get { return _pinSetting; }
            private set
            {
                if (IsPinSettingPossible(value) == true)
                { _pinSetting = value; }
            }
        }

        //List containing all possible PinSetting values for this Pin.
        protected List<PinSetting> _possiblePinSettingList = new List<PinSetting>();
        public List<PinSetting> PossiblePinSettingList
        {
            get { return _possiblePinSettingList; }
        }

        //Equipment this Pin is attached to.
        private string _attachedEquipmentModelName;
        public string AttachedEquipmentModelName
        {
            get { return _attachedEquipmentModelName; }
        }

        //Is this Pin attached to an equipment?
        public bool IsAttached
        {
            get { return (!String.IsNullOrWhiteSpace(_attachedEquipmentModelName)) ? true : false; }
        }
        #endregion

        #region Constructor
        public GPIOPinModel(string Name, int PinID)
        {
            _name = Name;
            _pinID = PinID;

            //Populate PinSettingList.
            _possiblePinSettingList.Add(PinSetting.Unset);
            _possiblePinSettingList.Add(PinSetting.Input);
            _possiblePinSettingList.Add(PinSetting.Output);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Ensures that the value can be found in PinSettingList.
        /// </summary>
        /// <param name="pinSetting"></param>
        /// <returns></returns>
        public bool IsPinSettingPossible(PinSetting pinSetting)
        {
            foreach (PinSetting possiblePinSetting in _possiblePinSettingList)
            {
                if (possiblePinSetting == pinSetting)
                { return true; }
            }
            return false;
        }

        /// <summary>
        /// Resets properties back to default values.
        /// Typically used when the pin is detached.
        /// </summary>
        public void ResetPinParameters()
        {
            PinSetting = PinSetting.Unset;
            _attachedEquipmentModelName = "";
        }

        /// <summary>
        /// Sets new properties to the Pin.
        /// Typically used when the pin is attached.
        /// </summary>
        /// <param name="pinSetting"></param>
        /// <param name="equipmentName"></param>
        public void SetPinParameters(PinSetting pinSetting, string equipmentName)
        {
            PinSetting = pinSetting;
            _attachedEquipmentModelName = equipmentName;
        }

        /// <summary>
        /// If all parameters are set appropriately for a print, then return true.
        /// </summary>
        public virtual bool ReadyToPrint()
        {
            if (_pinSetting != PinSetting.Unset)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}