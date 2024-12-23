using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using ModiPrint.ViewModels.PrinterViewModels.AxisViewModels;
using ModiPrint.Models.XMLSerializerModels;

namespace ModiPrint.Models.XMLSerializerModels.PrinterXMLSerializerModels.AxisXMLSerializerModels
{
    public class AxisXMLSerializerModel
    {
        #region Constructor
        public AxisXMLSerializerModel()
        {
            
        }
        #endregion

        #region Methods
        /// <summary>
        /// Writes AxisViewModel properties into a XmlWriter.
        /// </summary>
        /// <param name="xmlWriter"></param>
        /// <param name="axisViewModel"></param>
        public void SerializeAxis(XmlWriter xmlWriter, AxisViewModel axisViewModel)
        {
            //Outmost element should be "Axis".
            xmlWriter.WriteStartElement("Axis");

            //Outmost element should contain properties required for identification and instantiation.
            //Name.
            xmlWriter.WriteAttributeString("Name", axisViewModel.Name);

            //Axis ID.
            xmlWriter.WriteElementString("AxisID", axisViewModel.AxisID.ToString());

            //Motor Step Pin.
            if (axisViewModel.AttachedMotorStepGPIOPinViewModel != null)
            { xmlWriter.WriteElementString("MotorStepPin", axisViewModel.AttachedMotorStepGPIOPinViewModel.PinID.ToString()); }
                    
            //Motor Direction Pin.
            if (axisViewModel.AttachedMotorDirectionGPIOPinViewModel != null)
            { xmlWriter.WriteElementString("MotorDirectionPin", axisViewModel.AttachedMotorDirectionGPIOPinViewModel.PinID.ToString()); }

            //Step Pulse Time.
            xmlWriter.WriteElementString("StepPulseTime", axisViewModel.StepPulseTime.ToString());

            //Limit Switch Pin.
            if (axisViewModel.AttachedLimitSwitchGPIOPinViewModel != null)
            { xmlWriter.WriteElementString("LimitSwitchPin", axisViewModel.AttachedLimitSwitchGPIOPinViewModel.PinID.ToString()); }
                    
            //Max Speed.
            xmlWriter.WriteElementString("MaxSpeed", axisViewModel.MaxSpeed.ToString());

            //Max Acceleration.
            xmlWriter.WriteElementString("MaxAcceleration", axisViewModel.MaxAcceleration.ToString());

            //mm per Step.
            xmlWriter.WriteElementString("MmPerStep", axisViewModel.MmPerStep.ToString());

            //Max Position.
            xmlWriter.WriteElementString("MaxPosition", axisViewModel.MaxPosition.ToString());

            //Min Position.
            xmlWriter.WriteElementString("MinPosition", axisViewModel.MinPosition.ToString());

            //End "Axis" outmost element.
            xmlWriter.WriteEndElement();
        }
        #endregion
    }
}
