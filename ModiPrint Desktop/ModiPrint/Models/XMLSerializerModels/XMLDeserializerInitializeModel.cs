using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using ModiPrint.Models.XMLSerializerModels;
using ModiPrint.ViewModels;

namespace ModiPrint.Models.XMLSerializerModels
{
    public abstract class XMLDeserializerInitializeModel :  XMLConverterModel
    {
        #region Fields and Properties
        //Provides functions for reading XML.
        //StringReader helps XmlReader take strings as input. 
        protected StringReader _stringReader;
        protected XmlReaderSettings _xmlReaderSettings;
        protected XmlReader _xmlReader;
        #endregion

        #region Constructor
        public XMLDeserializerInitializeModel(ErrorListViewModel ErrorListViewModel) : base(ErrorListViewModel)
        {
            //XmlReader will be instantiated and disposed of in derived class methods.

            //XML Reader Settings.
            _xmlReaderSettings = new XmlReaderSettings();
            _xmlReaderSettings.ConformanceLevel = ConformanceLevel.Fragment;
            _xmlReaderSettings.CloseInput = true;
        }
        #endregion
    }
}
