using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using ModiPrint.Models.PrinterModels.PrintheadModels;
using ModiPrint.Models.PrinterModels.PrintheadModels.PrintheadTypeModels;
using ModiPrint.Models.XMLSerializerModels.PrinterXMLSerializerModels;
using ModiPrint.ViewModels;
using ModiPrint.ViewModels.PrinterViewModels.MicrocontrollerViewModels;
using ModiPrint.ViewModels.PrinterViewModels.PrintheadViewModels.PrintheadTypeViewModels;

namespace ModiPrint.Models.XMLSerializerModels.PrinterXMLSerializerModels.PrintheadXMLSerializerModels.PrintheadTypeXMLSerializerModels
{
    public class CustomPrintheadTypeXMLDeserializerModel : PrintheadTypeXMLDeserializerModel
    {
        #region Constructor
        public CustomPrintheadTypeXMLDeserializerModel(MicrocontrollerViewModel MicrocontrollerViewModel, ErrorListViewModel ErrorListViewModel) : base(MicrocontrollerViewModel, ErrorListViewModel)
        {

        }
        #endregion

        #region Methods
        /// <summary>
        /// Deserializes an XML file with Valve Printhead Type properties then stores those properties into the input Valve PrintheadType.
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="printheadTypeViewModel"></param>
        public override void DeserializePrintheadType(XmlReader xmlReader, PrintheadTypeViewModel printheadTypeViewModel)
        {
            CustomPrintheadTypeViewModel customPrintheadTypeViewModel = (CustomPrintheadTypeViewModel)printheadTypeViewModel;

            //Reading this XML is unnecessary until Custom Printhead Type is implemented.
            //For now, reading this XML while it has no child elements would actually break the reader.
            /*
            while (xmlReader.Read())
            {
                //Skip through newlines (this program's XML Writer uses newlines).
                if ((xmlReader.Name != "\n") && (!String.IsNullOrWhiteSpace(xmlReader.Name)))
                {    
                    //End method if the end of "CustomPrintheadType" element is reached.
                    if ((xmlReader.Name == "CustomPrintheadType") && (xmlReader.NodeType == XmlNodeType.EndElement))
                    {
                        return;
                    }

                    switch (xmlReader.Name)
                    {
                        default:
                            base.ReportXMLReaderError(xmlReader);
                            break;
                    }
                }
            }
            */
        }
        #endregion
    }
}
