using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using ModiPrint.Models.PrinterModels.PrintheadModels.PrintheadTypeModels;
using ModiPrint.Models.XMLSerializerModels.PrinterXMLSerializerModels.PrintheadXMLSerializerModels.PrintheadTypeXMLSerializerModels;
using ModiPrint.ViewModels;
using ModiPrint.ViewModels.PrinterViewModels;
using ModiPrint.ViewModels.PrinterViewModels.PrintheadViewModels;

namespace ModiPrint.Models.XMLSerializerModels.PrinterXMLSerializerModels.PrintheadXMLSerializerModels
{
    public class PrintheadXMLDeserializerModel : XMLConverterModel
    {
        #region Fields and Properties
        //Contains information of all other Printheads and Axes.
        PrinterViewModel _printerViewModel;
        #endregion

        #region Constructor
        public PrintheadXMLDeserializerModel(PrinterViewModel PrinterViewModel, ErrorListViewModel ErrorListViewModel) : base(ErrorListViewModel)
        {
            _printerViewModel = PrinterViewModel;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Reads PrintheadViewModel properties from XML and sets a PrintheadViewModel with said properties.
        /// </summary>
        /// <param name="xmlReader"></param>
        /// <param name="printheadViewModel"></param>
        public void DeserializePrinthead(XmlReader xmlReader, PrintheadViewModel printheadViewModel)
        {
            //Read through each element of the XML string and populate each property of the Axis.
            PrintheadTypeXMLDeserializerModel printheadTypeXMLDeserializerModel = null;
            while (xmlReader.Read())
            {
                //Skip through newlines (this program's XML Writer uses newlines).
                if ((xmlReader.Name != "\n") && (!String.IsNullOrWhiteSpace(xmlReader.Name)))
                {
                    //End method if the end of "Printhead" element is reached.
                    if ((xmlReader.Name == "Printhead") && (xmlReader.NodeType == XmlNodeType.EndElement))
                    {
                        return;
                    }

                    switch (xmlReader.Name)
                    {
                        case "PrintheadName": //See if there is a mismatch between the Printhead argument and the deserialized Printhead.
                            if (xmlReader.ReadElementContentAsString() != printheadViewModel.Name)
                            { base.ReportErrorMismatchedEqupment(xmlReader); }
                            break;
                        case "ZAxisName":
                            printheadViewModel.AttachedZAxisViewModel = _printerViewModel.FindAxis(xmlReader.ReadElementContentAsString());
                            break;
                        case "MotorizedPrintheadType":
                            printheadViewModel.PrintheadType = PrintheadType.Motorized;
                            printheadTypeXMLDeserializerModel = new MotorizedPrintheadTypeXMLDeserializer(_printerViewModel.MicrocontrollerViewModel, base._errorListViewModel);
                            printheadTypeXMLDeserializerModel.DeserializePrintheadType(xmlReader, printheadViewModel.PrintheadTypeViewModel);
                            break;
                        case "ValvePrintheadType":
                            printheadViewModel.PrintheadType = PrintheadType.Valve;
                            printheadTypeXMLDeserializerModel = new ValvePrintheadTypeXMLDeserializerModel(_printerViewModel.MicrocontrollerViewModel, base._errorListViewModel);
                            printheadTypeXMLDeserializerModel.DeserializePrintheadType(xmlReader, printheadViewModel.PrintheadTypeViewModel);
                            break;
                        case "CustomPrintheadType":
                            printheadViewModel.PrintheadType = PrintheadType.Custom;
                            printheadTypeXMLDeserializerModel = new CustomPrintheadTypeXMLDeserializerModel(_printerViewModel.MicrocontrollerViewModel, base._errorListViewModel);
                            printheadTypeXMLDeserializerModel.DeserializePrintheadType(xmlReader, printheadViewModel.PrintheadTypeViewModel);
                            break;
                        case "UnsetPrintheadType":
                            //Do nothing.
                            break;
                        case "XOffset":
                            printheadViewModel.XOffset = xmlReader.ReadElementContentAsDouble();
                            break;
                        case "YOffset":
                            printheadViewModel.YOffset = xmlReader.ReadElementContentAsDouble();
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
