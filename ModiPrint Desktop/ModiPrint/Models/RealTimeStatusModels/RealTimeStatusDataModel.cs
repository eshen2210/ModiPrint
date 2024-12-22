using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModiPrint.Models.PrinterModels;
using ModiPrint.Models.PrinterModels.AxisModels;
using ModiPrint.Models.PrinterModels.PrintheadModels;
using ModiPrint.Models.PrinterModels.PrintheadModels.PrintheadTypeModels;
using ModiPrint.DataTypes.GlobalValues;
using ModiPrint.Models.RealTimeStatusModels.RealTimeStatusPrintheadModels;
using ModiPrint.Models.RealTimeStatusModels.RealTimeStatusAxisModels;
using ModiPrint.ViewModels;
using ModiPrint.ViewModels.PrintViewModels;

namespace ModiPrint.Models.RealTimeStatusModels
{
    //Events that are fired when their respective command has been interpreted and recorded.
    public delegate void RecordSetAxisExecutedEventHandler(string axisName);
    public delegate void RecordSetMotorizedPrintheadExecutedEventHandler(string printheadName);
    public delegate void RecordSetValvePrintheadExecutedEventHandler(string printheadName);
    public delegate void RecordMoveAxesExecutedEventHandler();
    public delegate void RecordMotorizedPrintWithMovementExecutedEventHandler();
    public delegate void RecordValvePrintWithMovementExecutedEventHandler();
    public delegate void RecordMotorizedPrintWithoutMovementExecutedEventHandler();
    public delegate void RecordValvePrintWithoutMovementExecutedEventHandler();
    public delegate void RecordValveCloseExecutedEventHandler();

    //Events that are fired when their respective statuses have been interpreted and recorded.
    public delegate void RecordLimitExecutedEventHandler();

    //Events that are fired when the print sequence is aborted.
    public delegate void RealTimeStatusDataAbortedEventHandler();

    //Event that is fired when the respective notification lists are updated.
    public delegate void TaskQueuedMessagesUpdatedEventHandler(string taskQueuedMessage);
    public delegate void StatusMessagesUpdatedEventHandler(string statusMessage);
    public delegate void ErrorMessagesUpdatedEventHandler(string errorMessage);

    //Associated with RealTimeStatus Models.
    //States the status of the Limit Switch during operation.
    public enum LimitSwitchStatus
    {
        NoLimit,
        UpperLimit,
        LowerLimit
    }

    /// <summary>
    /// Holds parameters regarding the printer and microcontroller while the printer is operating.
    /// </summary>
    public class RealTimeStatusDataModel
    {
        #region Fields and Properties
        //Contains data and functions related to the Printer.
        private PrinterModel _printerModel;
        public PrinterModel PrinterModel
        {
            get { return _printerModel; }
        }

        //Tasks that are in the process of execution by the microcontroller.
        //These tasks are removed from the list as task completed messages are received.
        private ObservableCollection<string> _taskQueuedMessagesList = new ObservableCollection<string>();

        //Messages that arise out of the normal order of command -> execution (emergency messages, etc.)
        private ObservableCollection<string> _statusMessagesList = new ObservableCollection<string>();

        //Parameters of the Printer.

        //Current state of operations of the Printer.
        private PrintStatus _printStatus = PrintStatus.Manual;
        public PrintStatus PrintStatus
        {
            get { return _printStatus; }
            set { _printStatus = value; }
        }

        //Parameters of each Axis.
        private RealTimeStatusAxisModel _xRealTimeStatusAxisModel;
        public RealTimeStatusAxisModel XRealTimeStatusAxisModel
        {
            get { return _xRealTimeStatusAxisModel; }
        }

        private RealTimeStatusAxisModel _yRealTimeStatusAxisModel;
        public RealTimeStatusAxisModel YRealTimeStatusAxisModel
        {
            get { return _yRealTimeStatusAxisModel; }
        }

        private RealTimeStatusAxisModel _zRealTimeStatusAxisModel;       
        public RealTimeStatusAxisModel ZRealTimeStatusAxisModel
        {
            get { return _zRealTimeStatusAxisModel; }
        }

        //Current type of the active Printhead.
        private PrintheadType _activePrintheadType = PrintheadType.Unset;
        public PrintheadType ActivePrintheadType
        {
            get { return _activePrintheadType; }
        }

        //Parameters of the active Printhead.
        private RealTimeStatusPrintheadModel _activePrintheadModel;
        public RealTimeStatusPrintheadModel ActivePrintheadModel
        {
            get { return _activePrintheadModel; }
        }

        //States whether or not hitting Limit Switches on Z actuators should change the Z Axis offset / minmax positions.
        //Once minmax positions are recorded, this flag is automatically set to false.
        //This is a product of how the Z offset is tied with its minmax positions and how the device does not know its position on startup.
        private bool _shouldZCalibrate = false;
        public bool ShouldZCalibrate
        {
            set { _shouldZCalibrate = value; }
        }
        #endregion

        #region Events
        //Events that are fired when their respective command has been interpreted and recorded.
        public event RecordSetAxisExecutedEventHandler RecordSetAxisExecuted;
        private void OnRecordSetAxisExecuted(string axisName)
        {
            if (RecordSetAxisExecuted != null)
            { RecordSetAxisExecuted(axisName); }
        }

        public event RecordSetMotorizedPrintheadExecutedEventHandler RecordSetMotorizedPrintheadExecuted;
        private void OnRecordSetMotorizedPrintheadExecuted(string printheadName)
        {
            if (RecordSetMotorizedPrintheadExecuted != null)
            { RecordSetMotorizedPrintheadExecuted(printheadName); }
        }

        public event RecordSetValvePrintheadExecutedEventHandler RecordSetValvePrintheadExecuted;
        private void OnRecordSetValvePrintheadExecuted(string printheadName)
        {
            if (RecordSetValvePrintheadExecuted != null)
            { RecordSetValvePrintheadExecuted(printheadName); }
        }

        public event RecordMoveAxesExecutedEventHandler RecordMoveAxesExecuted;
        private void OnRecordMoveAxesExecuted()
        {
            if (RecordMoveAxesExecuted != null)
            { RecordMoveAxesExecuted(); }
        }

        public event RecordMotorizedPrintWithMovementExecutedEventHandler RecordMotorizedPrintWithMovementExecuted;
        private void OnRecordMotorizedPrintWithMovementExecuted()
        {
            if (RecordMotorizedPrintWithMovementExecuted != null)
            { RecordMotorizedPrintWithMovementExecuted(); }
        }

        public event RecordValvePrintWithMovementExecutedEventHandler RecordValvePrintWithMovementExecuted;
        private void OnRecordValvePrintWithMovementExecuted()
        {
            if (RecordValvePrintWithMovementExecuted != null)
            { RecordValvePrintWithMovementExecuted(); }
        }

        public event RecordMotorizedPrintWithoutMovementExecutedEventHandler RecordMotorizedPrintWithoutMovementExecuted;
        private void OnRecordMotorizedPrintWithoutMovementExecuted()
        {
            if (RecordMotorizedPrintWithoutMovementExecuted != null)
            { RecordMotorizedPrintWithoutMovementExecuted(); }
        }

        public event RecordValvePrintWithoutMovementExecutedEventHandler RecordValvePrintWithoutMovementExecuted;
        private void OnRecordValvePrintWithoutMovementExecuted()
        {
            if (RecordValvePrintWithoutMovementExecuted != null)
            { RecordValvePrintWithoutMovementExecuted(); }
        }

        public event RecordValveCloseExecutedEventHandler RecordValveCloseExecuted;
        private void OnRecordValveCloseExecuted()
        {
            if (RecordValveCloseExecuted != null)
            { RecordValveCloseExecuted(); }
        }

        public event RecordLimitExecutedEventHandler RecordLimitExecuted;
        private void OnRecordLimitExecuted()
        {
            if (RecordLimitExecuted != null)
            { RecordLimitExecuted(); }
        }

        public event RealTimeStatusDataAbortedEventHandler RealTimeStatusDataAborted;
        private void OnRealTimeStatusDataAborted()
        {
            if (RealTimeStatusDataAborted != null)
            { RealTimeStatusDataAborted(); }
        }

        public event TaskQueuedMessagesUpdatedEventHandler TaskQueuedMessagesUpdated;
        private void OnTaskQueuedMessagesUpdated(string taskQueuedMessage)
        {
            if (TaskQueuedMessagesUpdated != null)
            { TaskQueuedMessagesUpdated(taskQueuedMessage); }
        }

        public event StatusMessagesUpdatedEventHandler StatusMessagesUpdated;
        private void OnStatusMessagesUpdated(string statusMessage)
        {
            if (StatusMessagesUpdated != null)
            { StatusMessagesUpdated(statusMessage); }
        }

        public event ErrorMessagesUpdatedEventHandler ErrorMessagesUpdated;
        private void OnErrorMessagesUpdated(string errorMessage)
        {
            if (ErrorMessagesUpdated != null)
            { ErrorMessagesUpdated(errorMessage); }
        }
        #endregion

        #region Constructor
        public RealTimeStatusDataModel(PrinterModel PrinterModel)
        {
            _printerModel = PrinterModel;

            InitializeRealTimeStatusDataModel();
        }
        #endregion

        #region Methods        
        /// <summary>
        /// Set initial values for all data.
        /// </summary>
        private void InitializeRealTimeStatusDataModel()
        {
            _xRealTimeStatusAxisModel = new RealTimeStatusAxisModel("Unset", 0, 0, 0);
            _yRealTimeStatusAxisModel = new RealTimeStatusAxisModel("Unset", 0, 0, 0);
            _zRealTimeStatusAxisModel = new RealTimeStatusAxisModel("Unset", 0, 0, 0);

            _activePrintheadType = PrintheadType.Unset;
            _activePrintheadModel = new RealTimeStatusUnsetPrintheadModel("Unset");
        }

        /// <summary>
        /// Adds another entry to the Task Queued Messages list.
        /// </summary>
        /// <param name="taskQueuedMessage"></param>
        public void RecordTaskQueued(string taskQueuedMessage)
        {
            lock(_taskQueuedMessagesList)
            {
                _taskQueuedMessagesList.Add(taskQueuedMessage);
            }

            OnTaskQueuedMessagesUpdated(taskQueuedMessage);
        }
        
        /// <summary>
        /// Removes the earliest entry in the Task Queued Messages list.
        /// </summary>
        public void RecordTaskCompleted()
        {
            string taskQueuedMessage = "";
            lock (_taskQueuedMessagesList)
            {
                taskQueuedMessage = _taskQueuedMessagesList[0];
                _taskQueuedMessagesList.RemoveAt(0);
            }
            OnTaskQueuedMessagesUpdated(taskQueuedMessage);
        }

        /// <summary>
        /// Returns true if the TaskQueuedMessagesList contains messages.
        /// </summary>
        /// <returns></returns>
        public bool TaskQueuedMessagesListContainsMessages()
        {
            lock (_taskQueuedMessagesList)
            {
                return (_taskQueuedMessagesList.Count > 0) ? true : false;
            }
        }

        /// <summary>
        /// Returns true if there are no entries in the TaskQueuedMessagesList.
        /// </summary>
        /// <returns></returns>
        public bool IsTaskQueuedEmpty()
        {
            lock(_taskQueuedMessagesList)
            {
                return (_taskQueuedMessagesList.Count == 0) ? true : false;
            }
        }

        /// <summary>
        /// Returns the zero index of the TaskQueuedMessagesList.
        /// </summary>
        /// <returns></returns>
        public string RetrieveNextTaskQueuedMessage()
        {
            lock(_taskQueuedMessagesList)
            {
                return (_taskQueuedMessagesList.Count > 0) ? _taskQueuedMessagesList[0] : "";
            }
        }

        /// <summary>
        /// Remove the zero index of the TaskQueuedMessagesList.
        /// </summary>
        public void RemoveNextTaskQueuedMessage()
        {
            lock(_taskQueuedMessagesList)
            {
                if (_taskQueuedMessagesList.Count > 0)
                {
                    _taskQueuedMessagesList.RemoveAt(0);
                }
            }
        }

        /// <summary>
        /// Records a new message to the Status Messages list.
        /// </summary>
        /// <param name="statusMessage"></param>
        public void RecordStatusMessage(string statusMessage)
        {
            lock(_statusMessagesList)
            {
                _statusMessagesList.Add(statusMessage);
            }
            
            OnStatusMessagesUpdated(statusMessage);
        }

        /// <summary>
        /// Records a a new message to the Error Messages list.
        /// </summary>
        /// <param name="errorMessage"></param>
        public void RecordErrorMessage(string errorMessage)
        {
            OnErrorMessagesUpdated(errorMessage);
        }
        
        /// <summary>
        /// Record the parameters from a Set Axis command.
        /// </summary>
        /// <param name="axisName"></param>
        /// <param name="maxSpeed"></param>
        /// <param name="acceleration"></param>
        public void RecordSetAxis(char axisID, string axisName, double maxSpeed, double acceleration)
        {
            AxisModel axisModel = _printerModel.FindAxis(axisName);
            double mmPerStep = axisModel.MmPerStep;

            //Max speed is read as steps / s and acceleration is read as steps / s2.
            //These parameters are converted to this program's convention of mm / s and mm / s2.
            double convertedMaxSpeed = maxSpeed * mmPerStep;
            double convertedAcceleration = acceleration * mmPerStep;

            //Record new data.
            switch (axisID)
            {
                case 'X':
                    _xRealTimeStatusAxisModel = new RealTimeStatusAxisModel(axisName, _xRealTimeStatusAxisModel.Position, convertedMaxSpeed, convertedAcceleration);
                    break;
                case 'Y':
                    _yRealTimeStatusAxisModel = new RealTimeStatusAxisModel(axisName, _yRealTimeStatusAxisModel.Position, convertedMaxSpeed, convertedAcceleration);
                    break;
                case 'Z':
                    //If the Z Axis does not need to be changed, then keep the same position. Otherwise, create new positions assuming the Z actuator was retracted before becoming active.
                    double zPosition = (axisName == _zRealTimeStatusAxisModel.Name) ? _zRealTimeStatusAxisModel.Position : _printerModel.FindAxis(axisName).MaxPosition - GlobalValues.LimitBuffer - _printerModel.FindAxis(axisName).MinPosition;
                    _zRealTimeStatusAxisModel = new RealTimeStatusAxisModel(axisName, zPosition, convertedMaxSpeed, convertedAcceleration);
                    break;
                default:
                    //Should never reach this point.
                    break;
            }

            //Notify other classes.
            OnRecordSetAxisExecuted(axisName);
        }

        /// <summary>
        /// Record the parameters from a Set Motorized Printhead command.
        /// </summary>
        /// <param name="printheadName"></param>
        /// <param name="maxSpeed"></param>
        /// <param name="acceleration"></param>
        public void RecordSetMotorizedPrinthead(string printheadName, double maxSpeed, double acceleration)
        {
            _activePrintheadType = PrintheadType.Motorized;

            MotorizedPrintheadTypeModel motorizedPrintheadTypeModel = (MotorizedPrintheadTypeModel)_printerModel.FindPrinthead(printheadName).PrintheadTypeModel;
            double mmPerStep = motorizedPrintheadTypeModel.MmPerStep;

            //Max speed is read as steps / s and acceleration is read as steps / s2.
            //These parameters are converted to this program's convention of mm / s and mm / s2.
            double convertedMaxSpeed = maxSpeed * mmPerStep;
            double convertedAcceleration = acceleration * mmPerStep;

            _activePrintheadModel = new RealTimeStatusMotorizedPrintheadModel(printheadName, convertedMaxSpeed, convertedAcceleration);

            //Notify other classes.
            OnRecordSetMotorizedPrintheadExecuted(printheadName);
        }

        /// <summary>
        /// Record the parameters from a Set Valve Printhead command.
        /// </summary>
        /// <param name="printheadName"></param>
        public void RecordSetValvePrinthead(string printheadName)
        {
            _activePrintheadType = PrintheadType.Valve;

            _activePrintheadModel = new RealTimeStatusValvePrintheadModel(printheadName);

            //Notify other classes.
            OnRecordSetValvePrintheadExecuted(printheadName);
        }

        /// <summary>
        /// Record the parameters from a Move Axes command.
        /// Parameters represent relative positions.
        /// </summary>
        /// <param name="xPosition"></param>
        /// <param name="yPosition"></param>
        /// <param name="zPosition"></param>
        public void RecordMoveAxes(int xStepsTaken, int yStepsTaken, int zStepsTaken)
        {
            //mm Per Step.
            double xmmPerStep = _printerModel.AxisModelList[0].MmPerStep;
            double ymmPerStep = _printerModel.AxisModelList[1].MmPerStep;
            double zmmPerStep = 0;
            AxisModel zAxisModel = _printerModel.FindAxis(_zRealTimeStatusAxisModel.Name);
            if (zAxisModel != null)
            { zmmPerStep = zAxisModel.MmPerStep; }

            //Record positions.
            if (xStepsTaken != 0)
            {
                _xRealTimeStatusAxisModel.Position += (_printerModel.AxisModelList[0].IsDirectionInverted == false) ? (double)(xStepsTaken * xmmPerStep) : (double)(xStepsTaken * -1 * xmmPerStep);
                _xRealTimeStatusAxisModel.LimitSwitchStatus = LimitSwitchStatus.NoLimit;
            }

            if (yStepsTaken != 0)
            {
                _yRealTimeStatusAxisModel.Position += (_printerModel.AxisModelList[1].IsDirectionInverted == false) ? (double)(yStepsTaken * ymmPerStep) : (double)(yStepsTaken * -1 * ymmPerStep);
                _yRealTimeStatusAxisModel.LimitSwitchStatus = LimitSwitchStatus.NoLimit;
            }

            if (zStepsTaken != 0)
            {
                _zRealTimeStatusAxisModel.Position += (_printerModel.FindAxis(_zRealTimeStatusAxisModel.Name).IsDirectionInverted == false) ? (double)(zStepsTaken * zmmPerStep) : (double)(zStepsTaken * -1 * zmmPerStep);
                _zRealTimeStatusAxisModel.LimitSwitchStatus = LimitSwitchStatus.NoLimit;
            }

            //Notify other classes.
            OnRecordMoveAxesExecuted();
        }

        /// <summary>
        /// Record the parameters from a Motorized Printhead Print With Movement command.
        /// </summary>
        /// <param name="taskQueuedMessage"></param>
        /// <param name="eStepsTaken"></param>
        /// <param name="xStepsTaken"></param>
        /// <param name="yStepsTaken"></param>
        /// <param name="zStepsTaken"></param>
        public void RecordMotorizedPrintWithMovement(int eStepsTaken, int xStepsTaken, int yStepsTaken, int zStepsTaken)
        {
            if (_activePrintheadType == PrintheadType.Motorized)
            {
                MotorizedPrintheadTypeModel motorizedPrintheadTypeModel = (MotorizedPrintheadTypeModel)_printerModel.FindPrinthead(_activePrintheadModel.Name).PrintheadTypeModel;
                double emmPerStep = motorizedPrintheadTypeModel.MmPerStep;
                double xmmPerStep = _printerModel.AxisModelList[0].MmPerStep;
                double ymmPerStep = _printerModel.AxisModelList[1].MmPerStep;
                double zmmPerStep = _printerModel.FindAxis(_zRealTimeStatusAxisModel.Name).MmPerStep;

                //Record data.
                RealTimeStatusMotorizedPrintheadModel motorizedPrintheadModel = (RealTimeStatusMotorizedPrintheadModel)_activePrintheadModel;

                //Record positions.
                if (eStepsTaken != 0)
                {
                    motorizedPrintheadModel.Position += (motorizedPrintheadTypeModel.IsDirectionInverted == false) ? (double)(eStepsTaken * emmPerStep) : (double)(eStepsTaken * -1 * emmPerStep);
                    motorizedPrintheadModel.LimitSwitchStatus = LimitSwitchStatus.NoLimit;
                }

                if (xStepsTaken != 0)
                {
                    _xRealTimeStatusAxisModel.Position += (_printerModel.AxisModelList[0].IsDirectionInverted == false) ? (double)(xStepsTaken * xmmPerStep) : (double)(xStepsTaken * -1 * xmmPerStep);
                    _xRealTimeStatusAxisModel.LimitSwitchStatus = LimitSwitchStatus.NoLimit;
                }

                if (yStepsTaken != 0)
                {
                    _yRealTimeStatusAxisModel.Position += (_printerModel.AxisModelList[1].IsDirectionInverted == false) ? (double)(yStepsTaken * ymmPerStep) : (double)(yStepsTaken * -1 * ymmPerStep);
                    _yRealTimeStatusAxisModel.LimitSwitchStatus = LimitSwitchStatus.NoLimit;
                }

                if (zStepsTaken != 0)
                {
                    _zRealTimeStatusAxisModel.Position += (_printerModel.FindAxis(_zRealTimeStatusAxisModel.Name).IsDirectionInverted == false) ? (double)(zStepsTaken * zmmPerStep) : (double)(zStepsTaken * -1 * zmmPerStep);
                    _zRealTimeStatusAxisModel.LimitSwitchStatus = LimitSwitchStatus.NoLimit;
                }

                //Notify other classes.
                OnRecordMotorizedPrintWithMovementExecuted();
            }
        }

        /// <summary>
        /// Record the parameters from a Valve Printhead Print With Movement command.
        /// </summary>
        /// <param name="xStepsTaken"></param>
        /// <param name="yStepsTaken"></param>
        /// <param name="zStepsTaken"></param>
        public void RecordValvePrintWithMovement(int xStepsTaken, int yStepsTaken, int zStepsTaken)
        {
            double xmmPerStep = _printerModel.AxisModelList[0].MmPerStep;
            double ymmPerStep = _printerModel.AxisModelList[1].MmPerStep;
            double zmmPerStep = _printerModel.FindAxis(_zRealTimeStatusAxisModel.Name).MmPerStep;

            //Record data.
            RealTimeStatusValvePrintheadModel valvePrinthead = (RealTimeStatusValvePrintheadModel)_activePrintheadModel; //Unsed parameter.
            valvePrinthead.IsValveOn = false;

            if (xStepsTaken != 0)
            {
                _xRealTimeStatusAxisModel.Position += (_printerModel.AxisModelList[0].IsDirectionInverted == false) ? (double)(xStepsTaken * xmmPerStep) : (double)(xStepsTaken * -1 * xmmPerStep);
                _xRealTimeStatusAxisModel.LimitSwitchStatus = LimitSwitchStatus.NoLimit;
            }

            if (yStepsTaken != 0)
            {
                _yRealTimeStatusAxisModel.Position += (_printerModel.AxisModelList[1].IsDirectionInverted == false) ? (double)(yStepsTaken * ymmPerStep) : (double)(yStepsTaken * -1 * ymmPerStep);
                _yRealTimeStatusAxisModel.LimitSwitchStatus = LimitSwitchStatus.NoLimit;
            }

            if (zStepsTaken != 0)
            {
                _zRealTimeStatusAxisModel.Position += (_printerModel.FindAxis(_zRealTimeStatusAxisModel.Name).IsDirectionInverted == false) ? (double)(zStepsTaken * zmmPerStep) : (double)(zStepsTaken * -1 * zmmPerStep);
                _zRealTimeStatusAxisModel.LimitSwitchStatus = LimitSwitchStatus.NoLimit;
            }

            //Notify other classes.
            OnRecordValvePrintWithMovementExecuted();
        }

        /// <summary>
        /// Record the parameters from a Motorized Printhead Print without Movement command.
        /// </summary>
        /// <param name="ePosition"></param>
        public void RecordMotorizedPrintWithoutMovement(double eStepsTaken)
        {
            if (_activePrintheadType == PrintheadType.Motorized)
            {
                MotorizedPrintheadTypeModel motorizedPrintheadTypeModel = (MotorizedPrintheadTypeModel)_printerModel.FindPrinthead(_activePrintheadModel.Name).PrintheadTypeModel;
                double emmPerStep = motorizedPrintheadTypeModel.MmPerStep;

                //Record data.
                RealTimeStatusMotorizedPrintheadModel motorizedPrintheadModel = (RealTimeStatusMotorizedPrintheadModel)_activePrintheadModel;

                if (eStepsTaken != 0)
                {
                    motorizedPrintheadModel.Position += (motorizedPrintheadTypeModel.IsDirectionInverted == false) ? (double)(eStepsTaken * emmPerStep) : (double)(eStepsTaken * -1 * emmPerStep);
                    motorizedPrintheadModel.LimitSwitchStatus = LimitSwitchStatus.NoLimit;
                }

                //Notify other classes.
                OnRecordMotorizedPrintWithoutMovementExecuted();
            }
        }

        /// <summary>
        /// Record the parameters from a Valve Printhead Print without Movement command.
        /// </summary>
        /// <param name="openTime"></param>
        public void RecordValvePrintWithoutMovement(double openTime)
        {
            //Record data.
            if (_activePrintheadType == PrintheadType.Valve)
            {
                RealTimeStatusValvePrintheadModel realTimeStatusValvePrintheadModel = (RealTimeStatusValvePrintheadModel)_activePrintheadModel;

                //With a parameter value of zero, the valve should be open until the close command is executed.
                //Otherwise, the valve will have opened for a set time and have been closed by the time this message is received.
                if (openTime == 0)
                {
                    realTimeStatusValvePrintheadModel.IsValveOn = true;
                }
                else
                {
                    realTimeStatusValvePrintheadModel.IsValveOn = false;
                }
            }

            //Notify other classes.
            OnRecordValvePrintWithoutMovementExecuted();
        }

        /// <summary>
        /// Record the parameters from a Valve Printhead Close command.
        /// </summary>
        public void RecordValveClose()
        {
            RealTimeStatusValvePrintheadModel realTimeStatusValvePrintheadModel = (RealTimeStatusValvePrintheadModel)_activePrintheadModel;
            realTimeStatusValvePrintheadModel.IsValveOn = false;
        
            //Notify other classes.
            OnRecordValveCloseExecuted();
        }

        /// <summary>
        /// Record the parameters from a Limit Switch status message.
        /// </summary>
        /// <param name="xLimit"></param>
        /// <param name="yLimit"></param>
        /// <param name="zLimit"></param>
        /// <param name="eLimit"></param>
        /// <param name="xStepsTaken"></param>
        /// <param name="yStepsTaken"></param>
        /// <param name="zStepsTaken"></param>
        /// <param name="eStepsTaken"></param>
        public void RecordLimit(bool xLimit, bool yLimit, bool zLimit, bool eLimit, int xStepsTaken, int yStepsTaken, int zStepsTaken, int eStepsTaken)
        {
            //E
            if (_activePrintheadType == PrintheadType.Motorized)
            {
                //mm Per Step.
                MotorizedPrintheadTypeModel motorizedPrintheadTypeModel = (MotorizedPrintheadTypeModel)_printerModel.FindPrinthead(_activePrintheadModel.Name).PrintheadTypeModel;
                eStepsTaken = (motorizedPrintheadTypeModel.IsDirectionInverted == false) ? eStepsTaken : (-1 * eStepsTaken);
                double emmPerStep = motorizedPrintheadTypeModel.MmPerStep;

                //Update Position.
                RealTimeStatusMotorizedPrintheadModel realTimeStatusMotorizedPrintheadTypeModel = (RealTimeStatusMotorizedPrintheadModel)_activePrintheadModel;
                realTimeStatusMotorizedPrintheadTypeModel.Position += (double)(eStepsTaken * -1 * emmPerStep);

                //Update Max/Min Position and Limit.
                if (eLimit == true)
                {
                    if (eStepsTaken > 0)
                    {
                        motorizedPrintheadTypeModel.MaxPosition = realTimeStatusMotorizedPrintheadTypeModel.Position;
                        realTimeStatusMotorizedPrintheadTypeModel.LimitSwitchStatus = LimitSwitchStatus.UpperLimit;
                    }
                    else if (eStepsTaken < 0)
                    {
                        motorizedPrintheadTypeModel.MinPosition = realTimeStatusMotorizedPrintheadTypeModel.Position;
                        realTimeStatusMotorizedPrintheadTypeModel.LimitSwitchStatus = LimitSwitchStatus.LowerLimit;
                    }
                }
            }

            AxisModel xAxisModel = _printerModel.AxisModelList[0];
            AxisModel yAxisModel = _printerModel.AxisModelList[1];
            AxisModel zAxisModel = _printerModel.FindAxis(_zRealTimeStatusAxisModel.Name);

            //mm Per Step.
            double xmmPerStep = xAxisModel.MmPerStep;
            double ymmPerStep = yAxisModel.MmPerStep;
            double zmmPerStep = (zAxisModel != null) ? _printerModel.FindAxis(_zRealTimeStatusAxisModel.Name).MmPerStep : 0;

            //Record positions.
            if (xStepsTaken != 0)
            {
                xStepsTaken = (_printerModel.AxisModelList[0].IsDirectionInverted == false) ? xStepsTaken : (-1 * xStepsTaken);
                _xRealTimeStatusAxisModel.Position += (double)(xStepsTaken * xmmPerStep);
            }

            if (yStepsTaken != 0)
            {
                yStepsTaken = (_printerModel.AxisModelList[1].IsDirectionInverted == false) ? yStepsTaken : (-1 * yStepsTaken);
                _yRealTimeStatusAxisModel.Position += (double)(yStepsTaken * ymmPerStep);
            }

            if (zStepsTaken != 0)
            {
                zStepsTaken = (_printerModel.FindAxis(_zRealTimeStatusAxisModel.Name).IsDirectionInverted == false) ? zStepsTaken : (-1 * zStepsTaken);
                _zRealTimeStatusAxisModel.Position += (double)(zStepsTaken * zmmPerStep);
            }

            //Set Max Positions and Limits.
            if (xLimit == true)
            {
                if (xStepsTaken > 0)
                {
                    xAxisModel.MaxPosition = _xRealTimeStatusAxisModel.Position;
                    _xRealTimeStatusAxisModel.LimitSwitchStatus = LimitSwitchStatus.UpperLimit;
                }
                else if (xStepsTaken < 0)
                {
                    xAxisModel.MinPosition = _xRealTimeStatusAxisModel.Position;
                    _xRealTimeStatusAxisModel.LimitSwitchStatus = LimitSwitchStatus.LowerLimit;
                }
            }

            if (yLimit == true)
            {
                if (yStepsTaken > 0)
                {
                    yAxisModel.MaxPosition = _yRealTimeStatusAxisModel.Position;
                    _yRealTimeStatusAxisModel.LimitSwitchStatus = LimitSwitchStatus.UpperLimit;
                }
                else if (yStepsTaken < 0)
                {
                    yAxisModel.MinPosition = _yRealTimeStatusAxisModel.Position;
                    _yRealTimeStatusAxisModel.LimitSwitchStatus = LimitSwitchStatus.LowerLimit;
                }
            }

            if (zLimit == true)
            {
                if (zStepsTaken > 0)
                {
                    if (_shouldZCalibrate == true)
                    {
                        zAxisModel.MaxPosition = _zRealTimeStatusAxisModel.Position;
                        _shouldZCalibrate = false;
                    }
                    _zRealTimeStatusAxisModel.LimitSwitchStatus = LimitSwitchStatus.UpperLimit;
                }
                else if (zStepsTaken < 0)
                {
                    //The minimum Position of Z Axes are always zero.
                    //If the lower limit of a Z Axis is hit, then adjust the Max Position such that the range remains the same.
                    if (_shouldZCalibrate == true)
                    {
                        zAxisModel.MaxPosition = zAxisModel.MaxPosition + _zRealTimeStatusAxisModel.Position;
                        zAxisModel.MinPosition = 0;
                        _shouldZCalibrate = false;
                    }
                    _zRealTimeStatusAxisModel.LimitSwitchStatus = LimitSwitchStatus.LowerLimit;
                }
            }

            //Notify other classes.
            OnRecordLimitExecuted();
        }

        /// <summary>
        /// Called when an abort is executed.
        /// Clears various values.
        /// </summary>
        /// <returns></returns>
        public void Abort()
        {
            //Wipe the task queued messages list.
            lock (_taskQueuedMessagesList)
            {
                _taskQueuedMessagesList.Clear();
            }

            //Reset data.
            InitializeRealTimeStatusDataModel();

            OnRealTimeStatusDataAborted();
        }

        #endregion
    }
}
