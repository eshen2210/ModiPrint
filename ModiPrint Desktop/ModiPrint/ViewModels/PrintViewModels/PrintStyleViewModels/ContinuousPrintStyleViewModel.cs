using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModiPrint.Models.PrintModels.PrintStyleModels;

namespace ModiPrint.ViewModels.PrintViewModels.PrintStyleViewModels
{
    /// <summary>
    /// Interface between ContinuousPrintStyleModel and the GUI.
    /// </summary>
    public class ContinuousPrintStyleViewModel : PrintStyleViewModel
    {
        #region Fields and Proeprties
        //Model counterpart.
        private ContinuousPrintStyleModel _continuousPrintStyleModel;
        public ContinuousPrintStyleModel ContinuousPrintStyleModel
        {
            get { return _continuousPrintStyleModel; }
        }

        //Parameter used by Motorized Printheads while printing with this Continuous PrintStyle.
        //This parameter specifies the distance (in milimeters) of Printhead motor movement for each mm movement of the XYZ stage.
        public double MotorizedDispensePerMmMovement
        {
            get { return _continuousPrintStyleModel.MotorizedDispensePerMmMovement; }
            set { _continuousPrintStyleModel.MotorizedDispensePerMmMovement = value; }
        }

        //Parameter used by Motorized Printheads while printing with this Continuous PrintStyle.
        //This parameters specifies the distance (in milimeters) of Printhead motor movement during retraction.
        public double MotorizedDispenseRetractionDistance
        {
            get { return _continuousPrintStyleModel.MotorizedDispenseRetractionDistance; }
            set { _continuousPrintStyleModel.MotorizedDispenseRetractionDistance = value; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Writes ContinuousPrintStyleViewModel properties into XML.
        /// </summary>
        /// <param name="ContinuousPrintStyleModel"></param>
        public ContinuousPrintStyleViewModel(ContinuousPrintStyleModel ContinuousPrintStyleModel) : base(ContinuousPrintStyleModel)
        {
            _continuousPrintStyleModel = ContinuousPrintStyleModel;
        }
        #endregion
    }
}
