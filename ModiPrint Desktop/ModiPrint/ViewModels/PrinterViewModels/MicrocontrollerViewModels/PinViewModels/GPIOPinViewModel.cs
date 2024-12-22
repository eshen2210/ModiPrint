using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ModiPrint.DataTypes.GlobalValues;

using ModiPrint.Models.PrinterModels.MicrocontrollerModels.PinModels;

namespace ModiPrint.ViewModels.PrinterViewModels.MicrocontrollerViewModels.PinViewModels
{
    /// <summary>
    /// Interface between GPIOPinModel and the GUI.
    /// </summary>
    public abstract class GPIOPinViewModel : ViewModel
    {
        #region Fields and Properties
        private GPIOPinModel _gPIOPinModel;
        public GPIOPinModel GPIOPinModel
        {
            get { return _gPIOPinModel; }
        }

        //The name of the GPIO Pin.
        public string Name
        {
            get { return (_gPIOPinModel != null) ? _gPIOPinModel.Name : ""; }
        }

        //The pin number on the Arduino Mega.
        public int PinID
        {
            get { return (_gPIOPinModel != null) ? _gPIOPinModel.PinID : GlobalValues.PinIDNull; }
        }

        //What kind of functionality does this Pin perform?
        //PinSetting is determined by what kind of equipment this Pin is attached to.
        public PinSetting PinSetting
        {
            get { return (_gPIOPinModel != null) ? _gPIOPinModel.PinSetting : PinSetting.Unset; }
        }

        //Equipment this Pin is attached to.
        public string AttachedEquipmentViewModelName
        {
            get { return (_gPIOPinModel != null) ? _gPIOPinModel.AttachedEquipmentModelName : ""; }
        }

        //Is this Pin attached to an equipment?
        public bool IsAttached
        {
            get
            {
                if (_gPIOPinModel != null)
                {
                    return (!String.IsNullOrWhiteSpace(AttachedEquipmentViewModelName)) ? true : false;
                }
                else
                {
                    return false;
                }
            }
        }
        #endregion

        #region Constructor
        public GPIOPinViewModel(GPIOPinModel GPIOPinModel)
        {
            _gPIOPinModel = GPIOPinModel;
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
            return (_gPIOPinModel.IsPinSettingPossible(pinSetting)) ? true : false;
        }

        /// <summary>
        /// Resets properties back to default values.
        /// Typically used when the pin is detached.
        /// </summary>
        public void ResetPinParameters()
        {
            _gPIOPinModel.ResetPinParameters();

            OnPropertyChanged("AttachedEquipmentViewModelName");
            OnPropertyChanged("PinSetting");
        }

        /// <summary>
        /// Sets new properties to the Pin.
        /// Typically used when the pin is attached.
        /// </summary>
        /// <param name="pinSetting"></param>
        /// <param name="equipmentName"></param>
        public void SetPinParameters(PinSetting pinSetting, string equipmentName)
        {
            if (_gPIOPinModel != null)
            {
                _gPIOPinModel.SetPinParameters(pinSetting, equipmentName);
            }

            OnPropertyChanged("AttachedEquipmentViewModelName");
            OnPropertyChanged("PinSetting");
        }

        /// <summary>
        /// Calls OnPropertyChanged on all fields.
        /// </summary>
        public void UpdateProperties()
        {
            OnPropertyChanged("AttachedEquipmentViewModelName");
            OnPropertyChanged("PinSetting");
            OnPropertyChanged("Name");
            OnPropertyChanged("PinID");
        }
        #endregion
    }
}
