using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ModiPrint.Models.PrinterModels.MicrocontrollerModels.PinModels;
using ModiPrint.Models.PrinterModels.PrintheadModels.PrintheadTypeModels;
using ModiPrint.ViewModels.PrinterViewModels.MicrocontrollerViewModels;
using ModiPrint.ViewModels.PrinterViewModels.MicrocontrollerViewModels.PinViewModels;

namespace ModiPrint.ViewModels.PrinterViewModels.PrintheadViewModels.PrintheadTypeViewModels
{
    /// <summary>
    /// Interface between MotorizedPrintheadTypeModel and the GUI.
    /// </summary>
    public class MotorizedPrintheadTypeViewModel : PrintheadTypeViewModel
    {
        #region Fields and Properties
        private MotorizedPrintheadTypeModel _motorizedPrintheadTypeModel;
        public MotorizedPrintheadTypeModel MotorizedPrintheadTypeModel
        {
            get { return _motorizedPrintheadTypeModel; }
        }

        //GPIOPin that signals the step of this Printhead's motor.
        private GPIOPinViewModel _attachedMotorStepGPIOPinViewModel;
        public GPIOPinViewModel AttachedMotorStepGPIOPinViewModel
        {
            get { return _attachedMotorStepGPIOPinViewModel; }
            set
            {
                GPIOPinModel newGPIOPinModel = (value != null) ? value.GPIOPinModel : null;
                _motorizedPrintheadTypeModel.AttachedMotorStepGPIOPinModel = newGPIOPinModel;
                GPIOPinViewModelUtilities.AttachGPIOPinViewModel(value, ref _attachedMotorStepGPIOPinViewModel, PinSetting.Output, base.AttachedName + " Step");
                OnPropertyChanged("AttachedMotorStepGPIOPinViewModel");
            }
        }

        //Stores values that bind to this property's associated combobox.
        private ObservableCollection<GPIOPinViewModel> _motorStepPinViewModelList;
        public ObservableCollection<GPIOPinViewModel> MotorStepPinViewModelList
        {
            get { return _motorStepPinViewModelList; }
        }

        //GPIOPin that signals the direction of this Printhead's motor.
        private GPIOPinViewModel _attachedMotorDirectionGPIOPinViewModel;
        public GPIOPinViewModel AttachedMotorDirectionGPIOPinViewModel
        {
            get { return _attachedMotorDirectionGPIOPinViewModel; }
            set
            {
                GPIOPinModel newGPIOPinModel = (value != null) ? value.GPIOPinModel : null;
                _motorizedPrintheadTypeModel.AttachedMotorDirectionGPIOPinModel = newGPIOPinModel;
                GPIOPinViewModelUtilities.AttachGPIOPinViewModel(value, ref _attachedMotorDirectionGPIOPinViewModel, PinSetting.Output, base.AttachedName + " Direction");
                OnPropertyChanged("AttachedMotorDirectionGPIOPinViewModel");
            }
        }

        //Stores values that bind to this property's associated combobox.
        private ObservableCollection<GPIOPinViewModel> _motorDirectionPinViewModelList;
        public ObservableCollection<GPIOPinViewModel> MotorDirectionPinViewModelList
        {
            get { return _motorDirectionPinViewModelList; }
        }

        //GPIOPin for this Printhead motor's limit switch,
        private GPIOPinViewModel _attachedLimitSwitchGPIOPinViewModel;
        public GPIOPinViewModel AttachedLimitSwitchGPIOPinViewModel
        {
            get { return _attachedLimitSwitchGPIOPinViewModel; }
            set
            {
                GPIOPinModel newGPIOPinModel = (value != null) ? value.GPIOPinModel : null;
                _motorizedPrintheadTypeModel.AttachedLimitSwitchGPIOPinModel = newGPIOPinModel;
                GPIOPinViewModelUtilities.AttachGPIOPinViewModel(value, ref _attachedLimitSwitchGPIOPinViewModel, PinSetting.Input, base.AttachedName + " Limit");
                OnPropertyChanged("AttachedLimitSwitchGPIOPinViewModel");
            }
        }

        //Stores values that bind to this property's associated combobox.
        private ObservableCollection<GPIOPinViewModel> _limitSwitchPinViewModelList;
        public ObservableCollection<GPIOPinViewModel> LimitSwitchPinViewModelList
        {
            get { return _limitSwitchPinViewModelList; }
        }

        //Maximum speed of the motor of this Printhead.
        //In milimeter per second.
        public double MaxSpeed
        {
            get { return _motorizedPrintheadTypeModel.MaxSpeed; }
            set
            {
                _motorizedPrintheadTypeModel.MaxSpeed = value;
                OnPropertyChanged("MaxSpeed");
            }
        }

        //Maximum acceleration of the motor of this Printhead.
        //In milimeter per seconds squared.
        public double MaxAcceleration
        {
            get { return _motorizedPrintheadTypeModel.MaxAcceleration; }
            set
            {
                _motorizedPrintheadTypeModel.MaxAcceleration = value;
                OnPropertyChanged("MaxAcceleration");
            }
        }

        //Distance per step of the motor of this Printhead.
        //In milimeters per step.
        public double MmPerStep
        {
            get { return _motorizedPrintheadTypeModel.MmPerStep; }
            set
            {
                _motorizedPrintheadTypeModel.MmPerStep = value;
                OnPropertyChanged("MmPerStep");
                OnPropertyChanged("UmPerStep");
            }
        }

        //Distance per step of the motor of this Printhead.
        //In micrometers per step.
        public double UmPerStep
        {
            get { return _motorizedPrintheadTypeModel.MmPerStep * 1000; }
            set
            {
                _motorizedPrintheadTypeModel.MmPerStep = value / 1000;
                OnPropertyChanged("MmPerStep");
                OnPropertyChanged("UmPerStep");
            }
        }

        //Time in between step signal on and step signal off.
        //In microseconds.
        public int StepPulseTime
        {
            get { return _motorizedPrintheadTypeModel.StepPulseTime; }
            set
            {
                _motorizedPrintheadTypeModel.StepPulseTime = value;
                OnPropertyChanged("StepPulseTime");
            }
        }

        //Highest possible distance away from the origin.
        //In milimeters.
        public double MaxPosition
        {
            get { return _motorizedPrintheadTypeModel.MaxPosition; }
            set
            {
                _motorizedPrintheadTypeModel.MaxPosition = value;
                OnPropertyChanged("MaxPosition");
            }
        }

        //Highest possible distance away from the origin in the negative direction.
        //In milimeters.
        public double MinPosition
        {
            get { return _motorizedPrintheadTypeModel.MinPosition; }
            set
            {
                _motorizedPrintheadTypeModel.MinPosition = value;
                OnPropertyChanged("MinPosition");
            }
        }

        //If true, multiply all position values of this Axis by -1 during operation.
        public bool IsDirectionInverted
        {
            get { return _motorizedPrintheadTypeModel.IsDirectionInverted; }
            set { _motorizedPrintheadTypeModel.IsDirectionInverted = value; }
        }
        #endregion

        #region Constructor
        public MotorizedPrintheadTypeViewModel(MotorizedPrintheadTypeModel MotorizedPrintheadTypeModel, GPIOPinListsViewModel GPIOPinListsViewModel) : base(MotorizedPrintheadTypeModel, GPIOPinListsViewModel)
        {
            _motorizedPrintheadTypeModel = MotorizedPrintheadTypeModel;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Resets the parameters of all GPIO pins.
        /// Typically, this method is called when the equipment is removed.
        /// </summary>
        public override void UnattachGPIOPins()
        {
            if (_attachedMotorStepGPIOPinViewModel != null)
            { _attachedMotorStepGPIOPinViewModel.ResetPinParameters(); }
            if (_attachedMotorDirectionGPIOPinViewModel != null)
            { _attachedMotorDirectionGPIOPinViewModel.ResetPinParameters(); }
            if (_attachedLimitSwitchGPIOPinViewModel != null)
            { _attachedLimitSwitchGPIOPinViewModel.ResetPinParameters(); }
        }

        /// <summary>
        /// Calls OnPropertyChanged on the Min and Max Position properties.
        /// Typically called during automated calibration.
        /// </summary>
        public void UpdateMinMaxPositions()
        {
            OnPropertyChanged("MinPosition");
            OnPropertyChanged("MaxPosition");
        }
        #endregion

        #region Commands
        /// <summary>
        /// Updates UnattachedPinsByPinSetting with specified PinSetting.
        /// </summary>
        private RelayCommand<string> _refreshPinBySettingListCommand;
        public ICommand RefreshPinBySettingListCommand
        {
            get
            {
                if (_refreshPinBySettingListCommand == null)
                { _refreshPinBySettingListCommand = new RelayCommand<string>(ExecuteRefreshPinBySettingListCommand, CanExecuteRefreshPinBySettingListCommand); }
                return _refreshPinBySettingListCommand;
            }
        }

        public bool CanExecuteRefreshPinBySettingListCommand(string propertyName)
        {
            return (!String.IsNullOrWhiteSpace(propertyName)) ? true : false;
        }

        public void ExecuteRefreshPinBySettingListCommand(string propertyName)
        {
            switch (propertyName)
            {
                case "MotorStepPinViewModelList":
                    _gPIOPinListsViewModel.RefreshGPIOPinViewModelList(ref _attachedMotorStepGPIOPinViewModel, ref _motorStepPinViewModelList, PinSetting.Output);
                    break;
                case "MotorDirectionPinViewModelList":
                    _gPIOPinListsViewModel.RefreshGPIOPinViewModelList(ref _attachedMotorDirectionGPIOPinViewModel, ref _motorDirectionPinViewModelList, PinSetting.Output);
                    break;
                case "LimitSwitchPinViewModelList":
                    _gPIOPinListsViewModel.RefreshGPIOPinViewModelList(ref _attachedLimitSwitchGPIOPinViewModel, ref _limitSwitchPinViewModelList, PinSetting.Input);
                    break;
                default:
                    break;
            }
            OnPropertyChanged(propertyName);
        }

                /// <summary>
        /// Unattaches a GPIO Pin from this Printhead.
        /// </summary>
        private RelayCommand<string> _clearGPIOPinCommand;
        public ICommand ClearGPIOPinCommand
        {
            get
            {
                if (_clearGPIOPinCommand == null)
                { _clearGPIOPinCommand = new RelayCommand<string>(ExecuteClearGPIOPinCommand, CanExecuteClearGPIOPinCommand); }
                return _clearGPIOPinCommand;
            }
        }

        public bool CanExecuteClearGPIOPinCommand(string propertyName)
        {
            return true;
        }

        public void ExecuteClearGPIOPinCommand(string propertyName)
        {
            switch (propertyName)
            {
                case "AttachedMotorStepGPIOPinViewModel":
                    AttachedMotorStepGPIOPinViewModel = null;
                    _gPIOPinListsViewModel.RefreshGPIOPinViewModelList(ref _attachedMotorStepGPIOPinViewModel, ref _motorStepPinViewModelList, PinSetting.Output);
                    break;
                case "AttachedMotorDirectionGPIOPinViewModel":
                    AttachedMotorDirectionGPIOPinViewModel = null;
                    _gPIOPinListsViewModel.RefreshGPIOPinViewModelList(ref _attachedMotorDirectionGPIOPinViewModel, ref _motorDirectionPinViewModelList, PinSetting.Output);
                    break;
                case "AttachedLimitSwitchGPIOPinViewModel":
                    AttachedLimitSwitchGPIOPinViewModel = null;
                    _gPIOPinListsViewModel.RefreshGPIOPinViewModelList(ref _attachedLimitSwitchGPIOPinViewModel, ref _limitSwitchPinViewModelList, PinSetting.Input);
                    break;
                default:
                    break;
            }
            OnPropertyChanged(propertyName);
        }
        #endregion
    }
}
