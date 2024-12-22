using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModiPrint.Models.PrintModels.PrintStyleModels;

namespace ModiPrint.Models.PrinterModels.PrintheadModels.PrintheadTypeModels
{
    //Associated with the PrintheadModel class.
    //States the type of Printhead and its capability.
    public enum PrintheadType
    {
        Unset, //Default value. This printhead type is not yet set.
        Motorized, //This Printhead is extrusion-based and takes limitSwitch PWN signals. 
        Valve, //This Printhead is valve-based and takes operates on high-low signal inputs.
        Custom //This Printhead's takes custom forms of signals.
    }

    /// <summary>
    /// Extends the PrintheadModel class with functions related to the type of printhead.
    /// </summary>
    public abstract class PrintheadTypeModel
    {
        #region Fields and Properties
        //Name of the Printhead that this class is extending.
        private string _attachedName;
        public string AttachedName
        {
            get { return _attachedName; }
        }
        
        //Possible settings for this PrintheadType.
        protected List<PrintStyle> _possiblePrintStylesList = new List<PrintStyle>();
        public List<PrintStyle> PossiblePrintStylesList
        {
            get { return _possiblePrintStylesList; }
        }
        #endregion

        #region Constructor
        public PrintheadTypeModel(string AttachedName)
        {
            _attachedName = AttachedName;
            _possiblePrintStylesList.Add(PrintStyle.Unset);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Is this PrintheadType compatible with the argument PrintStyle?
        /// </summary>
        /// <param name="printStyle"></param>
        /// <returns></returns>
        public bool IsPrintStylePossible(PrintStyle printStyle)
        {
            foreach (PrintStyle possiblePrintStyle in _possiblePrintStylesList)
            {
                if (possiblePrintStyle == printStyle)
                { return true; }
            }
            return false;
        }

        /// <summary>
        /// If all parameters are set appropriately for a print, then return true.
        /// </summary>
        /// <returns></returns>
        public virtual bool ReadyToPrint()
        {
            //Made to be overriden.
            return false;
        }

        /// <summary>
        /// Resets the parameters of all GPIO pins.
        /// Typically, this method is called when the equipment is removed.
        /// </summary>
        public virtual void UnattachGPIOPins()
        {
            //Made to be overriden.
        }
        #endregion
    }
}
