using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ModiPrint.Models.PrinterModels.AxisModels;
using ModiPrint.Models.PrinterModels.MicrocontrollerModels.PinModels;
using ModiPrint.ViewModels.PrinterViewModels.MicrocontrollerViewModels;
using ModiPrint.ViewModels.PrinterViewModels.MicrocontrollerViewModels.PinViewModels;

namespace ModiPrint.ViewModels.PrinterViewModels.AxisViewModels
{
    //Associated with the AxisViewModel class.
    //States the status of the Axis during operation.
    public enum AxisStatus
    {
        Idle, //Default value. This Axis is not in use.
        BeingSet, //The command to set this Axis has been sent to the microcontroller.
        Active //The microcontroller is currently using this Printhead.
    }

    /// <summary>
    /// Interfaces AxisModel with the GUI.
    /// </summary>
    public class AxisViewModel : ViewModel
    {
        #region Fields and Properties
        //Model counterpart.
        private AxisModel _axisModel;
        public AxisModel AxisModel
        {
            get { return _axisModel; }
        }

        //Provides lists of GPIOPins sorted by function.
        private GPIOPinListsViewModel _gPIOPinListsViewModel;

        //Identifier used by ModiPrint and the GUI.
        public string Name
        {
            get { return _axisModel.Name; }
        }

        //Identifier used by the GCode.
        public char AxisID
        {
            get { return _axisModel.AxisID; }
        }

        //Pin that drives the Axis motor's step.
        private GPIOPinViewModel _attachedMotorStepGPIOPinViewModel;
        public GPIOPinViewModel AttachedMotorStepGPIOPinViewModel
        {
            get { return _attachedMotorStepGPIOPinViewModel; }
            set
            {
                GPIOPinModel newGPIOPinModel = (value != null) ? value.GPIOPinModel : null;
                _axisModel.AttachedMotorStepGPIOPinModel = newGPIOPinModel;
                GPIOPinViewModelUtilities.AttachGPIOPinViewModel (value, ref _attachedMotorStepGPIOPinViewModel, PinSetting.Output, Name + " Step");
                OnPropertyChanged("AttachedMotorStepGPIOPinViewModel");
            }
        }

        //Stores values that bind to this property's associated combobox.
        private ObservableCollection<GPIOPinViewModel> _motorStepPinViewModelList;
        public ObservableCollection<GPIOPinViewModel> MotorStepPinViewModelList
        {
            get { return _motorStepPinViewModelList; }
        }

        //Pin that signals the Axis motor's direction.
        private GPIOPinViewModel _attachedMotorDirectionGPIOPinViewModel;
        public GPIOPinViewModel AttachedMotorDirectionGPIOPinViewModel
        {
            get { return _attachedMotorDirectionGPIOPinViewModel; }
            set
            {
                GPIOPinModel newGPIOPinModel = (value != null) ? value.GPIOPinModel : null;
                _axisModel.AttachedMotorDirectionGPIOPinModel = newGPIOPinModel;
                GPIOPinViewModelUtilities.AttachGPIOPinViewModel(value, ref _attachedMotorDirectionGPIOPinViewModel, PinSetting.Output, Name + " Direction");
                OnPropertyChanged("AttachedMotorDirectionGPIOPinViewModel");
            }
        }

        //Stores values that bind to this property's associated combobox.
        private ObservableCollection<GPIOPinViewModel> _motorDirectionPinViewModelList;
        public ObservableCollection<GPIOPinViewModel> MotorDirectionPinViewModelList
        {
            get { return _motorDirectionPinViewModelList; }
        }

        //Pins that receive signal from the Limit Switches that limits the maximum and minimum range of the Axis.
        private GPIOPinViewModel _attachedLimitSwitchGPIOPinViewModel;
        public GPIOPinViewModel AttachedLimitSwitchGPIOPinViewModel
        {
            get { return _attachedLimitSwitchGPIOPinViewModel; }
            set
            {
                GPIOPinModel newGPIOPinModel = (value != null) ? value.GPIOPinModel : null;
                _axisModel.AttachedLimitSwitchGPIOPinModel = newGPIOPinModel;
                GPIOPinViewModelUtilities.AttachGPIOPinViewModel(value, ref _attachedLimitSwitchGPIOPinViewModel, PinSetting.Input, Name + " Limit");
                OnPropertyChanged("AttachedLimitSwitchGPIOPinViewModel");
            }
        }

        //Stores values that bind to this property's associated combobox.
        private ObservableCollection<GPIOPinViewModel> _limitSwitchPinViewModelList;
        public ObservableCollection<GPIOPinViewModel> LimitSwitchPinViewModelList
        {
            get { return _limitSwitchPinViewModelList; }
        }

        //Maximum speed of the Axis.
        //In milimeter per second.
        public double MaxSpeed
        {
            get { return _axisModel.MaxSpeed; }
            set
            {
                _axisModel.MaxSpeed = value;
                OnPropertyChanged("MaxSpeed");
            }
        }

        //Maximum acceleration of the Axis.
        //In milimeter per seconds squared.
        public double MaxAcceleration
        {
            get { return _axisModel.MaxAcceleration; }
            set
            {
                _axisModel.MaxAcceleration = value;
                OnPropertyChanged("MaxAcceleration");
            }
        }

        //Distance per step of the Axis.
        //In milimeters per step.
        public double MmPerStep
        {
            get { return _axisModel.MmPerStep; }
            set
            {
                _axisModel.MmPerStep = value;
                OnPropertyChanged("MmPerStep");
                OnPropertyChanged("UmPerStep");
            }
        }

        //Distance per step of the Axis.
        //In micrometers per step.
        public double UmPerStep
        {
            get { return _axisModel.MmPerStep * 1000; }
            set
            {
                _axisModel.MmPerStep = value / 1000;
                OnPropertyChanged("MmPerStep");
                OnPropertyChanged("UmPerStep");
            }
        }

        //Time in between step signal on and step signal off.
        //In microseconds.
        public int StepPulseTime
        {
            get { return _axisModel.StepPulseTime; }
            set
            {
                _axisModel.StepPulseTime = value;
                OnPropertyChanged("StepPulseTime");
            }
        }

        //Highest possible Axis position away from the origin.
        //In milimeters.
        public double MaxPosition
        {
            get { return _axisModel.MaxPosition; }
            set
            {
                _axisModel.MaxPosition = value;
                OnPropertyChanged("MaxPosition");
            }
        }

        //Highest possible distance away from the origin in the negative direction.
        //In milimeters.
        public double MinPosition
        {
            get { return _axisModel.MinPosition; }
            set
            {
                _axisModel.MinPosition = value;
                OnPropertyChanged("MinPosition");
            }
        }

        //Image that represents this Axis.
        public string ImageSource
        {
            get { return (IsRemovable == true) ? "/Resources/General/ZAxis.png" : "/Resources/General/Axis.png"; }
        }

        //States the current status of the Axis during operation.
        private AxisStatus _axisStatus = AxisStatus.Idle;
        public AxisStatus AxisStatus
        {
            get { return _axisStatus; }
            set
            {
                _axisStatus = value;
                OnPropertyChanged("AxisStatus");
            }
        }

        //If true, multiply all position values of this Axis by -1 during operation.
        public bool IsDirectionInverted
        {
            get { return _axisModel.IsDirectionInverted; }
            set { _axisModel.IsDirectionInverted = value; }
        }

        //Is this Axis removable from the printer?
        //Return true if it is a Z Axis.
        public bool IsRemovable
        {
            get { return _axisModel.IsRemovable; }
        }
        #endregion

        #region Constructors
        public AxisViewModel(AxisModel AxisModel, GPIOPinListsViewModel GPIOPinListsViewModel)
        {
            _axisModel = AxisModel;
            _gPIOPinListsViewModel = GPIOPinListsViewModel;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Resets the parameters of all GPIO pins.
        /// Typically, this method is called when the equipment is removed.
        /// </summary>
        public void UnattachGPIOPins()
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
            return true;
        }

        public void ExecuteRefreshPinBySettingListCommand(string propertyName)
        {
            switch(propertyName)
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
        /// Unattaches a GPIO Pin from this Axis.
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
