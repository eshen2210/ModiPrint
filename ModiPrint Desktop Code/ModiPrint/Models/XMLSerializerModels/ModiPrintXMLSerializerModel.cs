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
using ModiPrint.ViewModels.PrinterViewModels.AxisViewModels;
using ModiPrint.ViewModels.PrinterViewModels.PrintheadViewModels;
using ModiPrint.Models.XMLSerializerModels.GCodeXMLSerializerModels;
using ModiPrint.Models.XMLSerializerModels.PrinterXMLSerializerModels;
using ModiPrint.Models.XMLSerializerModels.PrintXMLSerializerModels;

namespace ModiPrint.Models.XMLSerializerModels
{
    public class ModiPrintXMLSerializerModel : XMLSerializerInitializeModel
    {
        #region Fields and Properties
        //Classes to deserialize the GCode, Printer, and Print XML.
        private GCodeManagerXMLSerializerModel _gCodeManagerXMLSerializerModel;
        private PrinterXMLSerializerModel _printerXMLSerializerModel;
        private PrintXMLSerializerModel _printXMLSerializerModel;

        //Classes required by the GCode Serializer classes.
        private GCodeManagerViewModel _gCodeManagerViewModel;
        private PrinterViewModel _printerViewModel;
        private PrintViewModel _printViewModel;
        #endregion

        #region Constructor
        public ModiPrintXMLSerializerModel(GCodeManagerViewModel GCodeManagerViewModel, PrinterViewModel PrinterViewModel, PrintViewModel PrintViewModel, ErrorListViewModel ErrorListViewModel) : base(ErrorListViewModel)
        {
            //ModiPrint parts.
            _gCodeManagerViewModel = GCodeManagerViewModel;
            _printerViewModel = PrinterViewModel;
            _printViewModel = PrintViewModel;

            //XML Serializers.
            _gCodeManagerXMLSerializerModel = new GCodeManagerXMLSerializerModel();
            _printerXMLSerializerModel = new PrinterXMLSerializerModel();
            _printXMLSerializerModel = new PrintXMLSerializerModel();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Writes this program's user input into XML.
        /// </summary>
        /// <param name="printerViewModel"></param>
        /// <param name="printViewModel"></param>
        /// <param name="gCodeManagerViewModel"></param>
        /// <returns></returns>
        public string SerializeModiPrint(PrinterViewModel printerViewModel, PrintViewModel printViewModel, GCodeManagerViewModel gCodeManagerViewModel)
        {
            using (base._stringWriter = new StringWriter())
            {
                using (base._xmlWriter = XmlWriter.Create(base._stringWriter, base._xmlWriterSettings))
                {
                    //Outmost element should be "ModiPrint".
                    base._xmlWriter.WriteStartElement("ModiPrint");

                    //GCode.
                    _gCodeManagerXMLSerializerModel.SerializeGCodeManager(base._xmlWriter, _gCodeManagerViewModel);

                    //Printer. 
                    //It's very important to set the Printer before the Print, or else a lot of Printer equipment will not be found to set the Print.
                    _printerXMLSerializerModel.SerializePrinter(base._xmlWriter, _printerViewModel);

                    //Print.
                    _printXMLSerializerModel.SerializePrint(base._xmlWriter, _printViewModel);

                    //End outmost element "ModiPrint".
                    base._xmlWriter.WriteEndElement();
                }

                return base._stringWriter.ToString();
            }
        }
        #endregion
    }
}
