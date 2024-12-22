using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using ModiPrint.Models.XMLSerializerModels.PrintXMLSerializerModels;
using ModiPrint.ViewModels.PrintViewModels.PrintStyleViewModels;

namespace ModiPrint.Models.XMLSerializerModels.PrintXMLSerializerModels.PrintStyleXMLSerializerModels
{
    public abstract class PrintStyleXMLSerializerModel
    {
        #region Constructor
        public PrintStyleXMLSerializerModel() : base()
        {

        }
        #endregion

        #region Methods
        public abstract void SerializePrintStyle(XmlWriter xmlWriter, PrintStyleViewModel printStyleViewModel);
        #endregion
    }
}
