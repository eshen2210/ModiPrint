using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModiPrint.Models.RealTimeStatusModels.RealTimeStatusPrintheadModels;

namespace ModiPrint.ViewModels.RealTimeStatusViewModels.RealTimeStatusPrintheadViewModels
{
    /// <summary>
    /// Interface between RealTimeStatusCustomPrintheadModel and the GUI.
    /// </summary>
    public class RealTimeStatusCustomPrintheadViewModel : RealTimeStatusPrintheadViewModel
    {
        #region Fields and Properties
        //Contains parameters of a Custom Printhead during operation.
        private RealTimeStatusCustomPrintheadModel _realTimeStatusCustomPrintheadModel;
        #endregion

        #region Constructor
        public RealTimeStatusCustomPrintheadViewModel(RealTimeStatusCustomPrintheadModel RealTimeStatusCustomPrintheadModel) : base(RealTimeStatusCustomPrintheadModel)
        {
            _realTimeStatusCustomPrintheadModel = RealTimeStatusCustomPrintheadModel;
        }
        #endregion
    }
}
