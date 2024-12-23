using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using ModiPrint.ViewModels;

namespace ModiPrint.Models.XMLSerializerModels
{
    public abstract class XMLConverterModel
    {
        #region Fields and Methods
        //Outputs error messages to the GUI.
        protected ErrorListViewModel _errorListViewModel;
        #endregion

        #region Constructor
        public XMLConverterModel(ErrorListViewModel ErrorListViewModel)
        {
            _errorListViewModel = ErrorListViewModel;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Output an error to the GUI containing information about the XML Reader.
        /// Outputs "Unrecognized Element" error message.
        /// </summary>
        /// <param name="xmlReader"></param>
        protected void ReportErrorUnrecognizedElement(XmlReader xmlReader)
        {
            IXmlLineInfo xmlInfo = xmlReader as IXmlLineInfo;
            _errorListViewModel.AddError("Improperly Loaded Save File: Unrecognized Element", "XML Line: " + xmlInfo.LineNumber);
        }

        /// <summary>
        /// Output an error to the GUI containing information about the XML Reader.
        /// Outputs "Mismatched Equipment" error message.
        /// </summary>
        /// <param name="xmlReader"></param>
        protected void ReportErrorMismatchedEqupment(XmlReader xmlReader)
        {
            IXmlLineInfo xmlInfo = xmlReader as IXmlLineInfo;
            _errorListViewModel.AddError("Improperly Loaded Save File: Mismatched Equipment", "XML Line: " + xmlInfo.LineNumber);
        }
        #endregion
    }
}
