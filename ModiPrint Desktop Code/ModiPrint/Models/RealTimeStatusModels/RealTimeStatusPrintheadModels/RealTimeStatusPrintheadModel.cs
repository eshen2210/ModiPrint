using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModiPrint.Models.RealTimeStatusModels.RealTimeStatusPrintheadModels
{
    public abstract class RealTimeStatusPrintheadModel
    {
        #region Fields and Properties
        //Name of the Printhead.
        protected string _name;
        public string Name
        {
            get { return _name; }
        }
        #endregion

        #region Constructor
        public RealTimeStatusPrintheadModel(string Name)
        {
            _name = Name;
        }
        #endregion
    }
}
