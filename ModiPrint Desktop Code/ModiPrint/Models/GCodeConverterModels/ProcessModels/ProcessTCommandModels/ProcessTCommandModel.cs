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
using ModiPrint.Models.PrintModels;
using ModiPrint.Models.PrintModels.MaterialModels;
using ModiPrint.Models.PrintModels.PrintStyleModels;
using ModiPrint.Models.RealTimeStatusModels;
using ModiPrint.Models.GCodeConverterModels.ProcessModels.WriteSetEquipmentModels;
using ModiPrint.Models.GCodeConverterModels.ProcessModels.ProcessG00Models;

namespace ModiPrint.Models.GCodeConverterModels.ProcessModels.ProcessTCommandModels

{
    public class ProcessTCommandModel
    {
        #region Fields and Properties
        //The GCode Converter will take print parameters from this object.
        private PrintModel _printModel;
        private PrinterModel _printerModel;
        private RealTimeStatusDataModel _realTimeStatusDataModel;

        //Class containing pseudo-global parameters for the GCodeConverter that are passed around various GCodeConverter classes.
        private ParametersModel _parametersModel;

        //Class that calls the appropriate WritePrinthead and returns the string output.
        private SetWritePrintheadModel _setWritePrintheadModel;
        private WriteSetAxisModel _writeSetAxisModel;
        #endregion

        #region Constructor
        public ProcessTCommandModel(PrintModel PrintModel, PrinterModel PrinterModel, RealTimeStatusDataModel RealTimeStatusDataModel, ParametersModel ParametersModel)
        {
            _printModel = PrintModel;
            _printerModel = PrinterModel;
            _realTimeStatusDataModel = RealTimeStatusDataModel;

            _parametersModel = ParametersModel;

            _setWritePrintheadModel = new SetWritePrintheadModel(_parametersModel);
            _writeSetAxisModel = new WriteSetAxisModel(_parametersModel);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Processes the command of setting a printhead.
        /// </summary>
        public List<ConvertedGCodeLine> ProcessTCommand(string[] repRapLine, ref MaterialModel currentMaterial)
        {
            AxisModel xAxis = _printerModel.AxisModelList[0];
            AxisModel yAxis = _printerModel.AxisModelList[1];
            AxisModel zAxisCurrent = _printerModel.FindAxis(currentMaterial.PrintheadModel.AttachedZAxisModel.Name);

            //The return GCode.
            List<ConvertedGCodeLine> convertedGCodeLinesList = new List<ConvertedGCodeLine>();

            //Finds the material that matches the TCommand.
            string repRapTCommand = repRapLine[0];
            MaterialModel matchingMaterial = _printModel.FindMaterial(repRapTCommand);

            //Appends the converted GCode line with a command that sets the Printhead.
            //Also appends the converted GCode line with a command that sets the new Z Axis associated with the new Printhead.
            //Also appends the converted GCode line with commands that compensate for the new Printhead's offsets.
            try  
            {
                //If the current Material is using a Motorized Printhead, retract it before switching Printheads.
                if (((currentMaterial.Name != "Unset") //If this is not the first Material
                  && (currentMaterial.PrintheadModel.PrintheadType == PrintheadType.Motorized)
                  && (currentMaterial.PrintheadModel.Name != matchingMaterial.PrintheadModel.Name) //Do not execute retraction-related actions if no Printhead change occurs
                  && (_parametersModel.IsRetracted == false)))
                {
                    MotorizedPrintheadTypeModel motorizedPrintheadTypeModel = (MotorizedPrintheadTypeModel)currentMaterial.PrintheadModel.PrintheadTypeModel;
                    ContinuousPrintStyleModel continuousPrintheadStyleModel = (ContinuousPrintStyleModel)currentMaterial.PrintStyleModel;
                    int eModiPrintCoordIndex = _parametersModel.FindEModiPrintCoordIndex(currentMaterial.PrintheadModel);
                    convertedGCodeLinesList = WriteG00.WriteMotorizedPrintWithoutMovement(-1 * continuousPrintheadStyleModel.MotorizedDispenseRetractionDistance, motorizedPrintheadTypeModel.MmPerStep,
                        motorizedPrintheadTypeModel.IsDirectionInverted, ref _parametersModel.ERepRapCoord.DeltaCoordRemainder, _parametersModel.EModiPrintCoordList[eModiPrintCoordIndex]);
                    _parametersModel.IsRetracted = true;
                }

                //If a new Motorized Printhead is being set...
                //Finds the EModiPrintCoord corresponding to the given Printhead and sets the Minimum and Maximum Position values of the EModiPrintCoord.
                if (matchingMaterial.PrintheadModel.PrintheadType == PrintheadType.Motorized)
                {
                    int eModiPrintCoordIndex = _parametersModel.FindEModiPrintCoordIndex(matchingMaterial.PrintheadModel);
                    MotorizedPrintheadTypeModel motorizedPrintheadTypeModel = (MotorizedPrintheadTypeModel)matchingMaterial.PrintheadModel.PrintheadTypeModel;
                    ContinuousPrintStyleModel continuousPrintheadStyleModel = (ContinuousPrintStyleModel)matchingMaterial.PrintStyleModel;

                    //Sets new Min and Max Positions.
                    _parametersModel.EModiPrintCoordList[eModiPrintCoordIndex].MinPosition = motorizedPrintheadTypeModel.MinPosition;
                    _parametersModel.EModiPrintCoordList[eModiPrintCoordIndex].MaxPosition = motorizedPrintheadTypeModel.MaxPosition;

                    //Sets the starting position of the new Motorized Printhead.
                    //The starting position is the retracted state.
                    //If this is the first Material, then retraction needs to occur to reach the starting retracted state.
                    if (currentMaterial.Name == "Unset") //If this is the first Material.
                    {
                        //Retraction needs to occur to reach the starting retracted state.
                        List<ConvertedGCodeLine> retractE = WriteG00.WriteMotorizedPrintWithoutMovement(-1 * continuousPrintheadStyleModel.MotorizedDispenseRetractionDistance, motorizedPrintheadTypeModel.MmPerStep,
                            motorizedPrintheadTypeModel.IsDirectionInverted, ref _parametersModel.ERepRapCoord.DeltaCoordRemainder, _parametersModel.EModiPrintCoordList[eModiPrintCoordIndex]);
                        if (retractE != null)
                        {
                            convertedGCodeLinesList.AddRange(retractE);
                        }

                        _parametersModel.ERepRapCoord.SetInitialCoord(continuousPrintheadStyleModel.MotorizedDispenseRetractionDistance);
                        _parametersModel.IsRetracted = true;
                    }

                    //If this is a new Motorized Printhead, then it is by default, retracted.
                    if (currentMaterial.PrintheadModel.Name != matchingMaterial.PrintheadModel.Name)
                    {
                        _parametersModel.ERepRapCoord.SetInitialCoord(continuousPrintheadStyleModel.MotorizedDispenseRetractionDistance);
                        _parametersModel.IsRetracted = true;
                    }
                }
                else
                {
                    _parametersModel.IsRetracted = false;
                }

                //Set new Min and Max Positions for the X and Y Axes.
                //This is only relevant when the first Material is set.
                _parametersModel.SetNewXYZCoord('X', _parametersModel.XCoord.CurrentCoord, _printerModel.AxisModelList[0].MinPosition, _printerModel.AxisModelList[0].MaxPosition);
                _parametersModel.SetNewXYZCoord('Y', _parametersModel.YCoord.CurrentCoord, _printerModel.AxisModelList[1].MinPosition, _printerModel.AxisModelList[1].MaxPosition);

                //If the Z Axis does not need to be changed, then keep the same Min and Max Positions in the ZCoord.
                //If the Z Axis was switched, then set a new ZCoord Position assuming the Z Axis started from a retracted position and traversed its Offset.
                if (matchingMaterial.PrintheadModel.AttachedZAxisModel.Name != matchingMaterial.PrintheadModel.AttachedZAxisModel.Name)
                {
                    AxisModel zAxisModel = _printerModel.FindAxis(matchingMaterial.PrintheadModel.AttachedZAxisModel.Name);
                    double newZPosition = zAxisModel.MaxPosition - GlobalValues.LimitBuffer; 
                    _parametersModel.SetNewXYZCoord('Z', newZPosition, zAxisModel.MinPosition, zAxisModel.MaxPosition);
                }

                //Set new Droplet parameters if applicable.
                if (!((currentMaterial.Name == "Unset") || (currentMaterial == null)) //If the current Material is set to Droplet... 
                 && (currentMaterial.PrintStyle == PrintStyle.Droplet))
                {
                    //Execute movement for the remaining movement left in droplet movements.
                    MaterialModel newMaterialParameter = (matchingMaterial.PrintStyle == PrintStyle.Droplet) ? matchingMaterial : null;
                    double[] remainingDropletMovementArr = _parametersModel.ResetDropletPrintParameters(currentMaterial, newMaterialParameter);
                    double unused = 0;
                    List<ConvertedGCodeLine> remainingDropletMovement = WriteG00.WriteAxesMovement(
                        xAxis.MmPerStep, yAxis.MmPerStep, zAxisCurrent.MmPerStep,
                        remainingDropletMovementArr[0], remainingDropletMovementArr[1], remainingDropletMovementArr[2],
                        xAxis.IsDirectionInverted, yAxis.IsDirectionInverted, zAxisCurrent.IsDirectionInverted,
                        ref unused, ref unused, ref unused);
                    if (remainingDropletMovement != null)
                    { convertedGCodeLinesList.AddRange(remainingDropletMovement); }
                }
                else if (matchingMaterial.PrintStyle == PrintStyle.Droplet) //If the current Material is irrelevant and the new Material is set to Droplet...
                {
                    _parametersModel.ResetDropletPrintParameters(null, matchingMaterial);
                }

                //Set the new Material.
                currentMaterial = matchingMaterial;

                //Command Set for switching Materials.
                //This Command Set will convert to the commands for retracting the Z Axis, switching Printheads, moving Offsets, and setting new movement speeds.
                string convertedGCodeLine = SerialMessageCharacters.SerialCommandSetCharacter + "SwitchMaterial ";

                //If applicable, pause the print sequence before switching to the next Material.
                if ((currentMaterial.Name != "Unset") && (currentMaterial.PauseBeforeDeactivating == true))
                {
                    convertedGCodeLine += "D";
                }

                //If applicable pause the print sequence after switching printheads.
                if (matchingMaterial.PauseAfterActivating == true)
                {
                    convertedGCodeLine += "A";
                }

                if (convertedGCodeLine[convertedGCodeLine.Length - 1] != ' ')
                { convertedGCodeLine += " "; }
                convertedGCodeLine += '"' + matchingMaterial.Name + '"';
                convertedGCodeLinesList.Add(new ConvertedGCodeLine(convertedGCodeLine));
            }
            catch when ((matchingMaterial == null) || (currentMaterial == null)) //Catch unset Material.
            {
                //Catching and error reporting should have happened earlier.
                _parametersModel.ErrorReporterViewModel.ReportError("G-Code Conversion Failed: Print Settings Incorrectly Set, Should Not Happen", "Material Null");
                return null;
            }
            catch when (matchingMaterial == null)
            {
                _parametersModel.ErrorReporterViewModel.ReportError("G-Code Conversion Failed: Print Settings Incorrectly Set, No Matching G-Code ID Found: ", "G-Code Command: " + repRapTCommand);
            }
            catch when (matchingMaterial.PrintheadModel == null) //Catch unset Printhead.
            {
                _parametersModel.ErrorReporterViewModel.ReportError("G-Code Conversion Failed: Print Settings Incorrectly Set, Printhead Not Set In Material Settings ", "G-Code Command: " + matchingMaterial.RepRapID);
                return null;
            }
            catch when (currentMaterial.PrintheadModel == null) //Catch unset Printhead.
            {
                _parametersModel.ErrorReporterViewModel.ReportError("G-Code Conversion Failed: Printhead Settings Incorrectly Set In Material Settings ", "G-Code Command: " + currentMaterial.RepRapID);
                return null;
            }
            catch when (matchingMaterial.PrintheadModel.AttachedZAxisModel == null) //Catch unset Z Axis.
            {
                _parametersModel.ErrorReporterViewModel.ReportError("G-Code Conversion Failed: Printer Settings For Z Actuator Parameter ", "Printhead Name: " + matchingMaterial.PrintheadModel.Name);
                return null;
            }
            catch
            {
                //Should never reach this point.
                _parametersModel.ErrorReporterViewModel.ReportError("G-Code Conversion Failed: Should Not Happen, Please Contact The Developer To Report This Error", "Unspecified Error");
            }

            return convertedGCodeLinesList;
        }
        #endregion
    }
}
