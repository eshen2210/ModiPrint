using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModiPrint.Models.PrintModels.PrintStyleModels.GradientModels
{
    //Associated with the GradientModel class.
    //States the type of gradient used by a Printhead using the Droplet PrintStyle.
    public enum GradientShape
    {
        None, //No gradient used. All droplets are dispensed with he same open time.
        Point, //A gradient forms around a single point.
        Line, //A gradient forms around a single line.
        Plane, //A gradient forms around a plane.
    }

    //Associated with the GradientModel class.
    //States how the magnitude of the gradient changes with distance.
    public enum GradientScaling
    {
        Linear, //Print parameter changes additively with distance.
        Exponential //Print parameter changes exponentially with distance.
    }

    public abstract class GradientModel
    {
        //How the magnitude of the gradient scales with distance.
        private GradientScaling _gradientScaling;
        public GradientScaling GradientScaling
        {
            get { return _gradientScaling; }
        }
        
        //How quickly the print magnitude changes in relation to the distance from the gradient geometry. 
        //In units of percent per mm.
        //For example, with a RateOfChange of -30, and a Linear GradientScaling, the Valve Printhead will have an OpenTime that decreases by 30% for every mm away from the Point/Line/Plane.
        //For example, with a RateOfChange of 50, and an Exponential GradientScaling, the Valve Printhead will have an OpenTime that is 150% of the initial OpenTime 1 mm away from the initial point, 225% 2 mm away, 337.5% 3 mm away, etc.
        //A positive value will result in an increasing OpenTime / EDispenseDistance as the Printhead moves away from the geometry.
        private double _rateOfChange = 0;
        public double RateOfChange
        {
            get { return _rateOfChange; }
        }
        private double _rateOfChangePercent;
        
        #region Constructor
        public GradientModel(GradientScaling GradientScaling, double RateOfChange)
        {
            _gradientScaling = GradientScaling;
            _rateOfChange = RateOfChange;
            _rateOfChangePercent = _rateOfChange / 100;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Calculates the distance from a point to the gradient geometry.
        /// </summary>
        /// <param name="xPoint"></param>
        /// <param name="yPoint"></param>
        /// <param name="zPoint"></param>
        /// <returns></returns>
        public abstract double DistanceToPoint(double xPoint, double yPoint, double zPoint);

        /// <summary>
        /// Calculates either the Valve OpenTime or the EDispensePerMm (printParameter) at a certain 3D point.
        /// Does not return values lower than zero.
        /// </summary>
        /// <param name="printParameter"></param>
        /// <param name="xPoint"></param>
        /// <param name="yPoint"></param>
        /// <param name="zPoint"></param>
        /// <returns></returns>
        public double PrintParameterAtDistance(double printParameter, double xPoint, double yPoint, double zPoint)
        {
            double printMagnitude = 0;
            //Linear increase of the print parameter with increasing distance.
            if (_gradientScaling == GradientScaling.Linear)
            {
                printMagnitude = printParameter + printParameter * (_rateOfChangePercent * DistanceToPoint(xPoint, yPoint, zPoint));
            }
            //Exponential increase of the print parameter with increasing distance.
            else if (_gradientScaling == GradientScaling.Exponential)
            {
                double x = DistanceToPoint(xPoint, yPoint, zPoint);
                printMagnitude = printParameter * Math.Pow((1 + _rateOfChangePercent), DistanceToPoint(xPoint, yPoint, zPoint));
            }
            return (printMagnitude > 0) ? printMagnitude : 0;
        }
        #endregion
    }
}
