using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using ModiPrint.Models.XMLSerializerModels.PrinterXMLSerializerModels;
using ModiPrint.ViewModels.PrinterViewModels.PrintheadViewModels.PrintheadTypeViewModels;

namespace ModiPrint.Models.XMLSerializerModels.PrinterXMLSerializerModels.PrintheadXMLSerializerModels.PrintheadTypeXMLSerializerModels
{
    public class MotorizedPrintheadTypeXMLSerializerModel : PrintheadTypeXMLSerializerModel
    {
        #region Constructor
        public MotorizedPrintheadTypeXMLSerializerModel() : base()
        {

        }
        #endregion

        #region Methods
        /// <summary>
        /// Writes Motorized Printhead Type properties into XML.
        /// </summary>
        /// <param name="printheadTypeViewModel"></param>
        /// <returns></returns>
        public override void SerializePrintheadType(XmlWriter xmlWriter, PrintheadTypeViewModel printheadTypeViewModel)
        {
            if (printheadTypeViewModel != null)
            {
                MotorizedPrintheadTypeViewModel motorizedPrintheadTypeViewModel = (MotorizedPrintheadTypeViewModel)printheadTypeViewModel;

                //Outmost element should be "MotorizedPrintheadType".
                xmlWriter.WriteStartElement("MotorizedPrintheadType");

                //Motor Step Pin.
                if (motorizedPrintheadTypeViewModel.AttachedMotorStepGPIOPinViewModel != null)
                { xmlWriter.WriteElementString("MotorStepPin", motorizedPrintheadTypeViewModel.AttachedMotorStepGPIOPinViewModel.PinID.ToString()); }

                //Motor Direction Pin.
                if (motorizedPrintheadTypeViewModel.AttachedMotorDirectionGPIOPinViewModel != null)
                { xmlWriter.WriteElementString("MotorDirectionPin", motorizedPrintheadTypeViewModel.AttachedMotorDirectionGPIOPinViewModel.PinID.ToString()); }

                //Step Pulse Time.
                xmlWriter.WriteElementString("StepPulseTime", motorizedPrintheadTypeViewModel.StepPulseTime.ToString());

                //Limit Switch Pin.
                if (motorizedPrintheadTypeViewModel.AttachedLimitSwitchGPIOPinViewModel != null)
                { xmlWriter.WriteElementString("LimitSwitchPin", motorizedPrintheadTypeViewModel.AttachedLimitSwitchGPIOPinViewModel.PinID.ToString()); }

                //Max Speed.
                xmlWriter.WriteElementString("MaxSpeed", motorizedPrintheadTypeViewModel.MaxSpeed.ToString());

                //Max Acceleration.
                xmlWriter.WriteElementString("MaxAcceleration", motorizedPrintheadTypeViewModel.MaxAcceleration.ToString());

                //mm per Step.
                xmlWriter.WriteElementString("MmPerStep", motorizedPrintheadTypeViewModel.MmPerStep.ToString());

                //Max Position.
                xmlWriter.WriteElementString("MaxPosition", motorizedPrintheadTypeViewModel.MaxPosition.ToString());

                //Min Position.
                xmlWriter.WriteElementString("MinPosition", motorizedPrintheadTypeViewModel.MinPosition.ToString());

                //Close outmost element "MotorizedPrintheadType".
                xmlWriter.WriteEndElement();
            }
        }
        #endregion
    }
}
