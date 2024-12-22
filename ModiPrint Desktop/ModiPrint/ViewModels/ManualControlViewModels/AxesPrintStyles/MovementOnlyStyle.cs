using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModiPrint.ViewModels.ManualControlViewModels.AxesPrintStyles
{
    /// <summary>
    /// Contains display elements for the AxesPrintStyle parameter on ManualControlViewModel.
    /// </summary>
    public class MovementOnlyStyle : AxesPrintStyle
    {
        #region Constructor
        public MovementOnlyStyle() : base()
        {
            base._styleID = "Movement Only";
            base._imageSource = "/Resources/General/Move2.png";
            base._displayString1 = "Movement";
            base._displayString2 = "Only";
        }
        #endregion
    }
}
