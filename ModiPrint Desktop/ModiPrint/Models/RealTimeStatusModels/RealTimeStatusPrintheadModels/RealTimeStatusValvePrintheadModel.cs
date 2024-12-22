using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModiPrint.Models.RealTimeStatusModels.RealTimeStatusPrintheadModels
{
    public class RealTimeStatusValvePrintheadModel : RealTimeStatusPrintheadModel
    {
        #region Fields and Properties        
        //Indicates whether or not the valve is on.
        private bool _isValveOn = false;
        public bool IsValveOn
        {
            get { return _isValveOn; }
            set { _isValveOn = value; }
        }
        #endregion

        #region Constructor
        public RealTimeStatusValvePrintheadModel(string Name) : base(Name)
        {

        }
        #endregion
    }
}
