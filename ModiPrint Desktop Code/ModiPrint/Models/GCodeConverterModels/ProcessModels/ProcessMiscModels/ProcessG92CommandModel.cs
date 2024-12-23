using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModiPrint.Models.GCodeConverterModels.ProcessModels.ProcessMiscModels
{
    public class ProcessG92CommandModel
    {
        #region Fields and Properties
        //Contains global variables for the GCodeConverter.
        private ParametersModel _parametersModel;
        #endregion

        #region Constructor
        public ProcessG92CommandModel(ParametersModel ParametersModel)
        {
            _parametersModel = ParametersModel;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Execute RepRap G92 command.
        /// </summary>
        /// <param name="repRapLine"></param>
        public void ProcessG92Command(string[] repRapLine)
        {
            //Read through the GCode line and parse coordinate values.
            for (int phrase = 1; phrase < repRapLine.Length; phrase++)
            {
                switch (repRapLine[phrase][0])
                {
                    case 'X':
                        _parametersModel.XCoord.NewCoord(GCodeStringParsing.ParseDouble(repRapLine[phrase]));
                        break;
                    case 'Y':
                        _parametersModel.YCoord.NewCoord(GCodeStringParsing.ParseDouble(repRapLine[phrase]));
                        break;
                    case 'Z':
                        _parametersModel.ZCoord.NewCoord(GCodeStringParsing.ParseDouble(repRapLine[phrase]));
                        break;
                    case 'E':
                        _parametersModel.ERepRapCoord.NewCoord(GCodeStringParsing.ParseDouble(repRapLine[phrase]));
                        break;
                }
            }
        }
        #endregion
    }
}
