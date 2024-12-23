using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using ModiPrint.Models.XMLSerializerModels.PrintXMLSerializerModels;
using ModiPrint.ViewModels.PrintViewModels.PrintStyleViewModels;

namespace ModiPrint.Models.XMLSerializerModels.PrintXMLSerializerModels.PrintStyleXMLSerializerModels
{
    public class ContinuousPrintStyleXMLSerializerModel : PrintStyleXMLSerializerModel
    {
        #region Constructor
        public ContinuousPrintStyleXMLSerializerModel() : base()
        {

        }
        #endregion

        #region Methods
        /// <summary>
        /// Writes Continuous Print Style properties into XML.
        /// </summary>
        /// <param name="printStyleViewModel"></param>
        /// <returns></returns>
        /// <remarks>
        /// Outmost element should be "ContinuousPrintStyle".
        /// Outmost element start and close should be handled by this function's caller.
        /// </remarks>
        public override void SerializePrintStyle(XmlWriter xmlWriter, PrintStyleViewModel printStyleViewModel)
        {
            ContinuousPrintStyleViewModel continuousPrintStyleViewModel = (ContinuousPrintStyleViewModel)printStyleViewModel;

            //Only element should be "ContinousPrintStyle".
            xmlWriter.WriteStartElement("ContinuousPrintStyle", " ");

            //Motorized Dispense Per Mm Movement.
            xmlWriter.WriteElementString("MotorizedDispensePerMmMovement", continuousPrintStyleViewModel.MotorizedDispensePerMmMovement.ToString());

            //Motorized Retraction Distance.
            xmlWriter.WriteElementString("MotorizedDispenseRetractionDistance", continuousPrintStyleViewModel.MotorizedDispenseRetractionDistance.ToString());

            //Close outmost element "ContinousPrintStyle".
            xmlWriter.WriteEndElement();
        }
        #endregion
    }
}
