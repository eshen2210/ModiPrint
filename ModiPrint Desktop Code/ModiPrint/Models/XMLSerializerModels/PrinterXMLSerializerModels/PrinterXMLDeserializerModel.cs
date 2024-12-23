using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using ModiPrint.Models.XMLSerializerModels.PrinterXMLSerializerModels.AxisXMLSerializerModels;
using ModiPrint.Models.XMLSerializerModels.PrinterXMLSerializerModels.PrintheadXMLSerializerModels;
using ModiPrint.ViewModels;
using ModiPrint.ViewModels.PrinterViewModels;
using ModiPrint.ViewModels.PrinterViewModels.AxisViewModels;
using ModiPrint.ViewModels.PrinterViewModels.PrintheadViewModels;
using ModiPrint.ViewModels.PrinterViewModels.MicrocontrollerViewModels;

namespace ModiPrint.Models.XMLSerializerModels.PrinterXMLSerializerModels
{
    public class PrinterXMLDeserializerModel : XMLConverterModel
    {
        #region Fields and Properties
        //Required for the deserializer classes to operate and/or required by the constructors of Printer equipment classes.
        GPIOPinListsViewModel _gPIOPinListsViewModel;
        PrinterViewModel _printerViewModel;

        //Contains functions to convert XML to Printer equipment settings.
        AxisXMLDeserializerModel _axisXMLDeserializerModel;
        PrintheadXMLDeserializerModel _printheadXMLDeserializerModel;

        //A list of the AxisViewModels that were deserialized.
        private List<AxisViewModel> _deserializedAxisViewModelsList = new List<AxisViewModel>();

        //A list of the PrintheadViewModels that were deserialized.
        private List<PrintheadViewModel> _deserializedPrintheadViewModelsList = new List<PrintheadViewModel>();
        #endregion

        #region Constructor
        public PrinterXMLDeserializerModel(GPIOPinListsViewModel GPIOPinListsViewModel, PrinterViewModel PrinterViewModel, ErrorListViewModel ErrorListViewModel) : base(ErrorListViewModel)
        {
            _gPIOPinListsViewModel = GPIOPinListsViewModel;
            _printerViewModel = PrinterViewModel;

            _axisXMLDeserializerModel = new AxisXMLDeserializerModel(_printerViewModel, base._errorListViewModel);
            _printheadXMLDeserializerModel = new PrintheadXMLDeserializerModel(_printerViewModel, base._errorListViewModel);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Reads Printer properties from XML and sets a Printer with said properties.
        /// </summary>
        /// <param name="xmlReader"></param>
        /// <param name="printerViewModel"></param>
        public void DeserializePrinter(XmlReader xmlReader, PrinterViewModel printerViewModel)
        {
            //Read through each element of the XML string and populate each property of the Printer.
            while (xmlReader.Read())
            {
                //Skip through newlines (this program's XML Writer uses newlines).
                if ((xmlReader.Name != "\n") && (!String.IsNullOrWhiteSpace(xmlReader.Name)))
                {
                    //End method if the end of "Printer" element is reached.
                    if ((xmlReader.Name == "Printer") && (xmlReader.NodeType == XmlNodeType.EndElement))
                    {
                        //Remove all Printer elements that were not just deserialized.
                        RemoveNonDeserializedAxes(printerViewModel);
                        RemoveNonDeserializedPrintheads(printerViewModel);

                        return;
                    }

                    //Deserialize and populate each property of the Printer.
                    switch (xmlReader.Name)
                    {
                        case "Axis":
                            LoadAxis(xmlReader, printerViewModel);
                            break;
                        case "Printhead":
                            LoadPrinthead(xmlReader, printerViewModel);
                            break;
                        case "ZAxesCreatedCount":
                            printerViewModel.ZAxesCreatedCount = xmlReader.ReadElementContentAsInt();
                            break;
                        case "PrintheadsCreatedCount":
                            printerViewModel.PrintheadsCreatedCount = xmlReader.ReadElementContentAsInt();
                            break;
                        default:
                            base.ReportErrorUnrecognizedElement(xmlReader);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Deserialize an Axis.
        /// </summary>
        /// <param name="xmlReader"></param>
        /// <param name="printerViewModel"></param>
        private void LoadAxis(XmlReader xmlReader, PrinterViewModel printerViewModel)
        {
            //Name of the new Axis.
            string axisName = xmlReader.GetAttribute("Name");

            //AxisID of the new Axis can only be 'Z' as only Z Axis can be created and removed.

            //Reference of the new Axis that will be deserialized.
            AxisViewModel newAxis = null;

            //See if there is a matching Axis. 
            bool matchingAxis = false;
            foreach (AxisViewModel axisViewModel in printerViewModel.AxisViewModelList)
            {
                if (axisViewModel.Name == axisName)
                {
                    matchingAxis = true;
                    newAxis = axisViewModel;
                    break;
                }
            }

            //If there is no matching Axis, then create a new Axis.
            if (matchingAxis == false)
            {
                newAxis = printerViewModel.AddZAxis(axisName);
            }

            //Loads the new Axis with properties from XML and remembers the newly deserialized Axis.
            if (newAxis != null)
            {
                _axisXMLDeserializerModel.DeserializeAxis(xmlReader, newAxis);
                _deserializedAxisViewModelsList.Add(newAxis);
            }
        }

        /// <summary>
        /// Deserialize a Printhead.
        /// </summary>
        /// <param name="xmlReader"></param>
        /// <param name="printerViewModel"></param>
        private void LoadPrinthead(XmlReader xmlReader, PrinterViewModel printerViewModel)
        {
            //Name of the new Printhead.
            string printheadName = xmlReader.GetAttribute("Name");

            //Reference of the new Printhead that will be deserialized.
            PrintheadViewModel newPrinthead = null;

            //See if there is a matching Printhead.
            bool matchingPrinthead = false;
            foreach (PrintheadViewModel printheadViewModel in printerViewModel.PrintheadViewModelList)
            {
                if (printheadViewModel.Name == printheadName)
                {
                    matchingPrinthead = true;
                    newPrinthead = printheadViewModel;
                    break;
                }
            }

            //If there is no matching Printhead, then create a new Printhead.
            if (matchingPrinthead == false)
            {
                newPrinthead = printerViewModel.AddPrinthead(printheadName);
            }

            //Loads the new Printhead with properties from XML and remembers the newly deserialized Printhead.
            if (newPrinthead != null)
            {
                _printheadXMLDeserializerModel.DeserializePrinthead(xmlReader, newPrinthead);
                _deserializedPrintheadViewModelsList.Add(newPrinthead);
            }
        }

        /// <summary>
        /// Remove all Axes that were not just deserialized.
        /// </summary>
        /// <param name="printerViewModel"></param>
        private void RemoveNonDeserializedAxes(PrinterViewModel printerViewModel)
        {
            //List of Axes that need to be removed.
            //Axes need to be removed if they are a part of the Printer but were not just deserialized.
            List<AxisViewModel> nonDeserializedZAxisViewModelsList = new List<AxisViewModel>();

            //Check if existing Axis is a deserialized Axis.
            foreach (AxisViewModel existingAxisViewModel in printerViewModel.ZAxisViewModelList)
            {
                bool wasDeserialized = false;
                foreach (AxisViewModel deserializedAxisViewModel in _deserializedAxisViewModelsList)
                {
                    if (existingAxisViewModel.Name == deserializedAxisViewModel.Name)
                    {
                        wasDeserialized = true;
                        break;
                    }
                }

                //If existing Axis was not a deserialized Axis, mark it for removal.
                if (wasDeserialized == false)
                {
                    nonDeserializedZAxisViewModelsList.Add(existingAxisViewModel);
                }
            }

            //Remove all non-deserialized Axes.
            foreach (AxisViewModel nonDeserializedZAxis in nonDeserializedZAxisViewModelsList)
            {
                printerViewModel.RemoveZAxis(nonDeserializedZAxis.Name);
            }
        }

        /// <summary>
        /// Remove all Printheads that were not just deserialized.
        /// </summary>
        /// <param name="printerViewModel"></param>
        private void RemoveNonDeserializedPrintheads(PrinterViewModel printerViewModel)
        {
            //List of Printheads that need to be removed.
            //Printheads need to be removed if they are a part of the Printer but were not just deserialized.
            List<PrintheadViewModel> nonDeserializedPrintheadViewModelsList = new List<PrintheadViewModel>();

            //Check if existing Printhead is a deserialized Printhead.
            foreach (PrintheadViewModel existingPrintheadViewModel in printerViewModel.PrintheadViewModelList)
            {
                bool wasDeserialized = false;
                foreach (PrintheadViewModel deserializedPrintheadViewModel in _deserializedPrintheadViewModelsList)
                {
                    if (existingPrintheadViewModel.Name == deserializedPrintheadViewModel.Name)
                    {
                        wasDeserialized = true;
                        break;
                    }
                }

                //If existing Printhead was not a deserialized Material, mark it for removal.
                if (wasDeserialized == false)
                {
                    nonDeserializedPrintheadViewModelsList.Add(existingPrintheadViewModel);
                }
            }

            //Remove all non-deserialized Printheads.
            foreach (PrintheadViewModel nonDeserializedPrinthead in nonDeserializedPrintheadViewModelsList)
            {
                printerViewModel.RemovePrinthead(nonDeserializedPrinthead.Name);
            }
        }
        #endregion
    }
}
