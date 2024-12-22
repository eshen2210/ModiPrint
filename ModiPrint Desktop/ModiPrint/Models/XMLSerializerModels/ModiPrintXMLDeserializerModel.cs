using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using ModiPrint.ViewModels;
using ModiPrint.ViewModels.GCodeManagerViewModels;
using ModiPrint.ViewModels.PrintViewModels;
using ModiPrint.ViewModels.PrintViewModels.MaterialViewModels;
using ModiPrint.ViewModels.PrinterViewModels;
using ModiPrint.ViewModels.PrinterViewModels.MicrocontrollerViewModels;
using ModiPrint.Models.XMLSerializerModels;
using ModiPrint.Models.XMLSerializerModels.GCodeXMLSerializerModels;
using ModiPrint.Models.XMLSerializerModels.PrinterXMLSerializerModels;
using ModiPrint.Models.XMLSerializerModels.PrintXMLSerializerModels;

namespace ModiPrint.Models.XMLSerializerModels
{
    public class ModiPrintXMLDeserializerModel : XMLDeserializerInitializeModel 
    {
        #region Fields and Properties
        //Classes to deserialize GCode, Print, and Printer XML.
        private GCodeManagerXMLDeserializerModel _gCodeManagerXMLDeserializerModel;
        private PrinterXMLDeserializerModel _printerXMLDeserializerModel;
        private PrintXMLDeserializerModel _printXMLDeserializerModel;

        //Classes requied by the XML Deserializers.
        private GCodeManagerViewModel _gCodeManagerViewModel;
        private PrinterViewModel _printerViewModel;
        private GPIOPinListsViewModel _gPIOPinListsViewModel;
        private PrintViewModel _printViewModel;
        #endregion

        #region Constructor
        public ModiPrintXMLDeserializerModel(GCodeManagerViewModel GCodeManagerViewModel, PrinterViewModel PrinterViewModel, GPIOPinListsViewModel GPIOPinListsViewModel, PrintViewModel PrintViewModel,
            ErrorListViewModel ErrorListViewModel) : base(ErrorListViewModel)
        {
            //ModiPrint parts.
            _gCodeManagerViewModel = GCodeManagerViewModel;
            _printerViewModel = PrinterViewModel;
            _gPIOPinListsViewModel = GPIOPinListsViewModel;
            _printViewModel = PrintViewModel;

            //XML Serializers.
            _gCodeManagerXMLDeserializerModel = new GCodeManagerXMLDeserializerModel(_errorListViewModel);
            _printerXMLDeserializerModel = new PrinterXMLDeserializerModel(_gPIOPinListsViewModel, _printerViewModel, _errorListViewModel);
            _printXMLDeserializerModel = new PrintXMLDeserializerModel(_printerViewModel, _errorListViewModel);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Loads saved XML user inputs containing properties of the Printer, Print, and GCode.
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="printerViewModel"></param>
        /// <param name="printViewModel"></param>
        /// <param name="gCodeManagerViewModel"></param>
        public void DeserializeModiPrint(string xml, PrinterViewModel printerViewModel, PrintViewModel printViewModel, GCodeManagerViewModel gCodeManagerViewModel)
        {
            using (base._stringReader = new StringReader(xml))
            {
                using (base._xmlReader = XmlReader.Create(base._stringReader, base._xmlReaderSettings))
                {
                    //Ensure that the outmost element is "ModiPrint".
                    if (!(base._xmlReader.Read()) 
                     || !(base._xmlReader.Name == "ModiPrint") 
                     || !(base._xmlReader.NodeType == XmlNodeType.Element))
                    {
                        return;
                    }

                    //Read through each line of the XML.
                    while (base._xmlReader.Read())
                    {   
                        //Skip through newlines (this program's XML Writer uses newlines).
                        if ((base._xmlReader.Name != "\n") && (!String.IsNullOrWhiteSpace(base._xmlReader.Name)))
                        {   
                            //End method if the end of "ModiPrint" element is reached.
                            if ((base._xmlReader.Name == "ModiPrint") && (base._xmlReader.NodeType == XmlNodeType.EndElement))
                            {
                                return;
                            }

                            //Deserialize and populate each property of the Print.
                            switch (base._xmlReader.Name)
                            {
                                case "GCodeManager":
                                    _gCodeManagerXMLDeserializerModel.DeserializeGCode(base._xmlReader, _gCodeManagerViewModel);
                                    break;
                                case "Printer":
                                    _printerXMLDeserializerModel.DeserializePrinter(base._xmlReader, _printerViewModel);
                                    break;
                                case "Print":
                                    _printXMLDeserializerModel.DeserializePrint(base._xmlReader, _printViewModel);
                                    break;
                                default:
                                    base.ReportErrorUnrecognizedElement(base._xmlReader);
                                    break;
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}
