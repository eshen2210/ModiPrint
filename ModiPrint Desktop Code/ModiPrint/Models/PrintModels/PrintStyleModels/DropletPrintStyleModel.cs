using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModiPrint.Models.PrintModels.PrintStyleModels.GradientModels;

namespace ModiPrint.Models.PrintModels.PrintStyleModels
{
    /// <summary>
    /// Extends the MaterialModel class with parameters related to droplet printing.
    /// </summary>
    public class DropletPrintStyleModel : PrintStyleModel
    {
        #region Fields and Properties
        //Parameter used by Motorized Printheads while printing with this Droplet PrintStyle.
        //This parameter specifies the distance (in milimeters) of Printhead motor movement for each droplet.
        private double _motorizedDispenseDistance = 0;
        public double MotorizedDispenseDistance
        {
            get { return _motorizedDispenseDistance; }
            set
            {
                if (0 <= value)
                { _motorizedDispenseDistance = value; }
                else
                { _motorizedDispenseDistance = 0; }
            }
        }

        //Parameter used by Motorized Printheads while printing with this Droplet PrintStyle.
        //This parameter specifies the maximum speed (in milimeters per second) by which this Printhead motor moves.
        private double _motorizedDispenseMaxSpeed = 0;
        public double MotorizedDispenseMaxSpeed
        {
            get { return _motorizedDispenseMaxSpeed; }
            set
            {
                if (0 <= value)
                { _motorizedDispenseMaxSpeed = value; }
                else
                { _motorizedDispenseMaxSpeed = 0; }
            }
        }

        //Parameter used by Motorized Printheads while printing with this Droplet PrintStyle.
        //This parameter specifies the acceleration (in milimeters per second squared) by which this Printhead motor moves.
        private double _motorizedDispenseAcceleration = 0;
        public double MotorizedDispenseAcceleration
        {
            get { return _motorizedDispenseAcceleration; }
            set
            {
                if (0 <= value)
                { _motorizedDispenseAcceleration = value; }
                else
                { _motorizedDispenseAcceleration = 0; }
            }
        }

        //Parameter used by Valve Printheads while printing with this Droplet PrintStyle. 
        //This parameter specifies the opening time (in microseconds) of the valve for each droplet.
        private int _valveOpenTime = 0;
        public int ValveOpenTime
        {
            get { return _valveOpenTime; }
            set
            {
                if (0 <= value)
                { _valveOpenTime = value; }
                else
                { _valveOpenTime = 0; }
            }
        } 

        //Distance between each droplet along the printed line.
        private double _interpolateDistance = 0;
        public double InterpolateDistance
        {
            get { return _interpolateDistance; }
            set
            {
                if (0 <= value)
                { _interpolateDistance = value; }
                else
                { _interpolateDistance = 0; }
            }
        }

        //The geometry that the gradient is based around.
        private GradientShape _gradientShape = GradientShape.None;
        public GradientShape GradientShape
        {
            get { return _gradientShape; }
            set { _gradientShape = value; }
        }

        //How the magnitude changes per distance in the gradient.
        private GradientScaling _gradientScaling = GradientScaling.Linear;
        public GradientScaling GradientScaling
        {
            get { return _gradientScaling; }
            set { _gradientScaling = value; }
        }

        //How quickly the print magnitude changes in relation to the distance from the gradient geometry. 
        //In units of percent per mm.
        //For example, with a RateOfChange of -30, then a Valve Printhead will have an OpenTime that decreases by 30% for every mm away from the Point/Line/Plane.
        //A positive value will result in an increasing OpenTime / EDispenseDistance as the Printhead moves away from the geometry.
        //If OpenTime is ever less than zero at some distance, OpenTime will default to zero.
        private double _percentPerMm;
        public double PercentPerMm
        {
            get { return _percentPerMm; }
            set
            {
                _percentPerMm = value;
            }
        }

        //The 3 dimensional coordinates that determine the geometry of the gradient.
        //All coordinates are relative to the origin (set as the position of the active Printhead at the start of each Print).
        //Distances are in units of mm.

        //A GradientShape of a Point requires one 3D coordinate.
        private double _x1;
        public double X1
        {
            get { return _x1; }
            set { _x1 = value; }
        }
        private double _y1;
        public double Y1
        {
            get { return _y1; }
            set { _y1 = value; }
        }
        private double _z1;
        public double Z1
        {
            get { return _z1; }
            set { _z1 = value; }
        }

        //A GradientShape of a Line requires two 3D coordinates.
        private double _x2;
        public double X2
        {
            get { return _x2; }
            set { _x2 = value; }
        }
        private double _y2;
        public double Y2
        {
            get { return _y2; }
            set { _y2 = value; }
        }
        private double _z2;
        public double Z2
        {
            get { return _z2; }
            set { _z2 = value; }
        }

        //A GradientShape of a Plane requires three 3D coordinates.
        private double _x3;
        public double X3
        {
            get { return _x3; }
            set { _x3 = value; }
        }
        private double _y3;
        public double Y3
        {
            get { return _y3; }
            set { _y3 = value; }
        }
        private double _z3;
        public double Z3
        {
            get { return _z3; }
            set { _z3 = value; }
        }
        #endregion

        #region Constructor
        public DropletPrintStyleModel() : base()
        {

        }
        #endregion
    }
}
