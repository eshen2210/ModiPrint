using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;


namespace ModiPrint.Models.PrintModels.PrintStyleModels.GradientModels
{
    public class PointModel : GradientModel
    {
        #region Feilds and Properties
        //The point.
        private Point3D _point;
        #endregion

        #region Constructor
        /// <summary>
        /// Generate a 3D point from a set of three 1D coordinates.
        /// </summary>
        /// <param name="Point"></param>
        public PointModel(GradientScaling GradientScaling, double RateOfChange, Point3D Point) : base(GradientScaling, RateOfChange)
        {
            _point = Point;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Calculate the shortest distance from a 3D point to a 3D point.
        /// </summary>
        /// <param name="xPoint"></param>
        /// <param name="yPoint"></param>
        /// <param name="zPoint"></param>
        /// <returns></returns>
        public override double DistanceToPoint(double xPoint, double yPoint, double zPoint)
        {
            return Math.Sqrt(Math.Pow(xPoint - _point.X, 2) + Math.Pow(yPoint - _point.Y, 2) + Math.Pow(zPoint - _point.Z, 2));
        }
        #endregion
    }
}
