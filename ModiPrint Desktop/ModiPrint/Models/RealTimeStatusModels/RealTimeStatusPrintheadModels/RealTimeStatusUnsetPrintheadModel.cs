using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModiPrint.Models.RealTimeStatusModels.RealTimeStatusPrintheadModels
{
    public class RealTimeStatusUnsetPrintheadModel : RealTimeStatusPrintheadModel
    {
        #region Constructor
        public RealTimeStatusUnsetPrintheadModel(string Name) : base(Name)
        {
            base._name = "Unset";
        }
        #endregion
    }
}
