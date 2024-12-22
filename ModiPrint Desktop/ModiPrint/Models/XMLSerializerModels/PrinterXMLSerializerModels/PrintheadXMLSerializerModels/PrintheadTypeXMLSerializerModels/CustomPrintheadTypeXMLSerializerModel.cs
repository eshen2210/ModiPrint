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
    public class CustomPrintheadTypeXMLSerializerModel : PrintheadTypeXMLSerializerModel
    {
        #region Constructor
        public CustomPrintheadTypeXMLSerializerModel()
        {

        }
        #endregion

        #region Methods
        /// <summary>
        /// Writes Custom Printhead Type properties into XML.
        /// </summary>
        /// <param name="printheadTypeViewModel"></param>
        /// <returns></returns>
        public override void SerializePrintheadType(XmlWriter xmlWriter, PrintheadTypeViewModel printheadTypeViewModel)
        {
            if (printheadTypeViewModel != null)
            {
                CustomPrintheadTypeViewModel customPrintheadTypeViewModel = (CustomPrintheadTypeViewModel)printheadTypeViewModel;

                //Outmost element should be "CustomPrintheadType".
                xmlWriter.WriteStartElement("CustomPrintheadType");

                //Close outmost element "CustomPrintheadType".
                xmlWriter.WriteEndElement();
            }
        }
        #endregion
    }
}
