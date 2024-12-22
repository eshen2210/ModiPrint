using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModiPrint.Models.RealTimeStatusModels.RealTimeStatusPrintheadModels;

namespace ModiPrint.ViewModels.RealTimeStatusViewModels.RealTimeStatusPrintheadViewModels
{
    /// <summary>
    /// Interface between RealTimeStatusValvePrintheadModel and the GUI.
    /// </summary>
    public class RealTimeStatusValvePrintheadViewModel : RealTimeStatusPrintheadViewModel
    {
        #region Fields and Properties
        //Contains parameters of a Valve Printhead during operation.
        private RealTimeStatusValvePrintheadModel _realTimeStatusValvePrintheadModel;

        //Indicates whether or not the valve is on.
        public bool IsValveOn
        {
            get { return _realTimeStatusValvePrintheadModel.IsValveOn; }
            set { _realTimeStatusValvePrintheadModel.IsValveOn = value; }
        }
        #endregion

        #region Constructor
        public RealTimeStatusValvePrintheadViewModel(RealTimeStatusValvePrintheadModel RealTimeStatusValvePrintheadModel) : base(RealTimeStatusValvePrintheadModel)
        {
            _realTimeStatusValvePrintheadModel = RealTimeStatusValvePrintheadModel;
            base._imageSource = "/Resources/General/ValvePrinthead.png";

            UpdateProperties();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Calls OnPropertyChanged to the IsValveOn property.
        /// </summary>
        public void UpdateIsValveOn()
        {
            OnPropertyChanged("IsValveOn");
        }

        /// <summary>
        /// Calls OnPropertyChanged to all properties.
        /// </summary>
        public void UpdateProperties()
        {
            OnPropertyChanged("Name");
            OnPropertyChanged("IsValveOn");
        }
        #endregion
    }
}
