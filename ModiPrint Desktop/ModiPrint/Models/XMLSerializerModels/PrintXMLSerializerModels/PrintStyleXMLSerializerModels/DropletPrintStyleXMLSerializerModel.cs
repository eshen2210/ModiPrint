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
    public class DropletPrintStyleXMLSerializerModel : PrintStyleXMLSerializerModel
    {
        #region Constructor
        public DropletPrintStyleXMLSerializerModel() : base()
        {

        }
        #endregion

        #region Methods
        /// <summary>
        /// Writes DropletPrintStyleViewModel properties into XML.
        /// </summary>
        /// <param name="printStyleViewModel"></param>
        /// <returns></returns>
        /// /// <remarks>
        /// Outmost element should be "DropletPrintStyle".
        /// Outmost element start and close should be handled by this function's caller.
        /// </remarks>
        public override void SerializePrintStyle(XmlWriter xmlWriter, PrintStyleViewModel printStyleViewModel)
        {
            DropletPrintStyleViewModel dropletPrintStyleViewModel = (DropletPrintStyleViewModel)printStyleViewModel;

            //Outmost element should be "DropletPrintStyle".
            xmlWriter.WriteStartElement("DropletPrintStyle");

            //Motorized Dispense Distance.
            xmlWriter.WriteElementString("MotorizedDispenseDistance", dropletPrintStyleViewModel.MotorizedDispenseDistance.ToString());

            //Motorized Dispense Max Speed.
            xmlWriter.WriteElementString("MotorizedDispenseMaxSpeed", dropletPrintStyleViewModel.MotorizedDispenseMaxSpeed.ToString());

            //Motorized Dispense Acceleration.
            xmlWriter.WriteElementString("MotorizedDispenseAcceleration", dropletPrintStyleViewModel.MotorizedDispenseAcceleration.ToString());

            //Valve Open Time.
            xmlWriter.WriteElementString("ValveOpenTime", dropletPrintStyleViewModel.ValveOpenTime.ToString());

            //Interpolate Distance.
            xmlWriter.WriteElementString("InterpolateDistance", dropletPrintStyleViewModel.InterpolateDistance.ToString());

            //Gradient Shape.
            xmlWriter.WriteElementString("GradientShape", dropletPrintStyleViewModel.GradientShape.ToString());

            //Gradient Scaling.
            xmlWriter.WriteElementString("GradientScaling", dropletPrintStyleViewModel.GradientScaling.ToString());

            //Percent per Mm.
            xmlWriter.WriteElementString("PercentPerMm", dropletPrintStyleViewModel.PercentPerMm.ToString());

            //Gradient Coordinates.
            xmlWriter.WriteElementString("X1", dropletPrintStyleViewModel.X1.ToString());
            xmlWriter.WriteElementString("Y1", dropletPrintStyleViewModel.Y1.ToString());
            xmlWriter.WriteElementString("Z1", dropletPrintStyleViewModel.Z1.ToString());
            xmlWriter.WriteElementString("X2", dropletPrintStyleViewModel.X2.ToString());
            xmlWriter.WriteElementString("Y2", dropletPrintStyleViewModel.Y2.ToString());
            xmlWriter.WriteElementString("Z2", dropletPrintStyleViewModel.Z2.ToString());
            xmlWriter.WriteElementString("X3", dropletPrintStyleViewModel.X3.ToString());
            xmlWriter.WriteElementString("Y3", dropletPrintStyleViewModel.Y3.ToString());
            xmlWriter.WriteElementString("Z3", dropletPrintStyleViewModel.Z3.ToString());

            //Close outmost element "DropletPrintStyle".
            xmlWriter.WriteEndElement();
        }
        #endregion
    }
}
