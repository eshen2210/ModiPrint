using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModiPrint.Models.PrintModels.PrintStyleModels;
using ModiPrint.Models.PrinterModels.MicrocontrollerModels.PinModels;

namespace ModiPrint.Models.PrinterModels.PrintheadModels.PrintheadTypeModels
{
    /// <summary>
    /// Extends PrintheadModel with properties and functions related the operation of motorized extruders.
    /// </summary>
    public class MotorizedPrintheadTypeModel : PrintheadTypeModel
    {
        #region Fields and Properties
        //GPIO Pin that signals the step of this Printhead's motor.
        private GPIOPinModel _attachedMotorStepGPIOPinModel;
        public GPIOPinModel AttachedMotorStepGPIOPinModel
        {
            get { return _attachedMotorStepGPIOPinModel; }
            set
            {
                GPIOPinModelUtilities.AttachGPIOPinModel(value, ref _attachedMotorStepGPIOPinModel, AttachedName + " Step", PinSetting.Output);
            }
        }

        //GPIO Pin that signals the direction of this Printhead's motor. 
        private GPIOPinModel _attachedMotorDirectionGPIOPinModel;
        public GPIOPinModel AttachedMotorDirectionGPIOPinModel
        {
            get { return _attachedMotorDirectionGPIOPinModel; }
            set
            {
                GPIOPinModelUtilities.AttachGPIOPinModel(value, ref _attachedMotorDirectionGPIOPinModel, AttachedName + " Direction", PinSetting.Output);
            }
        }

        //GPIO Pin that receives signal from the Limit Switches that limits the range of the Motorized Printhead.
        private GPIOPinModel _attachedLimitSwitchGPIOPinModel;
        public GPIOPinModel AttachedLimitSwitchGPIOPinModel
        {
            get { return _attachedLimitSwitchGPIOPinModel; }
            set
            {
                GPIOPinModelUtilities.AttachGPIOPinModel(value, ref _attachedLimitSwitchGPIOPinModel, AttachedName + " Limit", PinSetting.Input);
            }
        }

        //Maximum speed of the motor of this Printhead.
        //In milimeter per second.
        private double _maxSpeed = 0;
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

        //Maximum acceleration of the motor of this Printhead.
        //In milimeter per seconds squared.
        private double _maxAcceleration = 0;
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

        //Distance per step of the motor of this Printhead.
        //In milimeters per step.
        private double _mmPerStep = 0;
        public double MmPerStep
        {
            get { return _mmPerStep; }
            set { _mmPerStep = value; }
        }

        //Time in between step signal on and step signal off.
        //In microseconds.
        private int _stepPulseTime = 0;
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
        //Coventionally, stepper motors turn clockwise in the positive direction.
        //Motorized Printhead actuators in this system are mounted with the motors mounted on top facing down.
        //With this value as false, Motorized Printhead motors will cause the actuators to move downwards (printing) during positive direction operations.
        private bool _isDirectionInverted = false;
        public bool IsDirectionInverted
        {
            get { return _isDirectionInverted; }
            set { _isDirectionInverted = value; }
        }
        #endregion

        #region Constructor
        public MotorizedPrintheadTypeModel(string AttachedName) : base(AttachedName)
        {
            base._possiblePrintStylesList.Add(PrintStyle.Continuous);
            base._possiblePrintStylesList.Add(PrintStyle.Droplet);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns true if Motorized Printhead parameters are set for a print.
        /// </summary>
        /// <returns></returns>
        public override bool ReadyToPrint()
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
        public override void UnattachGPIOPins()
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
