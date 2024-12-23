using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using ModiPrint.Models.XMLSerializerModels;
using ModiPrint.Models.XMLSerializerModels.PrintXMLSerializerModels;
using ModiPrint.ViewModels;
using ModiPrint.ViewModels.PrintViewModels.PrintStyleViewModels;

namespace ModiPrint.Models.XMLSerializerModels.PrintXMLSerializerModels.PrintStyleXMLSerializerModels
{
    public abstract class PrintStyleXMLDeserializerModel : XMLConverterModel
    {
        #region Constructor
        public PrintStyleXMLDeserializerModel(ErrorListViewModel ErrorListViewModel) : base(ErrorListViewModel)
        {

        }
        #endregion

        #region Methods
        public abstract void DeserializePrintStyle(XmlReader xmlReader, PrintStyleViewModel printStyleViewModel);
        #endregion
    }
}
