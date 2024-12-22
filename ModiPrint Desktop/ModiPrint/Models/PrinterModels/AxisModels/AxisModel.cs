using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ModiPrint.Models.PrinterModels.MicrocontrollerModels.PinModels;

namespace ModiPrint.Models.PrinterModels.AxisModels
{
    /// <summary>
    /// Manages all aspects of the XY axis.
    /// </summary>
    public class AxisModel
    {
        #region Fields and Properties
        //Identifier used by ModiPrint and the GUI.
        private string _name;
        public string Name
        {
            get { return _name; }
        }

        //Identifier used by the GCode.
        private char _axisID;
        public char AxisID
        {
            get { return _axisID; }
        }

        //Pin that receives signal from the Limit Switches that limits the range of the Axis.
        private GPIOPinModel _attachedLimitSwitchGPIOPinModel;
        public GPIOPinModel AttachedLimitSwitchGPIOPinModel
        {
            get { return _attachedLimitSwitchGPIOPinModel; }
            set
            {
                GPIOPinModelUtilities.AttachGPIOPinModel(value, ref _attachedLimitSwitchGPIOPinModel, _name + " Limit", PinSetting.Input);
            }
        }

        //Pins that drive the Axis motor's step and direction.
        private GPIOPinModel _attachedMotorStepGPIOPinModel;
        public GPIOPinModel AttachedMotorStepGPIOPinModel
        {
            get { return _attachedMotorStepGPIOPinModel; }
            set
            {
                GPIOPinModelUtilities.AttachGPIOPinModel(value, ref _attachedMotorStepGPIOPinModel, _name + " Step", PinSetting.Output);
            }
        }

        private GPIOPinModel _attachedMotorDirectionGPIOPinModel;
        public GPIOPinModel AttachedMotorDirectionGPIOPinModel
        {
            get { return _attachedMotorDirectionGPIOPinModel; }
            set
            {
                GPIOPinModelUtilities.AttachGPIOPinModel(value, ref _attachedMotorDirectionGPIOPinModel, _name + " Direction", PinSetting.Output);
            }
        }

        //Maximum speed of the Axis.
        //In milimeter per second.
        private double _maxSpeed = 40;
        public double MaxSpeed
        {
            get { return _maxSpeed; }
            set
            {
                if (0 <= value)
                { _maxSpeed = value; }
                else
                { _maxSpeed = 0; }
            }
        }

        //Maximum acceleration of the Axis.
        //In milimeter per seconds squared.
        private double _maxAcceleration = 2000;
        public double MaxAcceleration
        {
            get { return _maxAcceleration; }
            set
            {
                if (0 <= value)
                { _maxAcceleration = value; }
                else
                { _maxAcceleration = 0; }
            }
        }
        
        //Distance per step of the Axis.
        //In milimeters per step.
        private double _mmPerStep = 0.005;
        public double MmPerStep
        {
            get { return _mmPerStep; }
            set { _mmPerStep = value; }
        }

        //Time in between step signal on and step signal off.
        //In microseconds.
        private int _stepPulseTime = 10;
        public int StepPulseTime
        {
            get { return _stepPulseTime; }
            set { _stepPulseTime = value; }
        }

        //Highest possible distance away from the origin.
        //In milimeters.
        private double _maxPosition = 0;
        public double MaxPosition
        {
            get { return _maxPosition; }
            set { _maxPosition = value; }
        }

        //Highest possible distance away from the origin in the negative direction.
        //In milimeters.
        private double _minPosition = 0;
        public double MinPosition
        {
            get { return _minPosition; }
            set { _minPosition = value; }
        }

        //If true, multiply all position values of this Axis by -1 during operation.
        private bool _isDirectionInverted = false;
        public bool IsDirectionInverted
        {
            get { return _isDirectionInverted; }
            set { _isDirectionInverted = value; }
        }

        //Can this axis be removed from the printer?
        //Returns true if it is a Z Axis.
        public bool IsRemovable
        {
            get { return _axisID == 'Z' ? true : false; }
        }
        #endregion

        #region Constructor
        public AxisModel(string Name, char AxisID)
        {
            _name = Name;
            _axisID = AxisID;

            //Coventionally, stepper motors turn clockwise in the positive direction.
            //Y Axis actuators in this system are mounted with the motor-end facing towards the user.
            //Inverting the direction of the Y Axis motors will cause the actuators to move away from the use during positive direction operations.
            //Similarly, inverting the Z Axis will cause the Z Axes to move up during positive direction operations.
            //The X Axes are defaulted to false.
            //This allows the X Axis to move right during positive direction operations.
            _isDirectionInverted = ((_axisID == 'Y') || (_axisID == 'Z')) ? true : false;

            //Z actuators have a finer distance per step. Z actuators also move slower.
            _mmPerStep = (_name[0] == 'Z') ? 0.0025 : 0.005;
            _maxSpeed = (_name[0] == 'Z') ? 5 : 40;
            _maxAcceleration = (_name[0] == 'Z') ? 100 : 2000;
        }
        #endregion

        #region Methods
        /// <summary>
        /// If all parameters are set appropriately for a print, then return true.
        /// </summary>
        public bool ReadyToPrint()
        {
            if ((_attachedMotorDirectionGPIOPinModel != null)
                && (_attachedMotorStepGPIOPinModel != null)
                && (_maxSpeed > 0)
                && (_maxAcceleration > 0)
                && (_mmPerStep > 0)
                && (_stepPulseTime > 0)
                && (_maxPosition < 0)
                && (_minPosition > 0))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Resets the parameters of all GPIO pins.
        /// Typically, this method is called when the equipment is removed.
        /// </summary>
        public void UnattachGPIOPins()
        {
            if (_attachedMotorStepGPIOPinModel != null)
            { _attachedMotorStepGPIOPinModel.ResetPinParameters(); }
            if (_attachedMotorDirectionGPIOPinModel != null)
            { _attachedMotorDirectionGPIOPinModel.ResetPinParameters(); }
            if (_attachedLimitSwitchGPIOPinModel != null)
            { _attachedLimitSwitchGPIOPinModel.ResetPinParameters(); }
        }
        #endregion
    }
}
