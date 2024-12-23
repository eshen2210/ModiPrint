using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using ModiPrint.Models.PrintModels.PrintStyleModels;
using ModiPrint.Models.XMLSerializerModels.PrintXMLSerializerModels;
using ModiPrint.ViewModels;
using ModiPrint.ViewModels.PrintViewModels.PrintStyleViewModels;

namespace ModiPrint.Models.XMLSerializerModels.PrintXMLSerializerModels.PrintStyleXMLSerializerModels
{
    public class ContinuousPrintStyleXMLDeserializerModel : PrintStyleXMLDeserializerModel
    {
        #region Constructor
        public ContinuousPrintStyleXMLDeserializerModel(ErrorListViewModel ErrorListViewModel) : base(ErrorListViewModel)
        {

        }
        #endregion

        #region Methods
        /// <summary>
        /// Reads ContinuousPrintStyleViewModel properties from XML and creates a ContinuousPrintStyleViewModel with said properties.
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public override void DeserializePrintStyle(XmlReader xmlReader, PrintStyleViewModel printStyleViewModel)
        {
            ContinuousPrintStyleViewModel continuousPrintStyleViewModel = (ContinuousPrintStyleViewModel)printStyleViewModel;

            //Read through each element of the XML string and populate each property of the Continuous Print Style.
            while (xmlReader.Read())
            {
                //Skip through newlines (this program's XML Writer uses newlines).
                if ((xmlReader.Name != "\n") && (!String.IsNullOrWhiteSpace(xmlReader.Name)))
                {
                    //End method if the end of "ContinuousPrintStyle" element is reached.
                    if ((xmlReader.Name == "ContinuousPrintStyle") && (xmlReader.NodeType == XmlNodeType.EndElement))
                    {
                        return;
                    }

                    switch (xmlReader.Name)
                    {
                        case "MotorizedDispensePerMmMovement":
                            continuousPrintStyleViewModel.MotorizedDispensePerMmMovement = xmlReader.ReadElementContentAsDouble();
                            break;
                        case "MotorizedDispenseRetractionDistance":
                            continuousPrintStyleViewModel.MotorizedDispenseRetractionDistance = xmlReader.ReadElementContentAsDouble();
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
