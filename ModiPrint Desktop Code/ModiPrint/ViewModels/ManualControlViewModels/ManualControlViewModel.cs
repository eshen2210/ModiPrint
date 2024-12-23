using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.ObjectModel;
using ModiPrint.Models.ManualControlModels;
using ModiPrint.Models.PrinterModels.PrintheadModels.PrintheadTypeModels;
using ModiPrint.Models.RealTimeStatusModels;
using ModiPrint.ViewModels.PrinterViewModels.AxisViewModels;
using ModiPrint.ViewModels.PrinterViewModels.PrintheadViewModels;
using ModiPrint.ViewModels.PrinterViewModels.PrintheadViewModels.PrintheadTypeViewModels;
using ModiPrint.ViewModels.RealTimeStatusViewModels;
using ModiPrint.ViewModels.PrinterViewModels;
using ModiPrint.ViewModels.ManualControlViewModels.AxesPrintStyles;

namespace ModiPrint.ViewModels.ManualControlViewModels
{
    public class ManualControlViewModel : ViewModel
    {
        #region Fields and Properties
        //Contains functions that send commands through the serial port.
        ManualControlModel _manualControlModel;

        //Displays information on the Printer's parameters during operations.
        RealTimeStatusDataViewModel _realTimeStatusDataViewModel;

        //Contains Printer parameters.
        PrinterViewModel _printerViewModel;

        //Indicates what is displayed on the second page menu after a button is clicked.
        //This second page will have additional options to set.
        private string _menu = "Base";
        public string Menu
        {
            get { return _menu; }
            set
            {
                _menu = value;
                OnPropertyChanged("Menu");
            }
        }

        //Indicates the PrintStyle that commands will execute.
        //Can be set to "Movement Only", "Continuous Print", or "Droplet Print".
        private string _axesPrintStyle = "Movement Only";
        public string AxesPrintStyle
        {
            get { return _axesPrintStyle; }
            set
            {
                _axesPrintStyle = value;
                OnPropertyChanged("AxesPrintStyle");
            }
        }

        //Used to select the AxesPrintStyle.
        private ObservableCollection<AxesPrintStyle> _axesPrintStylesList = new ObservableCollection<AxesPrintStyle>();
        public ObservableCollection<AxesPrintStyle> AxesPrintStylesList
        {
            get { return _axesPrintStylesList; }
        }

        //Used to set the move distance parameters for any manual move axes or motorized print command.
        //Enforces negative or positive values depending on the command being executed.
        //In units of mm.
        private double _xMoveAxisPositiveDistance = 0;
        public double XMoveAxisPositiveDistance
        {
            get { return _xMoveAxisPositiveDistance; }
            set { _xMoveAxisPositiveDistance = (value >= 0) ? value : (-1 * value); }
        }

        private double _xMoveAxisNegativeDistance = 0;
        public double XMoveAxisNegativeDistance
        {
            get { return _xMoveAxisNegativeDistance; }
            set { _xMoveAxisNegativeDistance = (value <= 0) ? value : (-1 * value); }
        }

        private double _yMoveAxisPositiveDistance = 0;
        public double YMoveAxisPositiveDistance
        {
            get { return _yMoveAxisPositiveDistance; }
            set { _yMoveAxisPositiveDistance = (value >= 0) ? value : (-1 * value); }
        }

        private double _yMoveAxisNegativeDistance = 0;
        public double YMoveAxisNegativeDistance
        {
            get { return _yMoveAxisNegativeDistance; }
            set { _yMoveAxisNegativeDistance = (value <= 0) ? value : (-1 * value); }
        }

        private double _zMoveAxisPositiveDistance = 0;
        public double ZMoveAxisPositiveDistance
        {
            get { return _zMoveAxisPositiveDistance; }
            set { _zMoveAxisPositiveDistance = (value >= 0) ? value : (-1 * value); }
        }

        private double _zMoveAxisNegativeDistance = 0;
        public double ZMoveAxisNegativeDistance
        {
            get { return _zMoveAxisNegativeDistance; }
            set { _zMoveAxisNegativeDistance = (value <= 0) ? value : (-1 * value); }
        }

        private double _eMoveAxisPositiveDistance = 0;
        public double EMoveAxisPositiveDistance
        {
            get { return _eMoveAxisPositiveDistance; }
            set { _eMoveAxisPositiveDistance = (value >= 0) ? value : (-1 * value); }
        }

        private double _eMoveAxisNegativeDistance = 0;
        public double EMoveAxisNegativeDistance
        {
            get { return _eMoveAxisNegativeDistance; }
            set { _eMoveAxisNegativeDistance = (value <= 0) ? value : (-1 * value); }
        }

        //Name of the Axis being set.
        private string _axisName;
        public string AxisName
        {
            get { return _axisName; }
            set
            {
                AxisViewModel axisViewModel = _printerViewModel.FindAxis(value);
                if (axisViewModel != null)
                {
                    _menu = "SetAxis";
                }
                _axisName = value;
                OnPropertyChanged("Menu");
                OnPropertyChanged("AxisName");
            }
        }

        //Name of Printhead being set.
        private string _printheadName;
        public string PrintheadName
        {
            get { return _printheadName; }
            set
            {
                if (value == "")
                {
                    _menu = "Base";
                }
                else
                {
                    PrintheadViewModel printheadViewModel = _printerViewModel.FindPrinthead(value);
                    if (printheadViewModel != null)
                    {
                        if (printheadViewModel.PrintheadType == PrintheadType.Motorized)
                        {
                            _menu = "SetMotorized";
                        }
                        else if (printheadViewModel.PrintheadType == PrintheadType.Valve)
                        {
                            _menu = "SetValve";
                        }
                    }
                }
                _printheadName = value;
                OnPropertyChanged("Menu");
                OnPropertyChanged("PrintheadName");
            }
        }

        //Reference to the Printhead that was last set.
        public PrintheadViewModel ActivePrintheadViewModel
        {
            get { return _printerViewModel.FindPrinthead(_printheadName); }
        }

        //Printhead type of currently active Printhead.
        public PrintheadType ActivePrintheadType
        {
            get { return _realTimeStatusDataViewModel.ActivePrintheadType; }
        }

        //Parameters to set Axes and Motorized Printheads.

        //If true, then set the max speed and acceleration values to their max possible.
        private bool _maximizeSpeeds = false;
        public bool MaximizeSpeeds
        {
            get { return _maximizeSpeeds; }
            set
            {
                if (value == true)
                {
                    if (_menu == "SetAxis")
                    {
                        _maxSpeed = _printerViewModel.FindAxis(_axisName).MaxSpeed;
                        _acceleration = _printerViewModel.FindAxis(_axisName).MaxAcceleration;
                    }
                    else if (_menu == "SetMotorized")
                    {
                        PrintheadViewModel printheadViewModel = _printerViewModel.FindPrinthead(_printheadName);
                        if (printheadViewModel.PrintheadType == PrintheadType.Motorized)
                        {
                            MotorizedPrintheadTypeViewModel motorizedPrintheadTypeViewModel = (MotorizedPrintheadTypeViewModel)printheadViewModel.PrintheadTypeViewModel;
                            _maxSpeed = motorizedPrintheadTypeViewModel.MaxSpeed;
                            _acceleration = motorizedPrintheadTypeViewModel.MaxAcceleration;
                        } 
                    }
                }

                _maximizeSpeeds = value;
                OnPropertyChanged("MaxSpeed");
                OnPropertyChanged("Acceleration");
                OnPropertyChanged("MaximizeSpeeds");
            }
        }

        //Max speed of the motor.
        //In mm/s.
        private double _maxSpeed = 0;
        public double MaxSpeed
        {
            get { return _maxSpeed; }
            set { _maxSpeed = (value >= 0) ? value : _maxSpeed; }
        }

        //Acceleration of the motor.
        //In mm/s2.
        private double _acceleration = 0;
        public double Acceleration
        {
            get { return _acceleration; }
            set { _acceleration = (value >= 0) ? value : _acceleration; }
        }

        //Parameters used in Printing.

        //Used to determine the speed of the Motor Printhead relative to the XYZ Axes.
        //In mm per mm.
        private double _eDispensePerDistance = 0;
        public double EDispensePerDistance
        {
            get { return _eDispensePerDistance; }
            set { _eDispensePerDistance = value; }
        }

        //Used to set the open time parameter for any open valve command.
        //In units of us.
        private int _valveOpenTime = 0;
        public int ValveOpenTime
        {
            get { return _valveOpenTime; }
            set { _valveOpenTime = (value >= 0) ? value : _valveOpenTime; }
        }

        //Used to set the open time parameter for any open valve command.
        //In units of ms.
        private int _valveOpenTimeMs = 0;
        public int ValveOpenTimeMs
        {
            get { return _valveOpenTime / 1000; }
            set { ValveOpenTime = (value * 1000); }
        }

        //Parameters for droplet printing.

        //Distance between each droplet.
        //In mm.
        private double _interpolateDistance = 0;
        public double InterpolateDistance
        {
            get { return _interpolateDistance; }
            set { _interpolateDistance = (value >= 0) ? value : _interpolateDistance; }
        }
        #endregion

        #region Constructor
        public ManualControlViewModel(ManualControlModel ManualControlModel, RealTimeStatusDataViewModel RealTimeStatusDataViewModel, PrinterViewModel PrinterViewModel)
        {
            _manualControlModel = ManualControlModel;
            _realTimeStatusDataViewModel = RealTimeStatusDataViewModel;
            _printerViewModel = PrinterViewModel;

            _axesPrintStylesList.Add(new MovementOnlyStyle());
            _axesPrintStylesList.Add(new DropletPrintStyle());
            _axesPrintStylesList.Add(new ContinuousPrintStyle());

            _realTimeStatusDataViewModel.RealTimeStatusDataModel.RecordSetAxisExecuted += new RecordSetAxisExecutedEventHandler(UpdateSetAxis);
            _realTimeStatusDataViewModel.RealTimeStatusDataModel.RecordSetMotorizedPrintheadExecuted += new RecordSetMotorizedPrintheadExecutedEventHandler(UpdateSetPrinthead);
            _realTimeStatusDataViewModel.RealTimeStatusDataModel.RecordSetValvePrintheadExecuted += new RecordSetValvePrintheadExecutedEventHandler(UpdateSetPrinthead);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Interprets a movement type string and returns XYZ movement distances.
        /// Resets parameters after retrieving them.
        /// </summary>
        /// <param name="movementType"></param>
        /// <param name="xDistance"></param>
        /// <param name="yDistance"></param>
        /// <param name="zDistance"></param>
        private void ParseMovement(ref double xDistance, ref double yDistance, ref double zDistance)
        {
            xDistance = (_xMoveAxisPositiveDistance != 0) ? _xMoveAxisPositiveDistance : _xMoveAxisNegativeDistance;
            yDistance = (_yMoveAxisPositiveDistance != 0) ? _yMoveAxisPositiveDistance : _yMoveAxisNegativeDistance;
            zDistance = (_zMoveAxisPositiveDistance != 0) ? _zMoveAxisPositiveDistance : _zMoveAxisNegativeDistance;

            //Reset parameters.
            _xMoveAxisPositiveDistance = 0;
            _xMoveAxisNegativeDistance = 0;
            _yMoveAxisPositiveDistance = 0;
            _yMoveAxisNegativeDistance = 0;
            _zMoveAxisPositiveDistance = 0;
            _zMoveAxisNegativeDistance = 0;
            OnPropertyChanged("XMoveAxisPositiveDistance");
            OnPropertyChanged("XMoveAxisNegativeDistance");
            OnPropertyChanged("YMoveAxisPositiveDistance");
            OnPropertyChanged("YMoveAxisNegativeDistance");
            OnPropertyChanged("ZMoveAxisPositiveDistance");
            OnPropertyChanged("ZMoveAxisNegativeDistance");
        }

        /// <summary>
        /// Updates the active Axis' name if a new Z Axis was set.
        /// Called when the microcontroller finishes setting an Axis.
        /// </summary>
        /// <param name="axisName"></param>
        private void UpdateSetAxis(string axisName)
        {
            if (axisName[0] == 'Z')
            {
                _axisName = axisName;
                OnPropertyChanged("AxisName");
            }
        }

        /// <summary>
        /// Updates the active Printhead's name.
        /// Called when the microcontroller finishes setting a Printhead.
        /// </summary>
        /// <param name="printheadName"></param>
        private void UpdateSetPrinthead(string printheadName)
        {
            _printheadName = printheadName;
            OnPropertyChanged("PrintheadName");
        }
        #endregion

        #region Commands
        /// <summary>
        /// Sets the menu value which determines the second screen that will appear when a manual control button is clicked.
        /// </summary>
        private RelayCommand<string> _setMenuCommand;
        public ICommand SetMenuCommand
        {
            get
            {
                if (_setMenuCommand == null)
                { _setMenuCommand = new RelayCommand<string>(ExecuteSetMenuCommand, CanExecuteSetMenuCommand); }
                return _setMenuCommand;
            }
        }

        public bool CanExecuteSetMenuCommand(string notUsed)
        {
            return true;
        }

        public void ExecuteSetMenuCommand(string parameters)
        {
            //If reverting the menu, then reset all parameters.
            if (parameters == "Base")
            {
                _xMoveAxisPositiveDistance = 0;
                _xMoveAxisNegativeDistance = 0;
                _yMoveAxisPositiveDistance = 0;
                _yMoveAxisNegativeDistance = 0;
                _zMoveAxisPositiveDistance = 0;
                _zMoveAxisNegativeDistance = 0;
                _eMoveAxisPositiveDistance = 0;
                _eMoveAxisNegativeDistance = 0;
                _maximizeSpeeds = false;
                _maxSpeed = 0;
                _acceleration = 0;
                _eDispensePerDistance = 0;
                _valveOpenTime = 0;
                _interpolateDistance = 0;
            }

            _menu = parameters;
            OnPropertyChanged("Menu");
        }

        /// <summary>
        /// Sends a movement command through the serial port.
        /// </summary>
        private RelayCommand<object> _manualMovementCommand;
        public ICommand ManualMovementCommand
        {
            get
            {
                if (_manualMovementCommand == null)
                { _manualMovementCommand = new RelayCommand<object>(ExecuteSetManualMovementCommand, CanExecuteManualMovementCommand); }
                return _manualMovementCommand;
            }
        }

        public bool CanExecuteManualMovementCommand(object notUsed)
        {
            return true;
        }

        public void ExecuteSetManualMovementCommand(object notUsed)
        {
            double xDistance = 0;
            double yDistance = 0;
            double zDistance = 0;

            ParseMovement(ref xDistance, ref yDistance, ref zDistance);

            _manualControlModel.ProcessManualMovementCommand(xDistance, yDistance, zDistance);

            _menu = "Base";
            OnPropertyChanged("Menu");
        }

        /// <summary>
        /// Sends a motor print with movement command through the serial port.
        /// </summary>
        private RelayCommand<object> _manualMotorPrintWithMovementCommand;
        public ICommand ManualMotorPrintWithMovementCommand
        {
            get
            {
                if (_manualMotorPrintWithMovementCommand == null)
                { _manualMotorPrintWithMovementCommand = new RelayCommand<object>(ExecuteManualMotorPrintWithMovementCommand, CanExecuteManualMotorPrintWithMovementCommand); }
                return _manualMotorPrintWithMovementCommand;
            }
        }

        public bool CanExecuteManualMotorPrintWithMovementCommand(object notUsed)
        {
            return true;
        }

        public void ExecuteManualMotorPrintWithMovementCommand(object notUsed)
        {
            double xDistance = 0;
            double yDistance = 0;
            double zDistance = 0;

            ParseMovement(ref xDistance, ref yDistance, ref zDistance);

            _manualControlModel.ProcessManualMotorPrintWithMovementCommand(xDistance, yDistance, zDistance, _eDispensePerDistance);

            _eDispensePerDistance = 0;
            OnPropertyChanged("EDispensePerDistance");

            _menu = "Base";
            OnPropertyChanged("Menu");
        }

        /// <summary>
        /// Sends a motor print without movement command through the serial port.
        /// </summary>
        private RelayCommand<object> _manualMotorPrintWithoutMovementCommand;
        public ICommand ManualMotorPrintWithoutMovementCommand
        {
            get
            {
                if (_manualMotorPrintWithoutMovementCommand == null)
                { _manualMotorPrintWithoutMovementCommand = new RelayCommand<object>(ExecuteManualMotorPrintWithoutMovementCommand, CanExecuteManualMotorPrintWithoutMovementCommand); }
                return _manualMotorPrintWithoutMovementCommand;
            }
        }

        public bool CanExecuteManualMotorPrintWithoutMovementCommand(object notUsed)
        {
            return true;
        }

        public void ExecuteManualMotorPrintWithoutMovementCommand(object notUsed)
        {
            double eDistance = (_eMoveAxisPositiveDistance != 0) ? _eMoveAxisPositiveDistance : _eMoveAxisNegativeDistance;
            _manualControlModel.ProcessManualMotorPrintWithoutMovementCommand(eDistance);

            _eMoveAxisPositiveDistance = 0;
            _eMoveAxisNegativeDistance = 0;
            OnPropertyChanged("EMoveAxisNegativeDirection");
            OnPropertyChanged("EMoveAxisPositiveDistance");

            _menu = "Base";
            OnPropertyChanged("Menu");
        }

        /// <summary>
        /// Sends a set of commands for motor droplet print through the serial port.
        /// </summary>
        private RelayCommand<object> _manualMotorDropletPrintCommand;
        public ICommand ManualMotorDropletPrintCommand
        {
            get
            {
                if (_manualMotorDropletPrintCommand == null)
                { _manualMotorDropletPrintCommand = new RelayCommand<object>(ExecuteManualMotorDropletPrintCommand, CanExecuteManualMotorDropletPrintCommand); }
                return _manualMotorDropletPrintCommand;
            }
        }

        public bool CanExecuteManualMotorDropletPrintCommand(object notUsed)
        {
            return true;
        }

        public void ExecuteManualMotorDropletPrintCommand(object notUsed)
        {
            double xDistance = 0;
            double yDistance = 0;
            double zDistance = 0;

            ParseMovement(ref xDistance, ref yDistance, ref zDistance);

            _manualControlModel.ProcessManualMotorDropletPrintCommand(xDistance, yDistance, zDistance, _interpolateDistance, _eMoveAxisPositiveDistance);

            _interpolateDistance = 0;
            _eMoveAxisPositiveDistance = 0;
            OnPropertyChanged("InterpolateDistance");
            OnPropertyChanged("EMoveAxisPositiveDistance");

            _menu = "Base";
            OnPropertyChanged("Menu");
        }

        /// <summary>
        /// Sends a valve print with movement command through the serial port.
        /// </summary>
        private RelayCommand<object> _manualValvePrintWithMovementCommand;
        public ICommand ManualValvePrintWithMovementCommand
        {
            get
            {
                if (_manualValvePrintWithMovementCommand == null)
                { _manualValvePrintWithMovementCommand = new RelayCommand<object>(ExecuteManualValvePrintWithMovementCommand, CanExecuteManualValvePrintWithMovementCommand); }
                return _manualValvePrintWithMovementCommand;
            }
        }

        public bool CanExecuteManualValvePrintWithMovementCommand(object notUsed)
        {
            return true;
        }

        public void ExecuteManualValvePrintWithMovementCommand(object notUsed)
        {
            double xDistance = 0;
            double yDistance = 0;
            double zDistance = 0;

            ParseMovement(ref xDistance, ref yDistance, ref zDistance);

            _manualControlModel.ProcessManualValvePrintWithMovementCommand(xDistance, yDistance, zDistance);

            _menu = "Base";
            OnPropertyChanged("Menu");
        }

        /// <summary>
        /// Sends a valve print without movement command through the serial port.
        /// </summary>
        private RelayCommand<object> _manualValvePrintWithoutMovementCommand;
        public ICommand ManualValvePrintWithoutMovementCommand
        {
            get
            {
                if (_manualValvePrintWithoutMovementCommand == null)
                { _manualValvePrintWithoutMovementCommand = new RelayCommand<object>(ExecuteManualValvePrintWithoutMovementCommand, CanExecuteManualValvePrintWithoutMovementCommand); }
                return _manualValvePrintWithoutMovementCommand;
            }
        }

        public bool CanExecuteManualValvePrintWithoutMovementCommand(object notUsed)
        {
            return true;
        }

        public void ExecuteManualValvePrintWithoutMovementCommand(object notUsed)
        {
            _manualControlModel.ProcessManualValvePrintWithoutMovementCommand(_valveOpenTime);

            _valveOpenTime = 0;
            OnPropertyChanged("ValveOpenTime");
            OnPropertyChanged("ValveOpenTimeMs");

            _menu = "Base";
            OnPropertyChanged("Menu");
        }

        /// <summary>
        /// Sends a set of commands for a valve droplet print through the serial port.
        /// </summary>
        private RelayCommand<object> _manualValveDropletPrintCommand;
        public ICommand ManualValveDropletPrintCommand
        {
            get
            {
                if (_manualValveDropletPrintCommand == null)
                { _manualValveDropletPrintCommand = new RelayCommand<object>(ExecuteManualValveDropletPrintCommand, CanExecuteManualValveDropletPrintCommand); }
                return _manualValveDropletPrintCommand;
            }
        }

        public bool CanExecuteManualValveDropletPrintCommand(object notUsed)
        {
            return true;
        }

        public void ExecuteManualValveDropletPrintCommand(object notUsed)
        {
            double xDistance = 0;
            double yDistance = 0;
            double zDistance = 0;

            ParseMovement(ref xDistance, ref yDistance, ref zDistance);

            _manualControlModel.ProcessManualValveDropletPrintCommand(xDistance, yDistance, zDistance, _interpolateDistance, _valveOpenTime);

            _interpolateDistance = 0;
            _valveOpenTime = 0;
            OnPropertyChanged("InterpolateDistance");
            OnPropertyChanged("ValveOpenTime");
            OnPropertyChanged("ValveOpenTimeMs");

            _menu = "Base";
            OnPropertyChanged("Menu");
        }

        /// <summary>
        /// Sends a valve close command through the serial port.
        /// </summary>
        private RelayCommand<object> _manualValveCloseCommand;
        public ICommand ManualValveCloseCommand
        {
            get
            {
                if (_manualValveCloseCommand == null)
                { _manualValveCloseCommand = new RelayCommand<object>(ExecuteManualValveCloseCommand, CanExecuteManualValveCloseCommand); }
                return _manualValveCloseCommand;
            }
        }

        public bool CanExecuteManualValveCloseCommand(object notUsed)
        {
            return true;
        }

        public void ExecuteManualValveCloseCommand(object notUsed)
        {
            _manualControlModel.ProcessManualValveCloseCommand();

            _menu = "Base";
            OnPropertyChanged("Menu");
        }

        /// <summary>
        /// Sends a manual set motorized printhead command through the serial port.
        /// </summary>
        private RelayCommand<object> _manualSetMotorizedPrintheadCommand;
        public ICommand ManualSetMotorizedPrintheadCommand
        {
            get
            {
                if (_manualSetMotorizedPrintheadCommand == null)
                { _manualSetMotorizedPrintheadCommand = new RelayCommand<object>(ExecuteManualSetMotorizedPrintheadCommand, CanExecuteManualSetMotorizedPrintheadCommand); }
                return _manualSetMotorizedPrintheadCommand;
            }
        }

        public bool CanExecuteManualSetMotorizedPrintheadCommand(object notUsed)
        {
            return true;
        }

        public void ExecuteManualSetMotorizedPrintheadCommand(object notUsed)
        {
            _manualControlModel.ProcessManualSetMotorizedPrintheadCommand(_printheadName, _maxSpeed, _acceleration);

            _maximizeSpeeds = false;
            _maxSpeed = 0;
            _acceleration = 0;
            OnPropertyChanged("MaximizeSpeeds");
            OnPropertyChanged("MaxSpeed");
            OnPropertyChanged("Acceleration");

            _menu = "Base";
            OnPropertyChanged("Menu");
        }

        /// <summary>
        /// Sends a mnual set valve printhead command through the serial port.
        /// </summary>
        private RelayCommand<object> _manualSetValvePrintheadCommand;
        public ICommand ManualSetValvePrintheadCommand
        {
            get
            {
                if (_manualSetValvePrintheadCommand == null)
                { _manualSetValvePrintheadCommand = new RelayCommand<object>(ExecuteManualSetValvePrintheadCommand, CanExecuteManualSetValvePrintheadCommand); }
                return _manualSetValvePrintheadCommand;
            }
        }

        public bool CanExecuteManualSetValvePrintheadCommand(object notUsed)
        {
            return true;
        }

        public void ExecuteManualSetValvePrintheadCommand(object notUsed)
        {
            _manualControlModel.ProcessManualSetValvePrintheadCommand(_printheadName);
            _menu = "Base";
            OnPropertyChanged("Menu");
        }

        /// <summary>
        /// Sends a set axis command through the serial port.
        /// </summary>
        private RelayCommand<object> _manualSetAxisCommand;
        public ICommand ManualSetAxisCommand
        {
            get
            {
                if (_manualSetAxisCommand == null)
                { _manualSetAxisCommand = new RelayCommand<object>(ExecuteManualSetAxisCommand, CanExecuteManualSetAxisCommand); }
                return _manualSetAxisCommand;
            }
        }

        public bool CanExecuteManualSetAxisCommand(object notUsed)
        {
            return true;
        }

        public void ExecuteManualSetAxisCommand(object notUsed)
        {
            _manualControlModel.ProcessManualSetAxisCommand(_axisName, _maxSpeed, _acceleration);

            _maximizeSpeeds = false;
            _maxSpeed = 0;
            _acceleration = 0;
            OnPropertyChanged("MaximizeSpeeds");
            OnPropertyChanged("MaxSpeed");
            OnPropertyChanged("Acceleration");

            _menu = "Base";
            OnPropertyChanged("Menu");
        }

        /// <summary>
        /// Reset the Combobox selection for Axis selection.
        /// </summary>
        private RelayCommand<object> _axisSelectionNullCommand;
        public ICommand AxisSelectionNullCommand
        {
            get
            {
                if (_axisSelectionNullCommand == null)
                { _axisSelectionNullCommand = new RelayCommand<object>(ExecuteAxisSelectionNullCommand, CanExecuteAxisSelectionNullCommand); }
                return _axisSelectionNullCommand;
            }
        }

        public bool CanExecuteAxisSelectionNullCommand(object notUsed)
        {
            return true;
        }

        public void ExecuteAxisSelectionNullCommand(object notUsed)
        {
            AxisName = "";
        }

        /// <summary>
        /// Reset the Combobox selection for Printhead selection.
        /// </summary>
        private RelayCommand<object> _printheadSelectionNullCommand;
        public ICommand PrintheadSelectionNullCommand
        {
            get
            {
                if (_printheadSelectionNullCommand == null)
                { _printheadSelectionNullCommand = new RelayCommand<object>(ExecutePrintheadSelectionNullCommand, CanExecutePrintheadSelectionNullCommand); }
                return _printheadSelectionNullCommand;
            }
        }

        public bool CanExecutePrintheadSelectionNullCommand(object notUsed)
        {
            return true;
        }

        public void ExecutePrintheadSelectionNullCommand(object notUsed)
        {
            PrintheadName = "";
        }
        #endregion
    }
}
