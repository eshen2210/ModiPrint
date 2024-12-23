using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModiPrint.DataTypes.GlobalValues;
using ModiPrint.Models.PrinterModels.AxisModels;
using ModiPrint.Models.GCodeConverterModels;

namespace ModiPrint.Models.GCodeConverterModels.ProcessModels.WriteSetEquipmentModels
{
    /// <summary>
    /// Produces converted GCode for setting Axes.
    /// </summary>
    public class WriteSetAxisModel
    {
        #region Fields and Properties
        //Contains pseudo-global parameters for the GCodeConverter that are passed around various GCodeConverter classes.
        ParametersModel _parametersModel;
        #endregion

        #region Constructor
        public WriteSetAxisModel(ParametersModel ParametersModel)
        {
            _parametersModel = ParametersModel;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns converted GCode for the setting of an Axis. Sets the highest Max Speed and Acceleration values.
        /// </summary>
        /// <param name="axisModel"></param>
        /// <returns></returns>
        public string WriteSetAxis(AxisModel axisModel)
        {
            string convertedGCode = "";

            try
            {
                int limitPinID = (axisModel.AttachedLimitSwitchGPIOPinModel == null) ? GlobalValues.PinIDNull : axisModel.AttachedLimitSwitchGPIOPinModel.PinID;

                convertedGCode = WriteSetAxis(axisModel.AxisID, axisModel.AttachedMotorStepGPIOPinModel.PinID, axisModel.AttachedMotorDirectionGPIOPinModel.PinID,
                    axisModel.StepPulseTime, limitPinID, axisModel.MaxSpeed, axisModel.MaxAcceleration, axisModel.MmPerStep);
            }
            catch when (axisModel == null) //Axis unset.
            {
                //Catching and error handling should have happened earlier.
                convertedGCode = "";
            }
            catch when (axisModel.AttachedMotorStepGPIOPinModel == null) //Motor Step pin unset.
            {
                _parametersModel.ErrorReporterViewModel.ReportError("GCode Converter: Printer Unset", axisModel.AxisID + " Axis" + " Step Pin Unset");
                convertedGCode = "";
            }
            catch when (axisModel.AttachedMotorDirectionGPIOPinModel == null) //Motor Direction pin unset.
            {
                _parametersModel.ErrorReporterViewModel.ReportError("GCode Converter: Printer Unset", axisModel.AxisID + " Axis" + " Direction Pin Unset");
                convertedGCode = "";
            }
            catch when (axisModel.StepPulseTime <= 0)
            {
                _parametersModel.ErrorReporterViewModel.ReportError("GCode Converter: Printer Unset", axisModel.AxisID + " Axis" + " Step Pulse Time Unset");
                convertedGCode = "";
            }
            catch
            {
                //Should never reach this point.
                _parametersModel.ErrorReporterViewModel.ReportError("GCodeConverter", "Unspecified Error, Please Check Code");
                convertedGCode = "";
            }

            return convertedGCode;
        }


        /// <summary>
        /// Returns converted GCode for the setting of an Axis.
        /// </summary>
        /// <param name="axisID"></param>
        /// <param name="stepPin"></param>
        /// <param name="directionPin"></param>
        /// <param name="stepPulseTime"></param>
        /// <param name="limitPin"></param>
        /// <param name="maxSpeed"></param>
        /// <param name="acceleration"></param>
        /// <param name="mmPerStep"></param>
        /// <returns></returns>
        public string WriteSetAxis(char axisID, int stepPin, int directionPin, int stepPulseTime, int limitPin, double maxSpeed, double acceleration, double mmPerStep)
        {
            string convertedGCode = "";
            
            //Command.
            convertedGCode += SerialCommands.SetAxis;
            convertedGCode += " " + axisID;

            //Motor Step and Motor Direction pin.
            convertedGCode += " T" + stepPin;
            convertedGCode += " D" + directionPin;

            //Step Pulse Time.
            convertedGCode += " P" + stepPulseTime;

            //Limit Switch Pin.
            convertedGCode += " L" + limitPin;

            //Max Speed and Max Acceleration.
            convertedGCode += " S" + (int)(maxSpeed / mmPerStep);
            convertedGCode += " A" + (int)(acceleration / mmPerStep);

            return convertedGCode;
        }
        #endregion
    }
}
