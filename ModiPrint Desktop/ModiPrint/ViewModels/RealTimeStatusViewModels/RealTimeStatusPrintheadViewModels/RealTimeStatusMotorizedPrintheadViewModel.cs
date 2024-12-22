using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModiPrint.Models.RealTimeStatusModels;
using ModiPrint.Models.RealTimeStatusModels.RealTimeStatusPrintheadModels;

namespace ModiPrint.ViewModels.RealTimeStatusViewModels.RealTimeStatusPrintheadViewModels
{
    /// <summary>
    /// Interface between RealTimeStatusMotorizedPrintheadModel and the GUI.
    /// </summary>
    public class RealTimeStatusMotorizedPrintheadViewModel : RealTimeStatusPrintheadViewModel
    {
        #region Fields and Properties
        //Contains parameters of a Motorized Printhead during operation.
        public RealTimeStatusMotorizedPrintheadModel _realTimeStatusMotorizedPrintheadModel;
        
        //Current position of this Printhead.
        public double Position
        {
            get { return _realTimeStatusMotorizedPrintheadModel.Position; }
        }

        //Current max speed of this Printhead.
        public double MaxSpeed
        {
            get { return _realTimeStatusMotorizedPrintheadModel.MaxSpeed; }
        }

        //Current acceleration of this Printhead.
        public double Acceleration
        {
            get { return _realTimeStatusMotorizedPrintheadModel.Acceleration; }
        }

        //Current Limit Switch status of the Motorized Printhead.
        public LimitSwitchStatus LimitSwitchStatus
        {
            get { return _realTimeStatusMotorizedPrintheadModel.LimitSwitchStatus; }
            set { _realTimeStatusMotorizedPrintheadModel.LimitSwitchStatus = value; }
        }
        #endregion

        #region Constructor
        public RealTimeStatusMotorizedPrintheadViewModel(RealTimeStatusMotorizedPrintheadModel RealTimeStatusMotorizedPrintheadModel) : base(RealTimeStatusMotorizedPrintheadModel)
        {
            _realTimeStatusMotorizedPrintheadModel = RealTimeStatusMotorizedPrintheadModel;
            base._imageSource = "/Resources/General/ValvePrinthead.png";

            UpdateProperties();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Calls OnPropertyChanged to the Position and LimitSwitchStatus property.
        /// </summary>
        public void UpdatePosition()
        {
            OnPropertyChanged("Position");
            OnPropertyChanged("LimitSwitchStatus");
        }

        /// <summary>
        /// Calls OnPropertyChanged to all properties.
        /// </summary>
        public void UpdateProperties()
        {
            OnPropertyChanged("Name");
            OnPropertyChanged("Position");
            OnPropertyChanged("MaxSpeed");
            OnPropertyChanged("Acceleration");
        }
        #endregion
    }
}
