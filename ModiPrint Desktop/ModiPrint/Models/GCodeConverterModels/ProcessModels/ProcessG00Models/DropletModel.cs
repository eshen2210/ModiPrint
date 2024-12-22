using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using ModiPrint.Models.PrintModels.PrintStyleModels.GradientModels;
using ModiPrint.Models.PrintModels.PrintStyleModels;

namespace ModiPrint.Models.GCodeConverterModels.ProcessModels.ProcessG00Models
{
    public class DropletModel
    {
        #region Fields and Properties
        //Distance already travelled since the previous printed droplet.
        //When the distanceTravelled == interpolateDistance, the system should print.
        //In mm.
        private double _distanceTravelled = 0;

        //Distances to travel before executing the next droplet print.
        //These distances will be a non-zero number whenever there is some movement but not enough movement to reach the next droplet.
        //Like the coordinate fields, the 0th index will be X, 1st Y, adn 2nd Z.
        //In mm.
        private double[] _preDistance = { 0, 0, 0 };
        public double[] PreDistance
        {
            get { return _preDistance; }
            set { _preDistance = value; }
        }

        //Distance in between each droplet.
        private double _interpolateDistance;

        //Is this the first droplet?
        //If so, then a droplet needs to be deposited at the beginning of the first movement.
        private bool _firstDroplet = true;

        //Contains functions for managing the Gradient. 
        private GradientModel _gradientModel;
        public GradientModel GradientModel
        {
            get { return _gradientModel; }
        }

        private GradientShape _gradientShape;
        public GradientShape GradientShape
        {
            get { return _gradientShape; }
        }
        private GradientScaling _gradientScaling;
        public GradientScaling GradientScaling
        {
            get { return _gradientScaling; }
        }

        #endregion

        #region Constructor
        /// <summary>
        /// Supports concentration gradients.
        /// </summary>
        /// <param name="DropletPrintStyleModel"></param>
        public DropletModel(DropletPrintStyleModel DropletPrintStyleModel)
        {
            _interpolateDistance = DropletPrintStyleModel.InterpolateDistance;
            _gradientShape = DropletPrintStyleModel.GradientShape;
            _gradientScaling = DropletPrintStyleModel.GradientScaling;

            Point3D point1 = new Point3D(DropletPrintStyleModel.X1, DropletPrintStyleModel.Y1, DropletPrintStyleModel.Z1);
            Point3D point2 = new Point3D(DropletPrintStyleModel.X2, DropletPrintStyleModel.Y2, DropletPrintStyleModel.Z2);
            Point3D point3 = new Point3D(DropletPrintStyleModel.X3, DropletPrintStyleModel.Y3, DropletPrintStyleModel.Z3);

            switch (DropletPrintStyleModel.GradientShape)
            {
                case GradientShape.None:
                    _gradientModel = null;
                    break;
                case GradientShape.Point:
                    _gradientModel = new PointModel(_gradientScaling, DropletPrintStyleModel.PercentPerMm, point1);
                    break;
                case GradientShape.Line:
                    _gradientModel = new LineModel(_gradientScaling, DropletPrintStyleModel.PercentPerMm, point1, point2);
                    break;
                case GradientShape.Plane:
                    _gradientModel = new PlaneModel(_gradientScaling, DropletPrintStyleModel.PercentPerMm, point1, point2, point3);
                    break;
            }
        }

        /// <summary>
        /// Does not support concentration gradients.
        /// </summary>
        /// <param name="InterpolateDistance"></param>
        public DropletModel(double InterpolateDistance)
        {
            _interpolateDistance = InterpolateDistance;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns a list of relative positions that should be droplet printed.
        /// Positions are relative and in units of mm.
        /// Movement by relative PreDistance values should be executed before droplet printing.
        /// PreDistance values should be an array of 3 where the XPreDistance value is in the 0th index, Y in 1st, Z in 2nd.
        /// If return value or PreDistances are null, then no printing or movement is required.
        /// </summary>
        /// <param name="xDistance"></param>
        /// <param name="yDistance"></param>
        /// <param name="zDistance"></param>
        /// <returns></returns>
        public List<double[]> PrintPosition(double xDistance, double yDistance, double zDistance)
        {
            //All position values in this function are relative to the starting point of movements.
            //Rounding is used here to prevent errors incurred from using floating point values.

            xDistance = Math.Round(xDistance, 8);
            yDistance = Math.Round(yDistance, 8);
            zDistance = Math.Round(zDistance, 8);

            double distanceToPrint = Math.Round(_interpolateDistance - _distanceTravelled, 8); //Distance until the next droplet in mm.
            double movementDistance = Math.Round(G00Calculator.CalculateDistance(xDistance, yDistance, zDistance), 8); //Total distance travelled in this movement.
            List<double[]> printDistancesList = new List<double[]>(); //Relative coordinates to print in units of mm.

            //Is this the first droplet?
            //If so, then a droplet needs to be deposited at the beginning of the first movement.
            if (_firstDroplet == true)
            {
                double[] printCoord = new double[3];
                printCoord[0] = printCoord[1] = printCoord[2] = 0;

                printDistancesList.Add(printCoord);

                _firstDroplet = false;
            }

            //Movement is too short for the interpolation.
            //No print happens.
            if (distanceToPrint > movementDistance)
            {
                _distanceTravelled += movementDistance;
                _preDistance[0] += xDistance;
                _preDistance[1] += yDistance;
                _preDistance[2] += zDistance;
                return null;
            }
            //Movement is exactly the length needed for the next droplet.
            //The print happens at the end of the movement.
            else if (distanceToPrint == movementDistance)
            {
                _distanceTravelled = 0;

                double[] printCoord = new double[3];
                printCoord[0] = _preDistance[0] + xDistance;
                printCoord[1] = _preDistance[1] + yDistance;
                printCoord[2] = _preDistance[2] + zDistance;

                _preDistance[0] = _preDistance[1] = _preDistance[2] = 0;

                printDistancesList.Add(printCoord);
                return printDistancesList;
            }
            //Movement is longer than the length needed for the next droplet.
            //One or multiple prints happen in the middle of the movement.
            else if (distanceToPrint < movementDistance)
            {
                double percentMovementTravelled = 0; //Percentage of the movement that needs to be traversed until the next droplet.
                double xPrintPosition = 0; //Last position printed at.
                double yPrintPosition = 0;
                double zPrintPosition = 0;

                for (percentMovementTravelled = Math.Abs(distanceToPrint / movementDistance); 
                     Math.Round(percentMovementTravelled, 8) <= 1; 
                     percentMovementTravelled += (_interpolateDistance / movementDistance))
                {
                    xPrintPosition = percentMovementTravelled * xDistance;
                    yPrintPosition = percentMovementTravelled * yDistance;
                    zPrintPosition = percentMovementTravelled * zDistance;

                    double[] printCoord = { xPrintPosition + _preDistance[0], yPrintPosition + _preDistance[1], zPrintPosition + _preDistance[2] };
                    printDistancesList.Add(printCoord);
                }

                _preDistance[0] = xDistance - xPrintPosition;
                _preDistance[1] = yDistance - yPrintPosition;
                _preDistance[2] = zDistance - zPrintPosition;
                _distanceTravelled = G00Calculator.CalculateDistance(_preDistance[0], _preDistance[1], _preDistance[2]);
                return printDistancesList;
            }

            //Should not reach this point.
            return null;
        }
        #endregion
    }
}
