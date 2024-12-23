using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModiPrint.Models.PrintModels.PrintStyleModels
{
    //Associated with the PrintStyleModel class.
    //States the style by which this printead prints.
    public enum PrintStyle
    {
        Unset, //This Printhead's style has not yet been set.
        Continuous, //This Printhead dispenses ink in lines.
        Droplet //This Printhead dispenses ink in droplets.
    }

    /// <summary>
    /// Extends the MaterialModel class with functions related to the style of printing.
    /// </summary>
    public abstract class PrintStyleModel
    {
        #region Constructor
        public PrintStyleModel()
        {

        }
        #endregion
    }
}
