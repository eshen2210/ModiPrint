using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using ModiPrint.Models.PrinterModels.PrintheadModels;
using ModiPrint.Models.PrinterModels.PrintheadModels.PrintheadTypeModels;
using ModiPrint.Models.XMLSerializerModels.PrinterXMLSerializerModels;
using ModiPrint.ViewModels;
using ModiPrint.ViewModels.PrinterViewModels.MicrocontrollerViewModels;
using ModiPrint.ViewModels.PrinterViewModels.PrintheadViewModels.PrintheadTypeViewModels;

namespace ModiPrint.Models.XMLSerializerModels.PrinterXMLSerializerModels.PrintheadXMLSerializerModels.PrintheadTypeXMLSerializerModels
{
    public class MotorizedPrintheadTypeXMLDeserializer : PrintheadTypeXMLDeserializerModel
    {
        #region Contructor
        public MotorizedPrintheadTypeXMLDeserializer(MicrocontrollerViewModel MicrocontrollerViewModel, ErrorListViewModel ErrorListViewModel) : base(MicrocontrollerViewModel, ErrorListViewModel)
        {

        }
        #endregion

        #region Methods
        /// <summary>
        /// Deserializes an XML file with Motorized Printhead Type properties then stores those properties into the input Valve PrintheadType.
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="printheadTypeViewModel"></param>
        public override void DeserializePrintheadType(XmlReader xmlReader, PrintheadTypeViewModel printheadTypeViewModel)
        {
            MotorizedPrintheadTypeViewModel motorizedPrintheadTypeViewModel = (MotorizedPrintheadTypeViewModel)printheadTypeViewModel;

            while (xmlReader.Read())
            {
                //Skip through newlines (this program's XML Writer uses newlines).
                if ((xmlReader.Name != "\n") && (!String.IsNullOrWhiteSpace(xmlReader.Name)))
                {
                    //End method if the end of "MotorizedPrintheadType" element is reached.
                    if ((xmlReader.Name == "MotorizedPrintheadType") && (xmlReader.NodeType == XmlNodeType.EndElement))
                    {
                        return;
                    }

                    switch (xmlReader.Name)
                    {
                        case "MotorStepPin":
                            motorizedPrintheadTypeViewModel.AttachedMotorStepGPIOPinViewModel = base._microcontrollerViewModel.FindPin(xmlReader.ReadElementContentAsInt());
                            motorizedPrintheadTypeViewModel.ExecuteRefreshPinBySettingListCommand("MotorStepPinViewModelList");
                            break;
                        case "MotorDirectionPin":
                            motorizedPrintheadTypeViewModel.AttachedMotorDirectionGPIOPinViewModel = base._microcontrollerViewModel.FindPin(xmlReader.ReadElementContentAsInt());
                            motorizedPrintheadTypeViewModel.ExecuteRefreshPinBySettingListCommand("MotorDirectionPinViewModelList");
                            break;
                        case "StepPulseTime":
                            motorizedPrintheadTypeViewModel.StepPulseTime = xmlReader.ReadElementContentAsInt();
                            break;
                        case "LimitSwitchPin":
                            motorizedPrintheadTypeViewModel.AttachedLimitSwitchGPIOPinViewModel = base._microcontrollerViewModel.FindPin(xmlReader.ReadElementContentAsInt());
                            motorizedPrintheadTypeViewModel.ExecuteRefreshPinBySettingListCommand("LimitSwitchPinViewModelList");
                            break;
                        case "MaxSpeed":
                            motorizedPrintheadTypeViewModel.MaxSpeed = xmlReader.ReadElementContentAsDouble();
                            break;
                        case "MaxAcceleration":
                            motorizedPrintheadTypeViewModel.MaxAcceleration = xmlReader.ReadElementContentAsDouble();
                            break;
                        case "MmPerStep":
                            motorizedPrintheadTypeViewModel.MmPerStep = xmlReader.ReadElementContentAsDouble();
                            break;
                        case "MaxPosition":
                            motorizedPrintheadTypeViewModel.MaxPosition = xmlReader.ReadElementContentAsDouble();
                            break;
                        case "MinPosition":
                            motorizedPrintheadTypeViewModel.MinPosition = xmlReader.ReadElementContentAsDouble();
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
