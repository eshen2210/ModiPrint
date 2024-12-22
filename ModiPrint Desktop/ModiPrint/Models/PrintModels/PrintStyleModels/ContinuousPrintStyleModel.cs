using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModiPrint.Models.PrintModels.PrintStyleModels
{
    /// <summary>
    /// Extends the MaterialModel class with parameters related to contiuous printing.
    /// </summary>
    public class ContinuousPrintStyleModel : PrintStyleModel
    {
        #region Fields and Properties
        //Parameter used by Motorized Printheads while printing with this Continuous PrintStyle.
        //This parameter specifies the distance (in milimeters) of Printhead motor movement for each mm movement of the XYZ stage.
        private double _motorizedDispensePerMmMovement = 0;
        public double MotorizedDispensePerMmMovement
        {
            get { return _motorizedDispensePerMmMovement; }
            set { _motorizedDispensePerMmMovement = value; }
        }

        //Parameter used by Motorized Printheads while printing with this Continuous PrintStyle.
        //This parameters specifies the distance (in milimeters) of Printhead motor movement during retraction.
        private double _motorizedDispenseRetractionDistance = 0;
        public double MotorizedDispenseRetractionDistance
        {
            get { return _motorizedDispenseRetractionDistance; }
            set { _motorizedDispenseRetractionDistance = value; }
        }

        //To Do: Convert to mdpt and debug continuous motorized dispensing
        #endregion

        #region Constructor
        public ContinuousPrintStyleModel() : base()
        {

        }
        #endregion
    }
}
