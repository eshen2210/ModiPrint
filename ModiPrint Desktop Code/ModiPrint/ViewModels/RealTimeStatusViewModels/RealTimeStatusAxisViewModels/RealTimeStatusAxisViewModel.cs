using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ModiPrint.Models.RealTimeStatusModels;
using ModiPrint.Models.RealTimeStatusModels.RealTimeStatusAxisModels;

namespace ModiPrint.ViewModels.RealTimeStatusViewModels.RealTimeStatusAxisViewModels
{
    public class RealTimeStatusAxisViewModel : ViewModel
    {
        #region Fields and Properties
        //Contains information on an Axis during Printer operation.
        RealTimeStatusAxisModel _realTimeStatusAxisModel;

        //Name of the Axis.
        public string Name
        {
            get { return _realTimeStatusAxisModel.Name; }
        }

        //Current position of the Axis.
        public double Position
        {
            get { return _realTimeStatusAxisModel.Position; }
        }

        //Current max speed of the X Axis.
        public double MaxSpeed
        {
            get { return _realTimeStatusAxisModel.MaxSpeed; }
        }

        //Current acceleration of the X Axis.
        public double Acceleration
        {
            get { return _realTimeStatusAxisModel.Acceleration; }
        }

        //Current Limit Switch status of the Axis.
        public LimitSwitchStatus LimitSwitchStatus
        {
            get { return _realTimeStatusAxisModel.LimitSwitchStatus; }
            set { _realTimeStatusAxisModel.LimitSwitchStatus = value; }
        }
        #endregion

        #region Constructor
        public RealTimeStatusAxisViewModel(RealTimeStatusAxisModel RealTimeStatusAxisModel)
        {
            _realTimeStatusAxisModel = RealTimeStatusAxisModel;

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
