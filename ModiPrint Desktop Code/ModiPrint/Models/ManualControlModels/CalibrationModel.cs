using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using ModiPrint.DataTypes.GlobalValues;
using ModiPrint.Models.PrinterModels;
using ModiPrint.Models.PrinterModels.AxisModels;
using ModiPrint.Models.PrinterModels.PrintheadModels;
using ModiPrint.Models.RealTimeStatusModels;
using ModiPrint.Models.SerialCommunicationModels;
using ModiPrint.Models.GCodeConverterModels.ProcessModels.WriteSetEquipmentModels;
using ModiPrint.Models.GCodeConverterModels;
using ModiPrint.Models.GCodeConverterModels.ProcessModels.ProcessG00Models;
using ModiPrint.ViewModels;

namespace ModiPrint.Models.ManualControlModels
{
    //Fires when a calibration sequence has begun.
    public delegate void CalibrationBegunEventHandler();
    
    /// <summary>
    /// Contains functions that automatically calibrate the Printer's position and range.
    /// </summary>
    public class CalibrationModel
    {
        #region Fields and Properties
        //Contains information on the Printer during operation.
        RealTimeStatusDataModel _realTimeStatusDataModel;

        //Contains information on the Printer.
        PrinterModel _printerModel;

        //Contains functions to output commands.
        WriteSetAxisModel _writeSetAxisModel;

        //Contains functions to queue outgoing messages for the serial port.
        SerialCommunicationOutgoingMessagesModel _serialCommunicationOutgoingMessagesModel;

        //Reports errors.
        ErrorListViewModel _errorListViewModel;
        #endregion

        #region Constructor
        public CalibrationModel(RealTimeStatusDataModel RealTimeStatusDataModel, PrinterModel PrinterModel, SerialCommunicationOutgoingMessagesModel SerialCommunicationOutgoingMessagesModel, ErrorListViewModel ErrorListViewModel)
        {
            _realTimeStatusDataModel = RealTimeStatusDataModel;
            _printerModel = PrinterModel;
            _serialCommunicationOutgoingMessagesModel = SerialCommunicationOutgoingMessagesModel;

            ParametersModel parametersModel = new ParametersModel(_printerModel, null);
            _writeSetAxisModel = new WriteSetAxisModel(parametersModel);

            _errorListViewModel = ErrorListViewModel;
        }
        #endregion

        #region Events
        //Fires when a calibration sequence has begun.
        public CalibrationBegunEventHandler CalibrationBegun;
        private void OnCalibrationBegun()
        {
            if (CalibrationBegun != null)
            { CalibrationBegun(); }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sends outgoing commands that retracts all Z Axes and moves X and Y Axes to the limit switches.
        /// </summary>
        public void Home(double xCalibrationSpeed, double yCalibrationSpeed, double zCalibrationSpeed, double xDistanceFromCenter, double yDistanceFromCenter)
        {
            try
            {
                //Retract Z Axes.
                RetractAllZ(zCalibrationSpeed);

                //Z Axes should be in default positions.
                AxisModel zAxisModel = _printerModel.ZAxisModelList[_printerModel.ZAxisModelList.Count - 1];
                double zPosition = zAxisModel.MaxPosition - GlobalValues.LimitBuffer;
                _serialCommunicationOutgoingMessagesModel.AppendProspectiveOutgoingMessage(SerialMessageCharacters.SerialCommandSetCharacter + "SetMinMaxPos " + "Z" + zPosition);

                //Set X Axis to calibration speeds.
                AxisModel xAxis = _printerModel.AxisModelList[0];
                int xLimitPinID = (xAxis.AttachedLimitSwitchGPIOPinModel == null) ? GlobalValues.PinIDNull : xAxis.AttachedLimitSwitchGPIOPinModel.PinID;
                string switchX = _writeSetAxisModel.WriteSetAxis(xAxis.AxisID, xAxis.AttachedMotorStepGPIOPinModel.PinID, xAxis.AttachedMotorDirectionGPIOPinModel.PinID, xAxis.StepPulseTime,
                    xLimitPinID, xCalibrationSpeed, xAxis.MaxAcceleration, xAxis.MmPerStep);
                _serialCommunicationOutgoingMessagesModel.AppendProspectiveOutgoingMessage(switchX);

                //Set Y Axis to max speeds.
                AxisModel yAxis = _printerModel.AxisModelList[1];
                int yLimitPinID = (yAxis.AttachedLimitSwitchGPIOPinModel == null) ? GlobalValues.PinIDNull : yAxis.AttachedLimitSwitchGPIOPinModel.PinID;
                string switchY = _writeSetAxisModel.WriteSetAxis(yAxis.AxisID, yAxis.AttachedMotorStepGPIOPinModel.PinID, yAxis.AttachedMotorDirectionGPIOPinModel.PinID, yAxis.StepPulseTime,
                    yLimitPinID, yCalibrationSpeed, yAxis.MaxAcceleration, yAxis.MmPerStep);
                _serialCommunicationOutgoingMessagesModel.AppendProspectiveOutgoingMessage(switchY);

                //Hit the min and max limit switches on X.
                double unused = 0;
                string xPositive = GCodeLinesConverter.GCodeLinesListToString(
                    WriteG00.WriteAxesMovement(
                    xAxis.MmPerStep, 0, 0,
                    5000, 0, 0,
                    xAxis.IsDirectionInverted, false, false,
                    ref unused, ref unused, ref unused));
                string xNegative = GCodeLinesConverter.GCodeLinesListToString(
                    WriteG00.WriteAxesMovement(
                    xAxis.MmPerStep, 0, 0,
                    -5000, 0, 0,
                    xAxis.IsDirectionInverted, false, false,
                    ref unused, ref unused, ref unused));
                _serialCommunicationOutgoingMessagesModel.AppendProspectiveOutgoingMessage(xPositive);
                _serialCommunicationOutgoingMessagesModel.AppendProspectiveOutgoingMessage(xNegative);

                //Move away from the limit switch.
                string xMoveAwayFromLimit = GCodeLinesConverter.GCodeLinesListToString(
                    WriteG00.WriteAxesMovement(
                    xAxis.MmPerStep, 0, 0,
                    GlobalValues.LimitBuffer, 0, 0,
                    xAxis.IsDirectionInverted, false, false,
                    ref unused, ref unused, ref unused));
                _serialCommunicationOutgoingMessagesModel.AppendProspectiveOutgoingMessage(xMoveAwayFromLimit);

                //Hit the min and max limit switches on Y.
                string yPositive = GCodeLinesConverter.GCodeLinesListToString(
                    WriteG00.WriteAxesMovement(
                    0, yAxis.MmPerStep, 0,
                    0, 5000, 0,
                    yAxis.IsDirectionInverted, false, false,
                    ref unused, ref unused, ref unused));
                string yNegative = GCodeLinesConverter.GCodeLinesListToString(
                    WriteG00.WriteAxesMovement(
                    0, yAxis.MmPerStep, 0,
                    0, -5000, 0,
                    yAxis.IsDirectionInverted, false, false,
                    ref unused, ref unused, ref unused));
                _serialCommunicationOutgoingMessagesModel.AppendProspectiveOutgoingMessage(yPositive);
                _serialCommunicationOutgoingMessagesModel.AppendProspectiveOutgoingMessage(yNegative);

                //Move away from the limit switch.
                string yMoveAwayFromLimit = GCodeLinesConverter.GCodeLinesListToString(
                    WriteG00.WriteAxesMovement(
                    0, yAxis.MmPerStep, 0,
                    0, GlobalValues.LimitBuffer, 0,
                    yAxis.IsDirectionInverted, false, false,
                    ref unused, ref unused, ref unused));
                _serialCommunicationOutgoingMessagesModel.AppendProspectiveOutgoingMessage(yMoveAwayFromLimit);

                //Set X Axis to max speeds.
                string switchXMax = _writeSetAxisModel.WriteSetAxis(xAxis);
                _serialCommunicationOutgoingMessagesModel.AppendProspectiveOutgoingMessage(switchXMax);

                //Set Y Axis to max speeds.
                string switchYMax = _writeSetAxisModel.WriteSetAxis(yAxis);
                _serialCommunicationOutgoingMessagesModel.AppendProspectiveOutgoingMessage(switchYMax);

                //Center the X and Y actuators.
                _serialCommunicationOutgoingMessagesModel.AppendProspectiveOutgoingMessage(SerialMessageCharacters.SerialCommandSetCharacter + "Center X" + xDistanceFromCenter + " Y" + yDistanceFromCenter);
                _serialCommunicationOutgoingMessagesModel.AppendProspectiveOutgoingMessage(SerialMessageCharacters.SerialCommandSetCharacter + "OriginXY");

                //At the end, switch the Z actuator to the Printhead used at the beginning of the print.
                if (_realTimeStatusDataModel.ZRealTimeStatusAxisModel.Name != "Unset")
                {
                    AxisModel zAxisFinalModel = _printerModel.FindAxis(_realTimeStatusDataModel.ZRealTimeStatusAxisModel.Name);
                    int zFinalLimitPinID = (zAxisFinalModel.AttachedLimitSwitchGPIOPinModel == null) ? GlobalValues.PinIDNull : zAxisFinalModel.AttachedLimitSwitchGPIOPinModel.PinID;
                    string switchZFinal = _writeSetAxisModel.WriteSetAxis(zAxisFinalModel.AxisID, zAxisFinalModel.AttachedMotorStepGPIOPinModel.PinID, zAxisFinalModel.AttachedMotorDirectionGPIOPinModel.PinID,
                            zAxisFinalModel.StepPulseTime, zFinalLimitPinID, zCalibrationSpeed, zAxisFinalModel.MaxAcceleration, zAxisFinalModel.MmPerStep);
                    _serialCommunicationOutgoingMessagesModel.AppendProspectiveOutgoingMessage(switchZFinal);
                }

                OnCalibrationBegun();
            }
            catch
            {
                _errorListViewModel.AddError("", "Unkown error when homing");
            }
        }

        /// <summary>
        /// Sends outgoing commands that mark the position of the currently active Printhead's point to printing (centered and closest to the print surface).
        /// Should only be called after Z actuators have hit their top limit switches this session.
        /// All Printheads should be calibrated within a single session as X and Y Offset positions are relative.
        /// Z Offset is simply the zero position of the Z actuator.
        /// </summary>
        public void CalibratePrintheadOffset()
        {
            try
            {
                //Set Offset values for the current Printhead.
                PrintheadModel printheadModel = _printerModel.FindPrinthead(_realTimeStatusDataModel.ActivePrintheadModel.Name);
                printheadModel.XOffset = _realTimeStatusDataModel.XRealTimeStatusAxisModel.Position;
                printheadModel.YOffset = _realTimeStatusDataModel.YRealTimeStatusAxisModel.Position;

                //Set current Z Axis MinPosition to 0.
                string setZMin = SerialMessageCharacters.SerialCommandSetCharacter + "SetMinMaxPos ZN0";
                _serialCommunicationOutgoingMessagesModel.AppendProspectiveOutgoingMessage(setZMin);

                //Maximize Z Axis speeds.
                string setZMaxSpeed = _writeSetAxisModel.WriteSetAxis(printheadModel.AttachedZAxisModel);
                _serialCommunicationOutgoingMessagesModel.AppendProspectiveOutgoingMessage(setZMaxSpeed);

                //The next Z Limit Switch hit will calibrate minmax positions.
                _realTimeStatusDataModel.ShouldZCalibrate = true;

                //Retracts the Z Axis up to a short distance away from the Limit Switch (does not hit the Limit Switch).
                string retractZ = SerialMessageCharacters.SerialCommandSetCharacter + "RetractZLimit";
                _serialCommunicationOutgoingMessagesModel.AppendProspectiveOutgoingMessage(retractZ);

                OnCalibrationBegun();
            }
            catch when (_realTimeStatusDataModel.ActivePrintheadModel == null)
            {
                _errorListViewModel.AddError("", "No printhead is set. Printhead must be set before calibrating printhead offset.");
            }
            catch
            {
                _errorListViewModel.AddError("", "Unkown error when calibrating printhead offset.");
            }
        }

        /// <summary>
        /// Sends outgoing commands that retracts all Z Axes.
        /// </summary>
        private void RetractAllZ(double zCalibrationSpeed)
        {
            foreach (AxisModel zAxis in _printerModel.ZAxisModelList)
            {
                //Switch to another Z Axis, hit the upper Limit Switch, and then move to default position.
                int zLimitPinID = (zAxis.AttachedLimitSwitchGPIOPinModel == null) ? GlobalValues.PinIDNull : zAxis.AttachedLimitSwitchGPIOPinModel.PinID;
                string switchZ = _writeSetAxisModel.WriteSetAxis(zAxis.AxisID, zAxis.AttachedMotorStepGPIOPinModel.PinID, zAxis.AttachedMotorDirectionGPIOPinModel.PinID, zAxis.StepPulseTime,
                    zLimitPinID, zCalibrationSpeed, zAxis.MaxAcceleration, zAxis.MmPerStep);
                _serialCommunicationOutgoingMessagesModel.AppendProspectiveOutgoingMessage(switchZ);
                _serialCommunicationOutgoingMessagesModel.AppendProspectiveOutgoingMessage(SerialMessageCharacters.SerialCommandSetCharacter + "RetractZLimit");
            }
        }
        #endregion
    }
}
