using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace ModiPrint.Models.PrintModels.PrintStyleModels.GradientModels
{
    /// <summary>
    /// A plane expressed as the equation AX + BY + CZ + D = 0.
    /// </summary>
    public class PlaneModel : GradientModel
    {
        //Variables in the reqpresentative equation of the plane aX + bY + cZ + d = 0.
        private double _a;
        public double A
        {
            get { return _a; }
        }
        private double _b;
        public double B
        {
            get { return _b; }
        }
        private double _c;
        public double C
        {
            get { return _c; }
        }
        private double _d;
        public double D
        {
            get { return _d; }
        }

        //Three points in the plane.
        private Point3D _point1;
        public Point3D Point1
        {
            get { return _point1; }
        }
        private Point3D _point2;
        public Point3D Point2
        {
            get { return _point2; }
        }
        private Point3D _point3;
        public Point3D Point3
        {
            get { return _point3; }
        }

        //Unit normal vector of the plane.
        private Vector3D _normalVector;
        public Vector3D NormalVector
        {
            get { return _normalVector; }
        }

        /// <summary>
        /// Takes three 3D points and generates a Plane.
        /// </summary>
        /// <param name="Point1"></param>
        /// <param name="Point2"></param>
        /// <param name="Point3"></param>
        public PlaneModel(GradientScaling GradientScaling, double RateOfChange, Point3D Point1, Point3D Point2, Point3D Point3) : base(GradientScaling, RateOfChange)
        {
            _point1 = Point1;
            _point2 = Point2;
            _point3 = Point3;

            Vector3D vector1 = Point3D.Subtract(_point2, _point1);
            Vector3D vector2 = Point3D.Subtract(_point2, _point3);

            _normalVector = Vector3D.CrossProduct(vector1, vector2);
            _normalVector.Normalize();

            _a = _normalVector.X;
            _b = _normalVector.Y;
            _c = _normalVector.Z;
            _d = -1 * (A * _point1.X + B * _point2.X + C * _point3.X);
        }

        /// <summary>
        /// Calculate the shortest distance from a 3D point to a 3D plane.
        /// </summary>
        /// <param name="xPoint"></param>
        /// <param name="yPoint"></param>
        /// <param name="zPoint"></param>
        /// <returns></returns>
        public override double DistanceToPoint(double xPoint, double yPoint, double zPoint)
        {
            Point3D point = new Point3D(xPoint, yPoint, zPoint);
            Vector3D pointToPlane = Point3D.Subtract(point, _point1);
            return Math.Abs(Vector3D.DotProduct(_normalVector, pointToPlane));
        }
    }
}
