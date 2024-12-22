using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using ModiPrint.Models.PrintModels.PrintStyleModels.GradientModels;
using ModiPrint.ViewModels;
using ModiPrint.ViewModels.PrintViewModels.PrintStyleViewModels;

namespace ModiPrint.Models.XMLSerializerModels.PrintXMLSerializerModels.PrintStyleXMLSerializerModels
{
    public class DropletPrintStyleXMLDeserializerModel : PrintStyleXMLDeserializerModel
    {
        #region Constructor
        public DropletPrintStyleXMLDeserializerModel(ErrorListViewModel ErrorListViewModel) : base(ErrorListViewModel)
        {

        }
        #endregion

        #region Methods
        /// <summary>
        /// Reads DropletPrintStyleViewModel properties from XML and creates a DropletPrintStyleViewModel with said properties.
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public override void DeserializePrintStyle(XmlReader xmlReader, PrintStyleViewModel printStyleViewModel)
        {
            DropletPrintStyleViewModel dropletPrintStyleViewModel = (DropletPrintStyleViewModel)printStyleViewModel;

            //Read through each element of the XML string and populate each property of the Droplet Print Style.
            while (xmlReader.Read())
            {
                //Skip through newlines (this program's XML Writer uses newlines).
                if ((xmlReader.Name != "\n") && (!String.IsNullOrWhiteSpace(xmlReader.Name)))
                {
                    //End method if the end of "DropletPrintStyle" element is reached.
                    if ((xmlReader.Name == "DropletPrintStyle") && (xmlReader.NodeType == XmlNodeType.EndElement))
                    {
                        return;
                    }

                    switch (xmlReader.Name)
                    {
                        case "MotorizedDispenseDistance":
                            dropletPrintStyleViewModel.MotorizedDispenseDistance = xmlReader.ReadElementContentAsDouble();
                            break;
                        case "MotorizedDispenseMaxSpeed":
                            dropletPrintStyleViewModel.MotorizedDispenseMaxSpeed = xmlReader.ReadElementContentAsDouble();
                            break;
                        case "MotorizedDispenseAcceleration":
                            dropletPrintStyleViewModel.MotorizedDispenseAcceleration = xmlReader.ReadElementContentAsDouble();
                            break;
                        case "ValveOpenTime":
                            dropletPrintStyleViewModel.ValveOpenTime = xmlReader.ReadElementContentAsInt();
                            break;
                        case "InterpolateDistance":
                            dropletPrintStyleViewModel.InterpolateDistance = xmlReader.ReadElementContentAsDouble();
                            break;
                        case "GradientShape":
                            dropletPrintStyleViewModel.GradientShape = (GradientShape)Enum.Parse(typeof(GradientShape), xmlReader.ReadElementContentAsString(), false);
                            break;
                        case "GradientScaling":
                            dropletPrintStyleViewModel.GradientScaling = (GradientScaling)Enum.Parse(typeof(GradientScaling), xmlReader.ReadElementContentAsString(), false);
                            break;
                        case "PercentPerMm":
                            dropletPrintStyleViewModel.PercentPerMm = xmlReader.ReadElementContentAsDouble();
                            break;
                        case "X1":
                            dropletPrintStyleViewModel.X1 = xmlReader.ReadElementContentAsDouble();
                            break;
                        case "Y1":
                            dropletPrintStyleViewModel.Y1 = xmlReader.ReadElementContentAsDouble();
                            break;
                        case "Z1":
                            dropletPrintStyleViewModel.Z1 = xmlReader.ReadElementContentAsDouble();
                            break;
                        case "X2":
                            dropletPrintStyleViewModel.X2 = xmlReader.ReadElementContentAsDouble();
                            break;
                        case "Y2":
                            dropletPrintStyleViewModel.Y2 = xmlReader.ReadElementContentAsDouble();
                            break;
                        case "Z2":
                            dropletPrintStyleViewModel.Z2 = xmlReader.ReadElementContentAsDouble();
                            break;
                        case "X3":
                            dropletPrintStyleViewModel.X3 = xmlReader.ReadElementContentAsDouble();
                            break;
                        case "Y3":
                            dropletPrintStyleViewModel.Y3 = xmlReader.ReadElementContentAsDouble();
                            break;
                        case "Z3":
                            dropletPrintStyleViewModel.Z3 = xmlReader.ReadElementContentAsDouble();
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
