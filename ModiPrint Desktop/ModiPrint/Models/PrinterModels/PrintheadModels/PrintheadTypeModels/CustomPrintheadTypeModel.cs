using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ModiPrint.Models.PrinterModels.PrintheadModels.PrintheadTypeModels
{
    /// <summary>
    /// Extends PrintheadModel with properties and functions related the operation of undefined Printheads.
    /// </summary>
    public class CustomPrintheadTypeModel : PrintheadTypeModel
    {
        //To do: This printheadtype needs a list of GPIOPins
        
        #region Constructor
        public CustomPrintheadTypeModel(string AttachedName) : base(AttachedName)
        {

        }
        #endregion

        #region Methods
        /// <summary>
        /// Resets the parameters of all GPIO pins.
        /// Typically, this method is called when the equipment is removed.
        /// </summary>
        public override void UnattachGPIOPins()
        {

        }
        #endregion
    }
}
