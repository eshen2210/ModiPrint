using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using ModiPrint.Models.XMLSerializerModels;
using ModiPrint.ViewModels;
using ModiPrint.ViewModels.PrinterViewModels.MicrocontrollerViewModels;
using ModiPrint.ViewModels.PrinterViewModels.PrintheadViewModels.PrintheadTypeViewModels;

namespace ModiPrint.Models.XMLSerializerModels.PrinterXMLSerializerModels.PrintheadXMLSerializerModels.PrintheadTypeXMLSerializerModels
{
    public abstract class PrintheadTypeXMLDeserializerModel : XMLConverterModel
    {
        #region Fields and Properties
        //Contains functions to find Printer equipment that associates to the PrintheadTypes.
        protected MicrocontrollerViewModel _microcontrollerViewModel;
        #endregion

        #region Constructor
        public PrintheadTypeXMLDeserializerModel(MicrocontrollerViewModel MicrocontrollerViewModel, ErrorListViewModel ErrorListViewModel) : base(ErrorListViewModel)
        {
            _microcontrollerViewModel = MicrocontrollerViewModel;
        }
        #endregion

        #region Methods
        public abstract void DeserializePrintheadType(XmlReader xmlReader, PrintheadTypeViewModel printheadTypeViewModel);
        #endregion
    }
}
