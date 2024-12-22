using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Win32;

namespace ModiPrint.Models.GCodeModels
{
    public class GCodeModel
    {
        #region Fields and Properties
        //GCode as a full string.
        //GCodeStr is meant to be displayed in the GUI.
        private string _gCodeStr;
        public string GCodeStr
        {
            get { return _gCodeStr; }
            set { _gCodeStr = value; }
        }
        #endregion

        #region Constructors
        public GCodeModel()
        {

        }
        #endregion

        #region Methods
        /// <summary>
        /// Parses the string parameter into a string[] with newline as the delimiter and sets the string[] GCode property as this string[].        
        /// /// </summary>
        public string[] GCodeStrToArr()
        {
            return _gCodeStr.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
        }
    
        /// <summary>
        /// Sets both GCode property to default values.
        /// </summary>
        public void Clear()
        {
            _gCodeStr = "";
        }
        #endregion
    }
}
