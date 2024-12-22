using System;
using System.Windows;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using ModiPrint.DataTypes.GlobalValues;
using ModiPrint.ViewModels;
using ModiPrint.Models.PrinterModels;
using ModiPrint.Models.PrinterModels.AxisModels;
using ModiPrint.Models.PrinterModels.PrintheadModels;
using ModiPrint.Models.PrintModels;
using ModiPrint.Models.PrintModels.MaterialModels;
using ModiPrint.Models.RealTimeStatusModels;
using ModiPrint.Models.GCodeConverterModels.ProcessModels;
using ModiPrint.Models.GCodeConverterModels.ProcessModels.ProcessG00Models;
using ModiPrint.Models.GCodeConverterModels.ProcessModels.ProcessMiscModels;
using ModiPrint.Models.GCodeConverterModels.ProcessModels.ProcessTCommandModels;
using ModiPrint.Models.GCodeConverterModels.ProcessModels.WriteSetEquipmentModels;
using ModiPrint.Models.GCodeConverterModels.CorneringModels;

namespace ModiPrint.Models.GCodeConverterModels
{
    //Event handler for updating conversion progress of the GCodeConverter.
    public delegate void GCodeConverterLineConvertedEventHandler(object sender, string progress);

    /// <summary>
    /// The master class for the GCodeConverter application logic.
    /// </summary>
    public class GCodeConverterModel
    {        
        #region Fields and Properties
        //The GCode Converter will take Print parameters from these objects.
        private PrinterModel _printerModel;
        private PrintModel _printModel;
        private RealTimeStatusDataModel _realTimeStatusDataModel;

        //Classes that output converted GCode.
        private WriteSetAxisModel _writeSetAxisModel;
        private CorneringModel _corneringModel;

        //Class associated with reporting GCodeConverter information to the GUI.
        private ErrorListViewModel _errorListViewModel;

        //Class containing pseudo-global parameters for the GCodeConverter that are passed around various GCodeConverter classes.
        private ParametersModel _parametersModel;
        public ParametersModel ParametersModel
        {
            get { return _parametersModel; }
        }

        //Classes associated with GCodeConverter-specific functions.
        private ProcessG00CommandModel _processG00CommandModel;
        private ProcessG92CommandModel _processG92CommandModel;
        private ProcessTCommandModel _processTCommandModel;

        //The GCode Converter is currently processing for which Material?
        //The GCode Converter will read parameters from this Material.
        private MaterialModel _currentMaterial;
        #endregion

        #region Constructors
        public GCodeConverterModel(PrinterModel PrinterModel, PrintModel PrintModel, RealTimeStatusDataModel RealTimeStatusDataModel, ErrorListViewModel ErrorListViewModel)
        {
            _printerModel = PrinterModel;
            _printModel = PrintModel;
            _realTimeStatusDataModel = RealTimeStatusDataModel;
            _errorListViewModel = ErrorListViewModel;

            InstantiateGCodeConverterClasses();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Create new classes for the g-code converter.
        /// Place in the constructor and at the start of g-code conversion.
        /// </summary>
        private void InstantiateGCodeConverterClasses()
        {
            _parametersModel = new ParametersModel(_printerModel, _errorListViewModel);
            _writeSetAxisModel = new WriteSetAxisModel(_parametersModel);
            _corneringModel = new CorneringModel(_printerModel, _printModel, _parametersModel);

            _processG00CommandModel = new ProcessG00CommandModel(_printerModel, _parametersModel);
            _processG92CommandModel = new ProcessG92CommandModel(_parametersModel);
            _processTCommandModel = new ProcessTCommandModel(_printModel, _printerModel, _realTimeStatusDataModel, _parametersModel);

            //Create a dummy Material with default parameters to be passed around until a user-set Material is in place.
            _currentMaterial = new MaterialModel("Unset", _printerModel);
            _currentMaterial.PrintheadModel = new PrintheadModel("Unset");
            _currentMaterial.PrintheadModel.AttachedZAxisModel = new AxisModel("Unset", 'I');
        }
        
        /// <summary>
        /// Converts RepRapGCode into ModiPrintGCode.
        /// </summary>
        public string ConvertGCode(string repRapGCodeInput)
        {
            //Reset parameters for the new conversion.
            InstantiateGCodeConverterClasses();

            //The input string is split into a 2D array, delimited first by linebreaks and then by whitespaces.
            string[][] repRapGCode = GCodeStringParsing.GCodeTo2DArr(repRapGCodeInput);

            //The return string which is the converted GCode.
            //Each line of the GCode is an index in the string.
            //Each line will be delimited by \r\n when converting to a string.
            List<ConvertedGCodeLine> convertedGCode = new List<ConvertedGCodeLine>();

            //Converted GCode commands for starting up the printer.
            convertedGCode.Add(new ConvertedGCodeLine("", "Setup"));

            //Set the current X and Y positions as the origin.
            convertedGCode.Add(new ConvertedGCodeLine(SerialMessageCharacters.SerialCommandSetCharacter + "OriginXY"));

            //Iterates through each line of RepRap's GCode and converts it to ModiPrint's flavor of GCode.
            convertedGCode.Add(new ConvertedGCodeLine("", "Print Start"));
            for (_parametersModel.RepRapLine = 0; (_parametersModel.RepRapLine < repRapGCode.Length) && (repRapGCode != null); _parametersModel.RepRapLine++)
            {
                if (repRapGCode[_parametersModel.RepRapLine] != null
                && !String.IsNullOrWhiteSpace(repRapGCode[_parametersModel.RepRapLine][0]))
                {
                    //Processes the single line of GCode and returns a converted string.
                    List<ConvertedGCodeLine> appendModiPrintGCode;
                    appendModiPrintGCode = SetProcessGCodeCommand(GCodeStringParsing.RemoveGCodeComments(repRapGCode[_parametersModel.RepRapLine]));

                    //Adds a converted GCode line to the return string with line breaks and comments.
                    if ((appendModiPrintGCode != null) && (appendModiPrintGCode.Count != 0))
                    {
                        //Comment the repRap line at the end of the converted GCode line.
                        appendModiPrintGCode[appendModiPrintGCode.Count - 1].Comment += (_parametersModel.RepRapLine + 1); //(repRapLine + 1) because repRapLine is index 0 where line count is index 1.
                        convertedGCode.AddRange(appendModiPrintGCode);
                    }
                }
                int percentCompleted = ((_parametersModel.RepRapLine + 1) * 100) / repRapGCode.Length;
                _parametersModel.ReportProgress("Converting GCode " + percentCompleted + "%");
            }

            //Calculates the deceleration steps parameter in G00 commands.
            _corneringModel.AddDecelerationStepsParameter(ref convertedGCode);

            //Retract this Z Axis.
            string retractZ = SerialMessageCharacters.SerialCommandSetCharacter + "RetractZ";
            convertedGCode.Add(new ConvertedGCodeLine(retractZ));

            _parametersModel.ReportProgress("");

            return GCodeLinesConverter.GCodeLinesListToString(convertedGCode);
        }

        /// <summary>
        /// Sets processGCodeCommand to a ProcessGCodeCommand method.
        /// Executes processGCodeCommand and returns the resulting string.
        /// </summary>
        private List<ConvertedGCodeLine> SetProcessGCodeCommand(string[] repRapLine)
        {
            //Return null if the input parameter is invalid.
            if ((repRapLine.Length == 0) || (String.IsNullOrWhiteSpace(repRapLine[0])))
            { return null; }

            //The return GCode.
            List<ConvertedGCodeLine> convertedGCodeLinesList = null;

            if ((repRapLine[0] == "G00") //Movement or printing
             || (repRapLine[0] == "G0")
             || (repRapLine[0] == "G01")
             || (repRapLine[0] == "G1"))
            {
                convertedGCodeLinesList = _processG00CommandModel.ProcessG00Command(repRapLine, _currentMaterial);
            }
            else if (repRapLine[0] == "GX") //GX commands will always print.
            {
                convertedGCodeLinesList = _processG00CommandModel.ProcessG00Command(repRapLine, _currentMaterial);
            }
            else if (repRapLine[0][0] == 'T') //Switch printheads.
            {
                convertedGCodeLinesList = _processTCommandModel.ProcessTCommand(repRapLine, ref _currentMaterial);
            }
            else if (repRapLine[0] == "G90") //Absolute coordinates for Axes.
            {
                _parametersModel.AbsCoordAxis = true;
            }
            else if (repRapLine[0] == "G91") //Relative coordinates for for X, Y, and Z but not E.
            {
                _parametersModel.AbsCoordAxis = false;
            }
            else if (repRapLine[0] == "G92") //Set new absolute coordinates for X, Y, and Z but not E.
            {
                _processG92CommandModel.ProcessG92Command(repRapLine);
            }
            else if (repRapLine[0] == "M82") //Absolute coordinates for E.
            {
                _parametersModel.AbsCoordExtruder = true;
            }
            else if (repRapLine[0] == "M83") //Relative coordinates for E.
            {
                _parametersModel.AbsCoordExtruder = false;
            }
            //ModiPrint will ignore these commands.
            else if (String.IsNullOrWhiteSpace(repRapLine[0])
                || (repRapLine[0][0] == SerialMessageCharacters.SerialTerminalCharacter) //Comments proceed this character.
                || (repRapLine[0][0] == SerialMessageCharacters.SerialPrintPauseCharacter) //Inidicates pausing of a print sequence.
                || (repRapLine[0] == "M104") //The command to set printer temperature. This is unneeded.
                || (repRapLine[0] == "M106") //The command to turn the fan on. ModiPrint printers do not have fans by default.
                || (repRapLine[0] == "M107") //The command to turn the fan off. ModiPrint printers do not have fans by default.
                || (repRapLine[0] == "M109") //The command to set extruder temperature. ModiPrint printers do not have fans by default.
                || (repRapLine[0] == "M140") //The command to set bed temperature. Irrelevant.
                || (repRapLine[0] == "G04") //The command to pause. ModiPrint has its own built in pausing features.
                || (repRapLine[0] == "G21") //The command to set units to milimeters. ModiPrint only operates in milimeters so this is irrelevant.
                || (repRapLine[0] == "G28") //The command to home to the origin. ModiPrint keeps track of positioning, not the microcontroller.
                )
            {
                return null;
            }
            //The command to set units to inches. ModiPrint operates in milimeters. The user should set RepRap to output milimeters only.
            else if (repRapLine[0] == "G20")
            {
                string repRapLineStr = GCodeStringParsing.GCodeLineArrToStr(repRapLine);
                _parametersModel.ErrorReporterViewModel.ReportError("G-Code Conversion Failed: Inches Not Supported, Set RepRap Parameters to Milimeters", "G-Code Line: " + repRapLineStr);
                return null;
            }
            //ModiPrint will throw errors for unrecognized commands.
            else
            {
                string repRapLineStr = GCodeStringParsing.GCodeLineArrToStr(repRapLine);
                _parametersModel.ErrorReporterViewModel.ReportError("G-Code Conversion Failed: Unrecognized Command, Ensure .gcode File Does Not Contain Unsupported G-Code.", "G-Code Line: " + repRapLineStr);
                return null;
            }

            return convertedGCodeLinesList;
        }
        #endregion
    }
}
