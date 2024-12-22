using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using ModiPrint.Models.PrintModels.PrintStyleModels;
using ModiPrint.Models.XMLSerializerModels.PrintXMLSerializerModels.PrintStyleXMLSerializerModels;
using ModiPrint.ViewModels.PrintViewModels.MaterialViewModels;

namespace ModiPrint.Models.XMLSerializerModels.PrintXMLSerializerModels.MaterialXMLSerializerModels
{
    public class MaterialXMLSerializerModel
    {
        #region Constructor
        public MaterialXMLSerializerModel()
        {

        }
        #endregion

        #region Methods
        /// <summary>
        /// Writes MaterialViewModel properties into a XmlWriter.
        /// </summary>
        /// <param name="xmlWriter"></param>
        /// <param name="materialViewModel"></param>
        public void SerializeMaterial(XmlWriter xmlWriter, MaterialViewModel materialViewModel)
        {
            //Outmost element should be "Material".
            xmlWriter.WriteStartElement("Material");

            //Outmost element should contain properties required for identification and instantiation.
            //Name.
            xmlWriter.WriteAttributeString("Name", materialViewModel.Name);

            //RepRap ID.
            xmlWriter.WriteElementString("RepRapID", materialViewModel.RepRapID);

            //Printhead.
            if (materialViewModel.PrintheadViewModel != null)
            { xmlWriter.WriteElementString("PrintheadName", materialViewModel.PrintheadViewModel.Name); }

            //Print Style.
            PrintStyleXMLSerializerModel printStyleXMLSerializerModel = null;
            switch (materialViewModel.PrintStyle)
            {
                case PrintStyle.Continuous:
                    printStyleXMLSerializerModel = new ContinuousPrintStyleXMLSerializerModel();
                    break;
                case PrintStyle.Droplet:
                    printStyleXMLSerializerModel = new DropletPrintStyleXMLSerializerModel();
                    break;
                case PrintStyle.Unset:
                    xmlWriter.WriteElementString("UnsetPrintStyle", "");
                    break;
            }
            if (printStyleXMLSerializerModel != null)
            { printStyleXMLSerializerModel.SerializePrintStyle(xmlWriter, materialViewModel.PrintStyleViewModel); }

            //Pause After Activating.
            xmlWriter.WriteElementString("PauseAfterActivating", materialViewModel.PauseAfterActivating.ToString());

            //Pause Before Deactivating.
            xmlWriter.WriteElementString("PauseBeforeDeactivating", materialViewModel.PauseBeforeDeactivating.ToString());

            //Junction Deviation.
            xmlWriter.WriteElementString("JunctionDeviation", materialViewModel.JunctionDeviation.ToString());

            //XY Print Speed.
            xmlWriter.WriteElementString("XYPrintSpeed", materialViewModel.XYPrintSpeed.ToString());

            //Z Print Speed.
            xmlWriter.WriteElementString("ZPrintSpeed", materialViewModel.ZPrintSpeed.ToString());

            //XY Print Acceleration.
            xmlWriter.WriteElementString("XYPrintAcceleration", materialViewModel.XYPrintAcceleration.ToString());

            //Z Print Acceleration.
            xmlWriter.WriteElementString("ZPrintAcceleration", materialViewModel.ZPrintAcceleration.ToString());

            //End "Material" outmost element.
            xmlWriter.WriteEndElement();
        }
        #endregion
    }
}
