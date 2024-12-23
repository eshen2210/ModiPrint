using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace ModiPrint.Models.PrintModels.PrintStyleModels.GradientModels
{
    public class LineModel : GradientModel
    {
        #region Fields and Properties
        //The line.
        private Vector3D _line;

        //Arbitrary points on the line.
        private Point3D _linePoint1;
        private Point3D _linePoint2;
        #endregion

        #region Constructor
        /// <summary>
        /// Takes 2 points to create a line.
        /// </summary>
        /// <param name="Point1"></param>
        /// <param name="Point2"></param>
        public LineModel(GradientScaling GradientScaling, double RateOfChange, Point3D Point1, Point3D Point2) : base(GradientScaling, RateOfChange)
        {
            _linePoint1 = Point1;
            _linePoint2 = Point2;
            _line = Point3D.Subtract(_linePoint2, _linePoint1);
        }
        #endregion

        #region 
        /// <summary>
        /// Calculate the shortest distance from a 3D point to a 3D line.
        /// </summary>
        /// <param name="xPoint"></param>
        /// <param name="yPoint"></param>
        /// <param name="zPoint"></param>
        /// <returns></returns>
        public override double DistanceToPoint(double xPoint, double yPoint, double zPoint)
        {
            Point3D pointCoord = new Point3D(xPoint, yPoint, zPoint);

            //Arbitrary line from the point to somewhere on the line.
            Vector3D lineToPoint = Point3D.Subtract(_linePoint1, pointCoord);

            return Vector3D.CrossProduct(_line, lineToPoint).Length / _line.Length;
        }
        #endregion
    }
}
