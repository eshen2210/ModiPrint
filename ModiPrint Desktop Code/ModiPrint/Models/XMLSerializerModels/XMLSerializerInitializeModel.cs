using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using ModiPrint.ViewModels;

namespace ModiPrint.Models.XMLSerializerModels
{
    public class XMLSerializerInitializeModel : XMLConverterModel
    {
        #region Fields and Properties
        //Provides functions for writing XML.
        //StringWriter helps XmlWriter output its XML in string format.
        protected StringWriter _stringWriter;
        protected XmlWriterSettings _xmlWriterSettings;
        protected XmlWriter _xmlWriter;
        #endregion

        #region Constructor
        public XMLSerializerInitializeModel(ErrorListViewModel ErrorListViewModel) : base(ErrorListViewModel)
        {
            //StringWriter and XmlWriter will be instantiated and disposed of in derived class methods.

            //XML Writer Settings.
            _xmlWriterSettings = new XmlWriterSettings();
            _xmlWriterSettings.Indent = true;
            _xmlWriterSettings.NewLineOnAttributes = true;
            _xmlWriterSettings.ConformanceLevel = ConformanceLevel.Fragment;
            _xmlWriterSettings.OmitXmlDeclaration = true;
            _xmlWriterSettings.CloseOutput = true;
        }
        #endregion
    }
}
