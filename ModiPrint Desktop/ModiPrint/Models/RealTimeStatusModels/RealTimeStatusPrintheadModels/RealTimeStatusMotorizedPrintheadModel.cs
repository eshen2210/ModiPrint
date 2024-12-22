using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModiPrint.Models.RealTimeStatusModels.RealTimeStatusPrintheadModels
{
    public class RealTimeStatusMotorizedPrintheadModel : RealTimeStatusPrintheadModel
    {
        #region Fields and Properties
        //Current position of this Printhead.
        private double _position = 0;
        public double Position
        {
            get { return _position; }
            set { _position = value; }
        }

        //Current max speed of this Printhead.
        private double _maxSpeed;
        public double MaxSpeed
        {
            get { return _maxSpeed; }
        }

        //Current acceleration of this Printhead.
        private double _acceleration;
        public double Acceleration
        {
            get { return _acceleration; }
        }

        //Current Limit Switch status of the Motorized Printhead.
        private LimitSwitchStatus _limitSwitchStatus = LimitSwitchStatus.NoLimit;
        private readonly object _limitSwitchLock = new object();
        public LimitSwitchStatus LimitSwitchStatus
        {
            get { lock (_limitSwitchLock) { return _limitSwitchStatus; } }
            set { lock (_limitSwitchLock) { _limitSwitchStatus = value; } }
        }
        #endregion

        #region Constructor
        public RealTimeStatusMotorizedPrintheadModel(string Name, double MaxSpeed, double Acceleration) : base(Name)
        {
            _maxSpeed = MaxSpeed;
            _acceleration = Acceleration;
        }
        #endregion
    }
}
