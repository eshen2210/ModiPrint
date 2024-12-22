using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using ModiPrint.Models.PrintModels.PrintStyleModels;
using ModiPrint.ViewModels;
using ModiPrint.ViewModels.PrintViewModels.MaterialViewModels;
using ModiPrint.ViewModels.PrinterViewModels;
using ModiPrint.ViewModels.PrintViewModels.PrintStyleViewModels;
using ModiPrint.Models.XMLSerializerModels.PrintXMLSerializerModels.PrintStyleXMLSerializerModels;

namespace ModiPrint.Models.XMLSerializerModels.PrintXMLSerializerModels.MaterialXMLSerializerModels
{
    public class MaterialXMLDeserializerModel : XMLConverterModel
    {
        #region Fields and Properties
        //Contains references to all  Printheads.
        public PrinterViewModel _printerViewModel;
        #endregion

        #region Constructor
        public MaterialXMLDeserializerModel(PrinterViewModel PrinterViewModel, ErrorListViewModel ErrorListViewModel) : base(ErrorListViewModel)
        {
            _printerViewModel = PrinterViewModel;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Reads MaterialViewModel properties from XML and sets a MaterialViewModel with said properties.
        /// </summary>
        /// <param name="xmlReader"></param>
        /// <param name="materialViewModel"></param>
        public void DeserializerMaterial(XmlReader xmlReader, MaterialViewModel materialViewModel)
        {
            //Read through each element of the XML string and populate each property of the Material.
            PrintStyleXMLDeserializerModel printStyleXMLDeserializerModel = null;
            while (xmlReader.Read())
            {
                //Skip through newlines (this program's XML Writer uses newlines).
                if ((xmlReader.Name != "\n") && (!String.IsNullOrWhiteSpace(xmlReader.Name)))
                {
                    //End method if the end of "Material" element is reached.
                    if ((xmlReader.Name == "Material") && (xmlReader.NodeType == XmlNodeType.EndElement))
                    {
                        return;
                    }

                    switch (xmlReader.Name)
                    {
                        case "MaterialName": //See if there is a mismatch between the Material argument and the deserialized Material.
                            if (xmlReader.ReadElementContentAsString() != materialViewModel.Name)
                            { base.ReportErrorMismatchedEqupment(xmlReader); }
                            break;
                        case "RepRapID":
                            materialViewModel.RepRapID = xmlReader.ReadElementContentAsString();
                            break;
                        case "PrintheadName":
                            materialViewModel.PrintheadViewModel = _printerViewModel.FindPrinthead(xmlReader.ReadElementContentAsString());
                            break;
                        case "UnsetPrintStyle":
                            //Do nothing.
                            break;
                        case "ContinuousPrintStyle":
                            materialViewModel.PrintStyle = PrintStyle.Continuous;
                            printStyleXMLDeserializerModel = new ContinuousPrintStyleXMLDeserializerModel(base._errorListViewModel);
                            printStyleXMLDeserializerModel.DeserializePrintStyle(xmlReader, materialViewModel.PrintStyleViewModel);
                            break;
                        case "DropletPrintStyle":
                            materialViewModel.PrintStyle = PrintStyle.Droplet;
                            printStyleXMLDeserializerModel = new DropletPrintStyleXMLDeserializerModel(base._errorListViewModel);
                            printStyleXMLDeserializerModel.DeserializePrintStyle(xmlReader, materialViewModel.PrintStyleViewModel);
                            break;
                        case "PauseAfterActivating":
                            string pauseAfterActivatingContent = xmlReader.ReadElementContentAsString();
                            materialViewModel.PauseAfterActivating = (pauseAfterActivatingContent == "True") ? true : false; 
                            break;
                        case "PauseBeforeDeactivating":
                            string pauseBeforeDeactivatingContent = xmlReader.ReadElementContentAsString();
                            materialViewModel.PauseBeforeDeactivating = (pauseBeforeDeactivatingContent == "True") ? true : false;
                            break;
                        case "JunctionDeviation":
                            materialViewModel.JunctionDeviation = xmlReader.ReadElementContentAsDouble();
                            break;
                        case "XYPrintSpeed":
                            materialViewModel.XYPrintSpeed = xmlReader.ReadElementContentAsDouble();
                            break;
                        case "ZPrintSpeed":
                            materialViewModel.ZPrintSpeed = xmlReader.ReadElementContentAsDouble();
                            break;
                        case "XYPrintAcceleration":
                            materialViewModel.XYPrintAcceleration = xmlReader.ReadElementContentAsDouble();
                            break;
                        case "ZPrintAcceleration":
                            materialViewModel.ZPrintAcceleration = xmlReader.ReadElementContentAsDouble();
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
