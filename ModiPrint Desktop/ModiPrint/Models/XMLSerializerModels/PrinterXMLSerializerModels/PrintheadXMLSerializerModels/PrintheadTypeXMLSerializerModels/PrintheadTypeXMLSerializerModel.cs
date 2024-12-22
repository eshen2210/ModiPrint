using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using ModiPrint.Models.XMLSerializerModels.PrinterXMLSerializerModels;
using ModiPrint.ViewModels.PrinterViewModels.PrintheadViewModels.PrintheadTypeViewModels;

namespace ModiPrint.Models.XMLSerializerModels.PrinterXMLSerializerModels.PrintheadXMLSerializerModels.PrintheadTypeXMLSerializerModels
{
    public abstract class PrintheadTypeXMLSerializerModel
    {
        #region Constructor
        public PrintheadTypeXMLSerializerModel()
        {

        }
        #endregion

        #region Methods
        public abstract void SerializePrintheadType(XmlWriter xmlWriter, PrintheadTypeViewModel printheadTypeViewModel);
        #endregion
    }
}
