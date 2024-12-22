using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModiPrint.Models.PrinterModels.PrintheadModels;
using ModiPrint.Models.PrinterModels.PrintheadModels.PrintheadTypeModels;
using ModiPrint.Models.GCodeConverterModels.ProcessModels.WriteSetEquipmentModels;

namespace ModiPrint.Models.GCodeConverterModels.ProcessModels.ProcessTCommandModels
{
    /// <summary>
    /// Finds and calls the appropriate WritePrinthead method.
    /// </summary>
    public class SetWritePrintheadModel
    {
        #region Fields
        //Classes that output convertedGCode based on Print and Printer parameters.
        private WriteSetPrintheadModel _writeSetPrintheadModel;

        //Class containing pseudo-global parameters for the GCodeConverter that are passed around various GCodeConverter classes.
        private ParametersModel _parametersModel;
        #endregion

        #region Constructor
        public SetWritePrintheadModel(ParametersModel ParametersModel)
        {
            _writeSetPrintheadModel = new WriteSetPrintheadModel(_parametersModel);
            _parametersModel = ParametersModel;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Finds the appropriate WritePrinthead method and returns the converted GCode output.
        /// </summary>
        /// <param name="printheadModel"></param>
        /// <returns></returns>
        public string SetWritePrinthead(PrintheadModel printheadModel)
        {
            string convertedGCode = "";

            try
            {
                switch (printheadModel.PrintheadType)
                {
                    case PrintheadType.Motorized:
                        convertedGCode = _writeSetPrintheadModel.WriteSetMotorDrivenPrinthead(printheadModel);
                        break;
                    case PrintheadType.Valve:
                        convertedGCode = _writeSetPrintheadModel.WriteSetValvePrinthead(printheadModel);
                        break;
                    case PrintheadType.Custom:
                        //To Do:
                        break;
                    case PrintheadType.Unset:
                        //To Do: Stop everything something's wrong
                        break;
                    default:
                        //To Do: Stop everything something's wrong
                        break;
                }
            }
            catch when (printheadModel == null) //Catch unset Printhead.
            {
                //This should have been caught earlier.
                convertedGCode = "";
            }
            catch
            {
                //Should never reach this point.
                _parametersModel.ErrorReporterViewModel.ReportError("G-Code Conversion Failed: Should Not Happen, Please Report This Error To The Developer", "Unspecified Error");
            }

            return convertedGCode;
        }
        #endregion
    }
}
