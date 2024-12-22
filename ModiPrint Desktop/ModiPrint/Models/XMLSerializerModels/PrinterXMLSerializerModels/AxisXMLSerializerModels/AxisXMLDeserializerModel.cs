using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using ModiPrint.Models.XMLSerializerModels;
using ModiPrint.Models.PrinterModels.AxisModels;
using ModiPrint.ViewModels;
using ModiPrint.ViewModels.PrinterViewModels;
using ModiPrint.ViewModels.PrinterViewModels.AxisViewModels;
using ModiPrint.ViewModels.PrinterViewModels.MicrocontrollerViewModels;

namespace ModiPrint.Models.XMLSerializerModels.PrinterXMLSerializerModels.AxisXMLSerializerModels
{
    public class AxisXMLDeserializerModel : XMLConverterModel
    {
        #region Fields and Properties
        //Contains information of all other Axes.
        PrinterViewModel _printerViewModel;
        #endregion

        #region Constructor
        public AxisXMLDeserializerModel(PrinterViewModel PrinterViewModel, ErrorListViewModel ErrorListViewModel) : base(ErrorListViewModel)
        {
            _printerViewModel = PrinterViewModel;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Reads AxisViewModel properties from XML and sets an AxisViewModel with said properties.
        /// </summary>
        /// <param name="xmlReader"></param>
        /// <param name="axisViewModel"></param>
        public void DeserializeAxis(XmlReader xmlReader, AxisViewModel axisViewModel)
        {
            //Read through each element of the XML string and populate each property of the Axis.
            while (xmlReader.Read())
            {
                //Skip through newlines (this program's XML Writer uses newlines).
                if ((xmlReader.Name != "\n") && (!String.IsNullOrWhiteSpace(xmlReader.Name)))
                {
                    //End method if the end of "Axis" element is reached.
                    if ((xmlReader.Name == "Axis") && (xmlReader.NodeType == XmlNodeType.EndElement))
                    {
                        return;
                    }

                    switch (xmlReader.Name)
                    {
                        case "Name": //See if there is a mismatch between the Axis argument and the deserialized Axis. 
                            if (xmlReader.ReadElementContentAsString() != axisViewModel.Name)
                            { base.ReportErrorMismatchedEqupment(xmlReader); }
                            break;
                        case "AxisID": //See if there is a mismatch between the Axis argument and the deserialized Axis.
                            if (xmlReader.ReadElementContentAsString() != axisViewModel.AxisID.ToString())
                            { base.ReportErrorMismatchedEqupment(xmlReader); }
                            break;
                        case "MotorStepPin":
                            axisViewModel.AttachedMotorStepGPIOPinViewModel = _printerViewModel.MicrocontrollerViewModel.FindPin(xmlReader.ReadElementContentAsInt());
                            axisViewModel.ExecuteRefreshPinBySettingListCommand("MotorStepPinViewModelList");
                            break;
                        case "MotorDirectionPin":
                            axisViewModel.AttachedMotorDirectionGPIOPinViewModel = _printerViewModel.MicrocontrollerViewModel.FindPin(xmlReader.ReadElementContentAsInt());
                            axisViewModel.ExecuteRefreshPinBySettingListCommand("MotorDirectionPinViewModelList");
                            break;
                        case "StepPulseTime":
                            axisViewModel.StepPulseTime = xmlReader.ReadElementContentAsInt();
                            break;
                        case "LimitSwitchPin":
                            axisViewModel.AttachedLimitSwitchGPIOPinViewModel = _printerViewModel.MicrocontrollerViewModel.FindPin(xmlReader.ReadElementContentAsInt());
                            axisViewModel.ExecuteRefreshPinBySettingListCommand("LimitSwitchPinViewModelList");
                            break;
                        case "MaxSpeed":
                            axisViewModel.MaxSpeed = xmlReader.ReadElementContentAsDouble();
                            break;
                        case "MaxAcceleration":
                            axisViewModel.MaxAcceleration = xmlReader.ReadElementContentAsDouble();
                            break;
                        case "MmPerStep":
                            axisViewModel.MmPerStep = xmlReader.ReadElementContentAsDouble();
                            break;
                        case "MaxPosition":
                            axisViewModel.MaxPosition = xmlReader.ReadElementContentAsDouble();
                            break;
                        case "MinPosition":
                            axisViewModel.MinPosition = xmlReader.ReadElementContentAsDouble();
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
