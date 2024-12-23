using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModiPrint.DataTypes.GlobalValues;


namespace ModiPrint.Models.RealTimeStatusModels.RealTimeStatusAxisModels
{
    /// <summary>
    /// Contains data regarding an Axis during operation.
    /// </summary>
    public class RealTimeStatusAxisModel
    {
        #region Fields and Properties
        //Name of the Axis.
        private string _name;
        public string Name
        {
            get { return _name; }
        }
        
        //Current position of the Axis.
        //In mm away from the origin.
        private double _position;
        public double Position
        {
            get { return _position; }
            set { _position = value; }
        }

        //Current max speed of the Axis.
        //In mm / s.
        private double _maxSpeed;
        public double MaxSpeed
        {
            get { return _maxSpeed; }
        }

        //Current acceleration of the Axis.
        //In mm / s^2.
        private double _acceleration;
        public double Acceleration
        {
            get { return _acceleration; }
        }

        //Current Limit Switch status of the Axis.
        private LimitSwitchStatus _limitSwitchStatus = LimitSwitchStatus.NoLimit;
        public LimitSwitchStatus LimitSwitchStatus
        {
            get { return _limitSwitchStatus; }
            set { _limitSwitchStatus = value; }
        }
        #endregion

        #region Constructor
        public RealTimeStatusAxisModel(string Name, double Position, double MaxSpeed, double Acceleration)
        {
            _name = Name;
            _maxSpeed = MaxSpeed;
            _acceleration = Acceleration;
            _position = Position;
        }
        #endregion
    }
}
