using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ModiPrint.DataTypes.GlobalValues;
using ModiPrint.Models.PrinterModels;
using ModiPrint.Models.PrinterModels.AxisModels;
using ModiPrint.Models.PrinterModels.PrintheadModels;
using ModiPrint.Models.PrinterModels.PrintheadModels.PrintheadTypeModels;
using ModiPrint.Models.RealTimeStatusModels;
using ModiPrint.Models.RealTimeStatusModels.RealTimeStatusPrintheadModels;
using ModiPrint.Models.SerialCommunicationModels;
using ModiPrint.Models.GCodeConverterModels;
using ModiPrint.Models.GCodeConverterModels.ProcessModels.WriteSetEquipmentModels;
using ModiPrint.Models.GCodeConverterModels.ProcessModels.ProcessG00Models;
using ModiPrint.ViewModels;

namespace ModiPrint.Models.ManualControlModels
{
    public class ManualControlModel
    {
        #region Fields and Methods
        //Contains information on the Printer.
        PrinterModel _printerModel;

        //Contains functions to queue outgoing messages for the serial port.
        SerialCommunicationOutgoingMessagesModel _serialCommunicationOutgoingMessagesModel;

        //Contains information on the state of printer information during operation.
        RealTimeStatusDataModel _realTimeStatusDataModel;

        //Handles errors.
        ErrorListViewModel _errorListViewModel;

        //Contains functions to generate commands.
        WriteSetPrintheadModel _writeSetPrintheadModel;
        WriteSetAxisModel _writeSetAxisModel;
        #endregion

        #region Constructor
        public ManualControlModel(PrinterModel PrinterModel, SerialCommunicationOutgoingMessagesModel SerialCommunicationOutgoingMessagesModel,
            RealTimeStatusDataModel RealTimeStatusDataModel, ErrorListViewModel ErrorListViewModel)
        {
            _printerModel = PrinterModel;
            _serialCommunicationOutgoingMessagesModel = SerialCommunicationOutgoingMessagesModel;
            _realTimeStatusDataModel = RealTimeStatusDataModel;
            _errorListViewModel = ErrorListViewModel;

            ParametersModel parametersModel = new ParametersModel(_printerModel, null);

            _writeSetPrintheadModel = new WriteSetPrintheadModel(parametersModel);
            _writeSetAxisModel = new WriteSetAxisModel(parametersModel);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Takes manual input parameters for a movement command and outputs the command to the serial port.
        /// </summary>
        /// <param name="xDistance"></param>
        /// <param name="yDistance"></param>
        /// <param name="zDistance"></param>
        public void ProcessManualMovementCommand(double xDistance, double yDistance, double zDistance)
        {
            double xmmPerStep = (xDistance != 0) ? _printerModel.AxisModelList[0].MmPerStep : double.MaxValue;
            double ymmPerStep = (yDistance != 0) ? _printerModel.AxisModelList[1].MmPerStep : double.MaxValue;
            AxisModel zAxisModel = _printerModel.FindAxis(_realTimeStatusDataModel.ZRealTimeStatusAxisModel.Name);
            double zmmPerStep = (zDistance != 0) ? _printerModel.FindAxis(zAxisModel.Name).MmPerStep : double.MaxValue;
            bool zDirectionInverted = (zAxisModel != null) ? zAxisModel.IsDirectionInverted : false;

            double unused = 0;
            string axesMovementString = GCodeLinesConverter.GCodeLinesListToString(
                WriteG00.WriteAxesMovement(xmmPerStep, ymmPerStep, zmmPerStep,
                xDistance, yDistance, zDistance,
                _printerModel.AxisModelList[0].IsDirectionInverted, _printerModel.AxisModelList[1].IsDirectionInverted, zDirectionInverted,
                ref unused, ref unused, ref unused));

            _serialCommunicationOutgoingMessagesModel.QueueNextProspectiveOutgoingMessage(axesMovementString);
        }

        /// <summary>
        /// Takes manual input parameters for a motor print with movement command and outputs the command to the serial port.
        /// </summary>
        /// <param name="xDistance"></param>
        /// <param name="yDistance"></param>
        /// <param name="zDistance"></param>
        /// <param name="eDispensePerDistance"></param>
        public void ProcessManualMotorPrintWithMovementCommand(double xDistance, double yDistance, double zDistance, double eDispensePerDistance)
        {
            MotorizedPrintheadTypeModel motorizedPrintheadTypeModel = (MotorizedPrintheadTypeModel)_printerModel.FindPrinthead(_realTimeStatusDataModel.ActivePrintheadModel.Name).PrintheadTypeModel;
            double emmPerStep = motorizedPrintheadTypeModel.MmPerStep;
            double xmmPerStep = (xDistance != 0) ? _printerModel.AxisModelList[0].MmPerStep : double.MaxValue;
            double ymmPerStep = (yDistance != 0) ? _printerModel.AxisModelList[1].MmPerStep : double.MaxValue;
            AxisModel zAxisModel = _printerModel.FindAxis(_realTimeStatusDataModel.ZRealTimeStatusAxisModel.Name);
            double zmmPerStep = (zDistance != 0) ? _printerModel.FindAxis(zAxisModel.Name).MmPerStep : double.MaxValue;
            bool zDirectionInverted = (zAxisModel != null) ? zAxisModel.IsDirectionInverted : false;

            double unused = 0;
            string printString = GCodeLinesConverter.GCodeLinesListToString(
                WriteG00.WriteMotorizedContinuousPrint(emmPerStep, xmmPerStep, ymmPerStep, zmmPerStep,
                eDispensePerDistance, xDistance, yDistance, zDistance,
                _printerModel.AxisModelList[0].IsDirectionInverted, _printerModel.AxisModelList[1].IsDirectionInverted, zDirectionInverted, motorizedPrintheadTypeModel.IsDirectionInverted,
                ref unused, ref unused, ref unused, ref unused,
                null));

            _serialCommunicationOutgoingMessagesModel.QueueNextProspectiveOutgoingMessage(printString);
        }

        /// <summary>
        /// Takes manual input parameters for a motor print without movement command and outputs the command to the serial port.
        /// </summary>
        /// <param name="eDistance"></param>
        public void ProcessManualMotorPrintWithoutMovementCommand(double eDistance)
        {
            MotorizedPrintheadTypeModel motorizedPrintheadTypeModel = (MotorizedPrintheadTypeModel)_printerModel.FindPrinthead(_realTimeStatusDataModel.ActivePrintheadModel.Name).PrintheadTypeModel;
            double emmPerStep = motorizedPrintheadTypeModel.MmPerStep;
            RealTimeStatusMotorizedPrintheadModel realTimeStatusMotorizedPrintheadModel = (RealTimeStatusMotorizedPrintheadModel)_realTimeStatusDataModel.ActivePrintheadModel;

            double unused = 0;
            string printString = GCodeLinesConverter.GCodeLinesListToString(
                WriteG00.WriteMotorizedPrintWithoutMovement(eDistance, emmPerStep, motorizedPrintheadTypeModel.IsDirectionInverted, ref unused, null));

            _serialCommunicationOutgoingMessagesModel.QueueNextProspectiveOutgoingMessage(printString);
        }

        /// <summary>
        /// Takes manual input parameters for a set of commands for motorized printhead droplet printing and outputs the commands to the serial port.
        /// </summary>
        /// <param name="xDistance"></param>
        /// <param name="yDistance"></param>
        /// <param name="zDistance"></param>
        /// <param name="interpolateDistance"></param>
        /// <param name="eDispensePerDroplet"></param>
        public void ProcessManualMotorDropletPrintCommand(double xDistance, double yDistance, double zDistance, double interpolateDistance, double eDispensePerDroplet)
        {
            PrintheadModel printheadModel = _printerModel.FindPrinthead(_realTimeStatusDataModel.ActivePrintheadModel.Name);
            MotorizedPrintheadTypeModel motorizedPrintheadTypeModel = (MotorizedPrintheadTypeModel)printheadModel.PrintheadTypeModel;
            double emmPerStep = motorizedPrintheadTypeModel.MmPerStep;
            RealTimeStatusMotorizedPrintheadModel realTimeStatusMotorizedPrintheadModel = (RealTimeStatusMotorizedPrintheadModel)_realTimeStatusDataModel.ActivePrintheadModel;
            double eMaxSpeed = realTimeStatusMotorizedPrintheadModel.MaxSpeed;
            double eAcceleration = realTimeStatusMotorizedPrintheadModel.Acceleration;
            double xmmPerStep = (xDistance != 0) ? _printerModel.AxisModelList[0].MmPerStep : double.MaxValue;
            double ymmPerStep = (yDistance != 0) ? _printerModel.AxisModelList[1].MmPerStep : double.MaxValue;
            AxisModel zAxisModel = _printerModel.FindAxis(_realTimeStatusDataModel.ZRealTimeStatusAxisModel.Name);
            double zmmPerStep = (zDistance != 0) ? _printerModel.FindAxis(zAxisModel.Name).MmPerStep : double.MaxValue;
            bool zDirectionInverted = (zAxisModel != null) ? zAxisModel.IsDirectionInverted : false;

            double unused = 0;
            string printString = GCodeLinesConverter.GCodeLinesListToString(
                WriteG00.WriteMotorizedDropletPrint(
                emmPerStep, xmmPerStep, ymmPerStep, zmmPerStep,
                eDispensePerDroplet, 0, xDistance, 0, yDistance, 0, zDistance,
                _printerModel.AxisModelList[0].IsDirectionInverted, _printerModel.AxisModelList[1].IsDirectionInverted, zDirectionInverted, motorizedPrintheadTypeModel.IsDirectionInverted,
                ref unused, ref unused, ref unused, ref unused,
                null, new DropletModel(interpolateDistance)));

            _serialCommunicationOutgoingMessagesModel.QueueNextProspectiveOutgoingMessage(printString);
        }

        /// <summary>
        /// Takes manual input parameters for a valve print with movement command and outputs the command to the serial port.
        /// </summary>
        /// <param name="xDistance"></param>
        /// <param name="yDistance"></param>
        /// <param name="zDistance"></param>
        public void ProcessManualValvePrintWithMovementCommand(double xDistance, double yDistance, double zDistance)
        {
            double xmmPerStep = (xDistance != 0) ? _printerModel.AxisModelList[0].MmPerStep : double.MaxValue;
            double ymmPerStep = (yDistance != 0) ? _printerModel.AxisModelList[1].MmPerStep : double.MaxValue;
            AxisModel zAxisModel = _printerModel.FindAxis(_realTimeStatusDataModel.ZRealTimeStatusAxisModel.Name);
            double zmmPerStep = (zDistance != 0) ? _printerModel.FindAxis(zAxisModel.Name).MmPerStep : double.MaxValue;
            bool zDirectionInverted = (zAxisModel != null) ? zAxisModel.IsDirectionInverted : false;

            double unused = 0;
            string printString = GCodeLinesConverter.GCodeLinesListToString(
                WriteG00.WriteValveContinuousPrint(xmmPerStep, ymmPerStep, zmmPerStep, 
                xDistance, yDistance, zDistance,
                _printerModel.AxisModelList[0].IsDirectionInverted, _printerModel.AxisModelList[1].IsDirectionInverted, zDirectionInverted,
                ref unused, ref unused, ref unused));

            _serialCommunicationOutgoingMessagesModel.QueueNextProspectiveOutgoingMessage(printString);
        }

        /// <summary>
        /// Takes manual input parameters for a valve print without movement command and outputs the command to the serial port.
        /// </summary>
        /// <param name="openTime"></param>
        public void ProcessManualValvePrintWithoutMovementCommand(int valveOpenTime)
        {
            string printString = GCodeLinesConverter.GCodeLinesListToString(
                WriteG00.WriteValvePrintWithoutMovement(valveOpenTime));

            _serialCommunicationOutgoingMessagesModel.QueueNextProspectiveOutgoingMessage(printString);
        }

        /// <summary>
        /// Takes manual input parameters for a set of commands for valve printhead droplet printing and outputs the commands to the serial port.
        /// </summary>
        /// <param name="xDistance"></param>
        /// <param name="yDistance"></param>
        /// <param name="zDistance"></param>
        /// <param name="interpolateDistance"></param>
        /// <param name="valveOpenTime"></param>
        public void ProcessManualValveDropletPrintCommand(double xDistance, double yDistance, double zDistance, double interpolateDistance, int valveOpenTime)
        {
            double xmmPerStep = (xDistance != 0) ? _printerModel.AxisModelList[0].MmPerStep : double.MaxValue;
            double ymmPerStep = (yDistance != 0) ? _printerModel.AxisModelList[1].MmPerStep : double.MaxValue;
            AxisModel zAxisModel = _printerModel.FindAxis(_realTimeStatusDataModel.ZRealTimeStatusAxisModel.Name);
            double zmmPerStep = (zDistance != 0) ? _printerModel.FindAxis(zAxisModel.Name).MmPerStep : double.MaxValue;
            bool zDirectionInverted = (zAxisModel != null) ? zAxisModel.IsDirectionInverted : false;

            double unused = 0;
            string printString = GCodeLinesConverter.GCodeLinesListToString(
                WriteG00.WriteValveDropletPrint(xmmPerStep, ymmPerStep, zmmPerStep, valveOpenTime, 
                0, xDistance, 0, yDistance, 0, zDistance,
                _printerModel.AxisModelList[0].IsDirectionInverted, _printerModel.AxisModelList[1].IsDirectionInverted, zDirectionInverted,
                ref unused, ref unused, ref unused,
                new DropletModel(interpolateDistance)));

            _serialCommunicationOutgoingMessagesModel.QueueNextProspectiveOutgoingMessage(printString);
        }

        /// <summary>
        /// Takes manual input parameters for a valve close command and outputs the command to the serial port.
        /// </summary>
        public void ProcessManualValveCloseCommand()
        {
            string valveCloseString = GCodeLinesConverter.GCodeLinesListToString(WriteG00.WriteValveClose());

            _serialCommunicationOutgoingMessagesModel.QueueNextProspectiveOutgoingMessage(valveCloseString);
        }

        /// <summary>
        /// Takes manual input parameters for a set axis command and outputs the command to the serial port.
        /// </summary>
        /// <param name="axisName"></param>
        /// <param name="maxSpeed"></param>
        /// <param name="acceleration"></param>
        public void ProcessManualSetAxisCommand(string axisName, double maxSpeed, double acceleration)
        {
            try
            {
                List<string> outgoingMessagesList = new List<string>();

                AxisModel axisModel = _printerModel.FindAxis(axisName);

                int limitPinID = (axisModel.AttachedLimitSwitchGPIOPinModel == null) ? GlobalValues.PinIDNull : axisModel.AttachedLimitSwitchGPIOPinModel.PinID;

                string setAxisString = _writeSetAxisModel.WriteSetAxis(axisModel.AxisID, axisModel.AttachedMotorStepGPIOPinModel.PinID, axisModel.AttachedMotorDirectionGPIOPinModel.PinID,
                    axisModel.StepPulseTime, limitPinID, maxSpeed, acceleration, axisModel.MmPerStep);

                //If switching Z Axes, then retract the current Z Axis first.
                if (axisModel.AxisID == 'Z')
                { outgoingMessagesList.Add(SerialMessageCharacters.SerialCommandSetCharacter + "RetractZ"); }

                outgoingMessagesList.Add(setAxisString);

                _serialCommunicationOutgoingMessagesModel.QueueNextProspectiveOutgoingMessage(outgoingMessagesList);
            }
            catch (NullReferenceException e)
            {
                _errorListViewModel.AddError("", "Actuator needs to be set in Printer Settings before setting the actuator in Control Menu.");
            }
            catch
            {
                _errorListViewModel.AddError("", "Unknown error while setting valve printhead.");
            }
        }

        /// <summary>
        /// Takes manual input parameters for a set motorized printhead command and outputs the command to the serial port.
        /// </summary>
        /// <param name="printheadName"></param>
        /// <param name="maxSpeed"></param>
        /// <param name="acceleration"></param>
        public void ProcessManualSetMotorizedPrintheadCommand(string printheadName, double maxSpeed, double acceleration)
        {
            try
            {
                List<string> outgoingMessagesList = new List<string>();

                PrintheadModel printheadModel = _printerModel.FindPrinthead(printheadName);
                MotorizedPrintheadTypeModel motorizedPrintheadTypeModel = (MotorizedPrintheadTypeModel)printheadModel.PrintheadTypeModel;

                int printheadLimitPinID = (motorizedPrintheadTypeModel.AttachedLimitSwitchGPIOPinModel == null) ? GlobalValues.PinIDNull : motorizedPrintheadTypeModel.AttachedLimitSwitchGPIOPinModel.PinID;
                string setPrintheadString = _writeSetPrintheadModel.WriteSetMotorDrivenPrinthead(motorizedPrintheadTypeModel.AttachedMotorStepGPIOPinModel.PinID, motorizedPrintheadTypeModel.AttachedMotorDirectionGPIOPinModel.PinID,
                    motorizedPrintheadTypeModel.StepPulseTime, printheadLimitPinID, maxSpeed, acceleration, motorizedPrintheadTypeModel.MmPerStep);
                outgoingMessagesList.Add(setPrintheadString);

                //Send GCode for the Z Axis attached to the Printhead.
                AxisModel axisModel = printheadModel.AttachedZAxisModel;

                int zAxisLimitPinID = (axisModel.AttachedLimitSwitchGPIOPinModel == null) ? GlobalValues.PinIDNull : axisModel.AttachedLimitSwitchGPIOPinModel.PinID;
                string setAxisString = _writeSetAxisModel.WriteSetAxis(axisModel.AxisID, axisModel.AttachedMotorStepGPIOPinModel.PinID, axisModel.AttachedMotorDirectionGPIOPinModel.PinID,
                    axisModel.StepPulseTime, zAxisLimitPinID, axisModel.MaxSpeed, axisModel.MaxAcceleration, axisModel.MmPerStep);

                //Retract Z Axis before changing Printheads.
                outgoingMessagesList.Add(SerialMessageCharacters.SerialCommandSetCharacter + "RetractZ");
                outgoingMessagesList.Add(setAxisString);
                _serialCommunicationOutgoingMessagesModel.QueueNextProspectiveOutgoingMessage(outgoingMessagesList);
            }
            catch (NullReferenceException e)
            {
                _errorListViewModel.AddError("", "Printhead or z actuator needs to be set in Printer Settings before setting printhead in Control Menu.");
            }
            catch
            {
                _errorListViewModel.AddError("", "Unknown error while setting valve printhead.");
            }
        }

        /// <summary>
        /// Takes manual input parameters for a set of valve printhead command and outputs the command to the serial port.
        /// </summary>
        /// <param name="printheadName"></param>
        public void ProcessManualSetValvePrintheadCommand(string printheadName)
        {
            try
            {
                List<string> outgoingMessagesList = new List<string>();

                PrintheadModel printheadModel = _printerModel.FindPrinthead(printheadName);
                ValvePrintheadTypeModel valvePrintheadTypeModel = (ValvePrintheadTypeModel)printheadModel.PrintheadTypeModel;
                string setPrintheadString = _writeSetPrintheadModel.WriteSetValvePrinthead(valvePrintheadTypeModel.AttachedValveGPIOPinModel.PinID);
                outgoingMessagesList.Add(setPrintheadString);

                //Send GCode for the Z Axis attached to the Printhead.
                AxisModel axisModel = printheadModel.AttachedZAxisModel;
                int zAxisLimitPinID = (axisModel.AttachedLimitSwitchGPIOPinModel == null) ? GlobalValues.PinIDNull : axisModel.AttachedLimitSwitchGPIOPinModel.PinID;
                string setAxisString = _writeSetAxisModel.WriteSetAxis(axisModel.AxisID, axisModel.AttachedMotorStepGPIOPinModel.PinID, axisModel.AttachedMotorDirectionGPIOPinModel.PinID,
                    axisModel.StepPulseTime, zAxisLimitPinID, axisModel.MaxSpeed, axisModel.MaxAcceleration, axisModel.MmPerStep);

                outgoingMessagesList.Add(SerialMessageCharacters.SerialCommandSetCharacter + "RetractZ");
                outgoingMessagesList.Add(setAxisString);
                _serialCommunicationOutgoingMessagesModel.QueueNextProspectiveOutgoingMessage(outgoingMessagesList);
            }
            catch (NullReferenceException e)
            {
                _errorListViewModel.AddError("", "Printhead or z actuator needs to be set in Printer Settings before setting printhead in Control Menu.");
            }
            catch
            {
                _errorListViewModel.AddError("", "Unknown error while setting valve printhead.");
            }
        }
        #endregion
    }
}
