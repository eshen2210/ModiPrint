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
    public class ValvePrintheadTypeXMLSerializerModel : PrintheadTypeXMLSerializerModel
    {
        #region Constructor
        public ValvePrintheadTypeXMLSerializerModel() : base()
        {

        }
        #endregion

        #region Methods
        /// <summary>
        /// Writes Valve Printhead Type properties into XML.
        /// </summary>
        /// <param name="printheadTypeViewModel"></param>
        /// <returns></returns>
        public override void SerializePrintheadType(XmlWriter xmlWriter, PrintheadTypeViewModel printheadTypeViewModel)
        {
            if (printheadTypeViewModel != null)
            {
                ValvePrintheadTypeViewModel valvePrintheadTypeViewModel = (ValvePrintheadTypeViewModel)printheadTypeViewModel;

                //Outmost element should be "ValvePrintheadType".
                xmlWriter.WriteStartElement("ValvePrintheadType");

                //Valve Pin.
                if (valvePrintheadTypeViewModel.AttachedValveGPIOPinViewModel != null)
                { xmlWriter.WriteElementString("ValvePin", valvePrintheadTypeViewModel.AttachedValveGPIOPinViewModel.PinID.ToString()); }

                //Close outmost element "ValvePrintheadType".
                xmlWriter.WriteEndElement();
            }
        }
        #endregion
    }
}
