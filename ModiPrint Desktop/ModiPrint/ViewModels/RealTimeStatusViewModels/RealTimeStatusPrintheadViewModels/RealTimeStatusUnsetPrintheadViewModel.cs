using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModiPrint.Models.RealTimeStatusModels.RealTimeStatusPrintheadModels;

namespace ModiPrint.ViewModels.RealTimeStatusViewModels.RealTimeStatusPrintheadViewModels
{
    /// <summary>
    /// Interface between RealTimeStatusUnsetPrintheadModel and the GUI.
    /// </summary>
    public class RealTimeStatusUnsetPrintheadViewModel : RealTimeStatusPrintheadViewModel
    {
        #region Fields and Properties
        //Contains parameters of a Unset Printhead during operation.
        private RealTimeStatusPrintheadModel _realTimeStatusPrintheadModel;
        #endregion

        #region Constructor
        public RealTimeStatusUnsetPrintheadViewModel(RealTimeStatusPrintheadModel RealTimeStatusPrintheadModel) : base(RealTimeStatusPrintheadModel)
        {
            _realTimeStatusPrintheadModel = RealTimeStatusPrintheadModel;
        }
        #endregion
    }
}
