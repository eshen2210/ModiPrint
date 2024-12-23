using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModiPrint.Models.RealTimeStatusModels.RealTimeStatusPrintheadModels;

namespace ModiPrint.ViewModels.RealTimeStatusViewModels.RealTimeStatusPrintheadViewModels
{
    /// <summary>
    /// Interface between RealTimeStatusPrintheadModel and the GUI.
    /// </summary>
    public class RealTimeStatusPrintheadViewModel : ViewModel
    {
        #region Fields and Properties
        //Contains information on the parmaeters of a Printhead during operation.
        protected RealTimeStatusPrintheadModel _realTimeStatusPrintheadModel;

        //Name of the Printhead.
        public string Name
        {
            get { return _realTimeStatusPrintheadModel.Name; }
        }

        //Image that represents this printhead.
        protected string _imageSource = "/Resources/General/Printhead.png";
        public string ImageSource
        {
            get { return _imageSource; }
        }
        #endregion

        #region Constructor
        public RealTimeStatusPrintheadViewModel(RealTimeStatusPrintheadModel RealTimeStatusPrintheadModel)
        {
            _realTimeStatusPrintheadModel = RealTimeStatusPrintheadModel;

            OnPropertyChanged("Name");
        }
        #endregion
    }
}
