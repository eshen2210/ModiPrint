using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModiPrint.DataTypes.GlobalValues;

using ModiPrint.Models.PrinterModels.PrintheadModels;
using ModiPrint.Models.PrinterModels.PrintheadModels.PrintheadTypeModels;

namespace ModiPrint.Models.GCodeConverterModels.ProcessModels.WriteSetEquipmentModels
{
    /// <summary>
    /// Produces converted GCode for setting printheads.
    /// </summary>
    public class WriteSetPrintheadModel
    {
        #region Fields and Properties
        //Class containing pseudo-global parameters for the GCodeConverter that are passed around various GCodeConverter classes.
        private ParametersModel _parametersModel;
        #endregion

        #region Constructor
        public WriteSetPrintheadModel(ParametersModel ParametersModel)
        {
            _parametersModel = ParametersModel;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns converted GCode for the setting of Motor-Driven Printheads.
        /// </summary>
        /// <param name="printheadModel"></param>
        /// <returns></returns>
        public string WriteSetMotorDrivenPrinthead(PrintheadModel printheadModel)
        {
            string convertedGCode = "";

            if (printheadModel.PrintheadType == PrintheadType.Motorized)
            {
                MotorizedPrintheadTypeModel motorizedPrinthead = (MotorizedPrintheadTypeModel)printheadModel.PrintheadTypeModel;

                try
                {
                    int limitPin = GlobalValues.PinIDNull;
                    if (motorizedPrinthead.AttachedLimitSwitchGPIOPinModel != null)
                    { limitPin = motorizedPrinthead.AttachedLimitSwitchGPIOPinModel.PinID; }

                    convertedGCode = WriteSetMotorDrivenPrinthead(motorizedPrinthead.AttachedMotorStepGPIOPinModel.PinID, motorizedPrinthead.AttachedMotorDirectionGPIOPinModel.PinID, motorizedPrinthead.StepPulseTime,
                        limitPin, motorizedPrinthead.MaxSpeed, motorizedPrinthead.MaxAcceleration, motorizedPrinthead.MmPerStep);
                }
                catch when (printheadModel == null)
                {
                    _parametersModel.ErrorReporterViewModel.ReportError("GCode Converter: Printer Unset", "Printhead Not Found");
                    convertedGCode = "";
                }
                catch when (printheadModel.PrintheadTypeModel == null)
                {
                    _parametersModel.ErrorReporterViewModel.ReportError("GCode Converter: Printer Unset", printheadModel.Name + " Printhead Type Unset");
                    convertedGCode = "";
                }
                catch when (motorizedPrinthead.AttachedMotorStepGPIOPinModel == null)
                {
                    _parametersModel.ErrorReporterViewModel.ReportError("GCode Converter: Printer Unset", printheadModel.Name + " Step Pin Unset");
                    convertedGCode = "";
                }
                catch when (motorizedPrinthead.AttachedMotorDirectionGPIOPinModel == null)
                {
                    _parametersModel.ErrorReporterViewModel.ReportError("GCode Converter: Printer Unset", printheadModel.Name + " Direction Pin Unset");
                    convertedGCode = "";
                }
                catch when (motorizedPrinthead.StepPulseTime <= 0)
                {
                    _parametersModel.ErrorReporterViewModel.ReportError("GCode Converter: Printer Unset", printheadModel.Name + " Step Pulse Time Unset");
                    convertedGCode = "";
                }
                catch
                {
                    //Should never reach this point.
                    _parametersModel.ErrorReporterViewModel.ReportError("GCodeConverter", "Unspecified Error, Please Check Code");
                    convertedGCode = "";
                }
            }
            else
            {
                _parametersModel.ErrorReporterViewModel.ReportError("GCode Converter: Printhead Unset", printheadModel.Name + " Printhead Type Incorrect");
                convertedGCode = "";
            }

            return convertedGCode;
        }

        /// <summary>
        /// Returns converted GCode for the setting of Motor-Driven Printheads.
        /// </summary>
        /// <param name="stepPin"></param>
        /// <param name="directionPin"></param>
        /// <param name="stepPulseTime"></param>
        /// <param name="limitPin"></param>
        /// <param name="maxSpeed"></param>
        /// <param name="acceleration"></param>
        /// <param name="mmPerStep"></param>
        /// <returns></returns>
        public string WriteSetMotorDrivenPrinthead(int stepPin, int directionPin, int stepPulseTime, int limitPin, double maxSpeed, double acceleration, double mmPerStep)
        {
            string convertedGCode = SerialCommands.SetMotorPrinthead;

            //Step pin.
            convertedGCode += " T" + stepPin;

            //Direction pin.
            convertedGCode += " D" + directionPin;

            //Step Pulse Time.
            convertedGCode += " P" + stepPulseTime;

            //Limit Switch pin.
            convertedGCode += " L" + limitPin;

            //Maximum Speed and Maximum Acceleration.
            convertedGCode += " S" + (int)(maxSpeed / mmPerStep);
            convertedGCode += " A" + (int)(acceleration / mmPerStep);

            return convertedGCode;
        }

        /// <summary>
        /// Returns converted GCode for the setting of Valve Printheads.
        /// </summary>
        /// <returns></returns>
        public string WriteSetValvePrinthead(PrintheadModel printheadModel)
        {
            string convertedGCode = "";

            if (printheadModel.PrintheadType == PrintheadType.Valve)
            {
                ValvePrintheadTypeModel valvePrinthead = (ValvePrintheadTypeModel)printheadModel.PrintheadTypeModel;

                try
                {
                    convertedGCode = WriteSetValvePrinthead(valvePrinthead.AttachedValveGPIOPinModel.PinID);
                }
                catch when (printheadModel == null)
                {
                    _parametersModel.ErrorReporterViewModel.ReportError("GCode Converter: Printer Unset", "Printhead Not Found");
                    convertedGCode = "";
                }
                catch when (printheadModel.PrintheadTypeModel == null)
                {
                    _parametersModel.ErrorReporterViewModel.ReportError("GCode Converter: Printer Unset", printheadModel.Name + " Printhead Type Unset");
                    convertedGCode = "";
                }
                catch when (valvePrinthead.AttachedValveGPIOPinModel == null)
                {
                    _parametersModel.ErrorReporterViewModel.ReportError("GCode Converter: Printhead Type Unset", printheadModel.Name + " Valve Pin Unset");
                    convertedGCode = "";
                }
                catch
                {
                    //Should never reach this point.
                    _parametersModel.ErrorReporterViewModel.ReportError("GCodeConverter", "Unspecified Error, Please Check Code");
                    convertedGCode = "";
                }

            }
            else
            {
                _parametersModel.ErrorReporterViewModel.ReportError("GCode Converter: Printhead Unset", printheadModel.Name + " Printhead Type Incorrect");
                convertedGCode = "";
            }

            return convertedGCode;
        }

        /// <summary>
        /// Returns converted GCode for the setting of Valve Printheads.
        /// </summary>
        /// <param name="valvePin"></param>
        /// <returns></returns>
        public string WriteSetValvePrinthead(int valvePin)
        {
            string convertedGCode = SerialCommands.SetValvePrinthead;

            //Valve pin.
            convertedGCode += " V" + valvePin;

            return convertedGCode;
        }

        #endregion
    }
}
