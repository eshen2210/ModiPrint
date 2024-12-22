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
    public abstract class AxesPrintStyle
    {
        #region Fields and Properties
        //Identifier for this style.
        protected string _styleID;
        public string StyleID
        {
            get { return _styleID; }
        }

        //Image representing this style.
        protected string _imageSource;
        public string ImageSource
        {
            get { return _imageSource; }
        }

        //String to display when selecteding print styles.
        protected string _displayString1;
        public string DisplayString1
        {
            get { return _displayString1; }
        }

        protected string _displayString2;
        public string DisplayString2
        {
            get { return _displayString2; }
        }
        #endregion

        #region Constructor
        public AxesPrintStyle()
        {

        }
        #endregion
    }
}
