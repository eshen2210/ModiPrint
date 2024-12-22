using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using ModiPrint.Models.XMLSerializerModels.PrintXMLSerializerModels.PrintStyleXMLSerializerModels;
using ModiPrint.Models.XMLSerializerModels.PrintXMLSerializerModels.MaterialXMLSerializerModels;
using ModiPrint.ViewModels.GCodeManagerViewModels;
using ModiPrint.ViewModels.PrintViewModels;
using ModiPrint.ViewModels.PrintViewModels.MaterialViewModels;

namespace ModiPrint.Models.XMLSerializerModels.PrintXMLSerializerModels
{
    public class PrintXMLSerializerModel
    {
        #region Fields and Properties
        //Contains functions to generate XML from Print equipment classes.
        MaterialXMLSerializerModel _materialXMLSerializerModel;
        #endregion

        #region Constructor
        public PrintXMLSerializerModel()
        {
            _materialXMLSerializerModel = new MaterialXMLSerializerModel();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Writes PrintViewModel properies into XML.
        /// </summary>
        /// <param name="xmlWriter"></param>
        /// <param name="printViewModel"></param>
        public void SerializePrint(XmlWriter xmlWriter, PrintViewModel printViewModel)
        {
            //Outmost element should be "Print".
            xmlWriter.WriteStartElement("Print");

            //Output XML for Materials.
            foreach (MaterialViewModel materialViewModel in printViewModel.MaterialViewModelList)
            {
                _materialXMLSerializerModel.SerializeMaterial(xmlWriter, materialViewModel);
            }

            //Number of Materials Created.
            xmlWriter.WriteElementString("MaterialsCreatedCount", printViewModel.MaterialsCreatedCount.ToString());

            //Close outmost element "Print".
            xmlWriter.WriteEndElement();
        }
        #endregion
    }
}
