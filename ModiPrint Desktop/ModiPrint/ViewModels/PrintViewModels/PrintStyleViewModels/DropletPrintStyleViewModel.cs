using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModiPrint.Models.PrintModels.PrintStyleModels;
using ModiPrint.Models.PrintModels.PrintStyleModels.GradientModels;

namespace ModiPrint.ViewModels.PrintViewModels.PrintStyleViewModels
{
    /// <summary>
    /// Interface between DropletPrintStyleModel and the GUI.
    /// </summary>
    public class DropletPrintStyleViewModel : PrintStyleViewModel
    {
        #region Fields and Properties
        //Model counterpart.
        private DropletPrintStyleModel _dropletPrintStyleModel;
        public DropletPrintStyleModel DropletPrintStyleModel
        {
            get { return _dropletPrintStyleModel; }
        }

        //The parameter used by Motorized Printheads while printing with this Droplet PrintStyle.
        //This parameter specifies the distance (in milimeters) of Printhead motor movement for each droplet.
        public double MotorizedDispenseDistance
        {
            get { return _dropletPrintStyleModel.MotorizedDispenseDistance; }
            set
            {
                _dropletPrintStyleModel.MotorizedDispenseDistance = value;
                OnPropertyChanged("MotorizedDispenseDistance");
            }
        }

        //Parameter used by Motorized Printheads while printing with this Droplet PrintStyle.
        //This parameter specifies the maximum speed (in milimeters per second) by which this Printhead motor moves.
        public double MotorizedDispenseMaxSpeed
        {
            get { return _dropletPrintStyleModel.MotorizedDispenseMaxSpeed; }
            set
            {
                _dropletPrintStyleModel.MotorizedDispenseMaxSpeed = value;
                OnPropertyChanged("MotorizedDispenseMaxSpeed");
            }
        }

        //Parameter used by Motorized Printheads while printing with this Droplet PrintStyle.
        //This parameter specifies the acceleration (in milimeters per second squared) by which this Printhead motor moves.
        public double MotorizedDispenseAcceleration
        {
            get { return _dropletPrintStyleModel.MotorizedDispenseAcceleration; }
            set
            {
                _dropletPrintStyleModel.MotorizedDispenseAcceleration = value;
                OnPropertyChanged("MotorizedDispenseAcceleration");
            }
        }

        //The parameter used by Valve Printheads while printing with this Droplet PrintStyle. 
        //This parameter specifies the opening time (in microseconds) of the valve for each droplet.
        public int ValveOpenTime
        {
            get { return _dropletPrintStyleModel.ValveOpenTime; }
            set
            {
                _dropletPrintStyleModel.ValveOpenTime = value;
                OnPropertyChanged("ValveOpenTime");
            }
        }

        //Distance between each droplet along the printed line.
        public double InterpolateDistance
        {
            get { return _dropletPrintStyleModel.InterpolateDistance; }
            set
            {
                _dropletPrintStyleModel.InterpolateDistance = value;
                OnPropertyChanged("InterpolateDistance");
            }
        }

        //The geometry that the gradient is based around.
        public GradientShape GradientShape
        {
            get { return _dropletPrintStyleModel.GradientShape; }
            set
            {
                _dropletPrintStyleModel.GradientShape = value;
                OnPropertyChanged("GradientShape");
            }
        }

        //Returns values of GradientShape that are bindable to a control's Itemsource property.
        public IEnumerable<GradientShape> GradientShapeValues
        {
            get { return Enum.GetValues(typeof(GradientShape)).Cast<GradientShape>(); }
        }

        //How the magnitude changes per distance in the gradient.
        public GradientScaling GradientScaling
        {
            get { return _dropletPrintStyleModel.GradientScaling; }
            set
            {
                _dropletPrintStyleModel.GradientScaling = value;
                OnPropertyChanged("GradientScaling");
            }
        }

        //Returns values of GradientScaling that are bindable to a control's Itemsource property.
        public IEnumerable<GradientScaling> GradientScalingValues
        {
            get { return Enum.GetValues(typeof(GradientScaling)).Cast<GradientScaling>(); }
        }

        //How quickly the print magnitude changes in relation to the distance from the gradient geometry. 
        //In units of percent per mm.
        //For example, with a RateOfChange of -30, then a Valve Printhead will have an OpenTime that decreases by 30% for every mm away from the Point/Line/Plane.
        //A positive value will result in an increasing OpenTime or EDispenseDistance as the Printhead moves away from the geometry.
        public double PercentPerMm
        {
            get { return _dropletPrintStyleModel.PercentPerMm; }
            set { _dropletPrintStyleModel.PercentPerMm = value; }
        }

        //The 3 dimensional coordinates that determine the geometry of the gradient.
        //All coordinates are relative to the origin (set as the position of the active Printhead at the start of each Print).
        //Distances are in units of mm.

        //A GradientShape of a Point requires one 3D coordinate.
        public double X1
        {
            get { return _dropletPrintStyleModel.X1; }
            set { _dropletPrintStyleModel.X1 = value; }
        }
        public double Y1
        {
            get { return _dropletPrintStyleModel.Y1; }
            set { DropletPrintStyleModel.Y1 = value; }
        }
        public double Z1
        {
            get { return _dropletPrintStyleModel.Z1; }
            set { _dropletPrintStyleModel.Z1 = value; }
        }

        //A GradientShape of a Line requires two 3D coordinates.
        public double X2
        {
            get { return _dropletPrintStyleModel.X2; }
            set { _dropletPrintStyleModel.X2 = value; }
        }
        public double Y2
        {
            get { return _dropletPrintStyleModel.Y2; }
            set { _dropletPrintStyleModel.Y2 = value; }
        }
        public double Z2
        {
            get { return _dropletPrintStyleModel.Z2; }
            set { _dropletPrintStyleModel.Z2 = value; }
        }

        //A GradientShape of a Plane requires three 3D coordinates.
        public double X3
        {
            get { return _dropletPrintStyleModel.X3; }
            set { _dropletPrintStyleModel.X3 = value; }
        }
        public double Y3
        {
            get { return _dropletPrintStyleModel.Y3; }
            set { _dropletPrintStyleModel.Y3 = value; }
        }
        public double Z3
        {
            get { return _dropletPrintStyleModel.Z3; }
            set { _dropletPrintStyleModel.Z3 = value; }
        }
        #endregion

        #region Constructor
        public DropletPrintStyleViewModel(DropletPrintStyleModel DropletPrintStyleModel) : base(DropletPrintStyleModel)
        {
            _dropletPrintStyleModel = DropletPrintStyleModel;
        }
        #endregion
    }
}
