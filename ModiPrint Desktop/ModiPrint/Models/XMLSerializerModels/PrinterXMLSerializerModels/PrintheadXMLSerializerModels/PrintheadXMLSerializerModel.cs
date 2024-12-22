using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using ModiPrint.Models.PrinterModels.PrintheadModels.PrintheadTypeModels;
using ModiPrint.ViewModels.PrinterViewModels.PrintheadViewModels;
using ModiPrint.Models.XMLSerializerModels.PrinterXMLSerializerModels.PrintheadXMLSerializerModels.PrintheadTypeXMLSerializerModels;

namespace ModiPrint.Models.XMLSerializerModels.PrinterXMLSerializerModels.PrintheadXMLSerializerModels
{
    public class PrintheadXMLSerializerModel
    {
        #region Constructor
        public PrintheadXMLSerializerModel()
        {

        }
        #endregion

        #region Methods
        /// <summary>
        /// Writes PrintheadViewModel properties into a XmlWriter.
        /// </summary>
        /// <param name="xmlWriter"></param>
        /// <param name="printheadViewModel"></param>
        public void SerializePrinthead(XmlWriter xmlWriter, PrintheadViewModel printheadViewModel)
        {
            //Outmost element should be "Printhead".
            xmlWriter.WriteStartElement("Printhead");

            //Outmost element should contain properties required for identification and instantiation.
            //Name.
            xmlWriter.WriteAttributeString("Name", printheadViewModel.Name);

            //Z Axis.
            if (printheadViewModel.AttachedZAxisViewModel != null)
            { xmlWriter.WriteElementString("ZAxisName", printheadViewModel.AttachedZAxisViewModel.Name); }

            //Printhead Type.
            PrintheadTypeXMLSerializerModel printheadTypeXMLSerializerModel = null;
            switch (printheadViewModel.PrintheadType)
            {
                case PrintheadType.Motorized:
                    printheadTypeXMLSerializerModel = new MotorizedPrintheadTypeXMLSerializerModel();
                    break;
                case PrintheadType.Valve:
                    printheadTypeXMLSerializerModel = new ValvePrintheadTypeXMLSerializerModel();
                    break;
                case PrintheadType.Custom:
                    printheadTypeXMLSerializerModel = new CustomPrintheadTypeXMLSerializerModel();
                    break;
                case PrintheadType.Unset:
                    xmlWriter.WriteElementString("UnsetPrintheadType", "");
                    break;
            }
            if (printheadTypeXMLSerializerModel != null)
            { printheadTypeXMLSerializerModel.SerializePrintheadType(xmlWriter, printheadViewModel.PrintheadTypeViewModel); }

            //XOffset.
            xmlWriter.WriteElementString("XOffset", printheadViewModel.XOffset.ToString());

            //YOffset.
            xmlWriter.WriteElementString("YOffset", printheadViewModel.YOffset.ToString());
                    
            //End "Printhead" outmost element.
            xmlWriter.WriteEndElement();
        }
        #endregion
    }
}
