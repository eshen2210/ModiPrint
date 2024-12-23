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
    public class ValvePrintheadTypeXMLDeserializerModel : PrintheadTypeXMLDeserializerModel
    {
        #region Constructor
        public ValvePrintheadTypeXMLDeserializerModel(MicrocontrollerViewModel MicrocontrollerViewModel, ErrorListViewModel ErrorListViewModel) : base(MicrocontrollerViewModel, ErrorListViewModel)
        {

        }
        #endregion

        #region Methods
        /// <summary>
        /// Deserializes an XML file with Valve Printhead Type properties then stores those properties into the input Valve PrintheadType.
        /// </summary>
        /// <param name="xmlReader"></param>
        /// <param name="printheadTypeViewModel"></param>
        public override void DeserializePrintheadType(XmlReader xmlReader, PrintheadTypeViewModel printheadTypeViewModel)
        {
            ValvePrintheadTypeViewModel valvePrintheadTypeViewModel = (ValvePrintheadTypeViewModel)printheadTypeViewModel;

            while (xmlReader.Read())
            {
                //Skip through newlines (this program's XML Writer uses newlines).
                if ((xmlReader.Name != "\n") && (!String.IsNullOrWhiteSpace(xmlReader.Name)))
                {
                    //End method if the end of "ValvePrintheadType" element is reached.
                    if ((xmlReader.Name == "ValvePrintheadType") && (xmlReader.NodeType == XmlNodeType.EndElement))
                    {
                        return;
                    }

                    switch (xmlReader.Name)
                    {
                        case "ValvePin":
                            valvePrintheadTypeViewModel.AttachedValveGPIOPinViewModel = base._microcontrollerViewModel.FindPin(xmlReader.ReadElementContentAsInt());
                            valvePrintheadTypeViewModel.ExecuteRefreshPinBySettingListCommand("ValveGPIOPinViewModelList");
                            break;
                            break;
                        default:
                            base.ReportErrorUnrecognizedElement(xmlReader);
                            break;
                    }
                }
            }
        }
        #endregion
    }
}
