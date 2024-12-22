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
    public class DropletPrintStyle : AxesPrintStyle
    {
        #region Constructor
        public DropletPrintStyle() : base()
        {
            base._styleID = "Droplet Print";
            base._imageSource = "/Resources/General/Droplet.png";
            base._displayString1 = "Droplet";
            base._displayString2 = "Print";
        }
        #endregion
    }
}
