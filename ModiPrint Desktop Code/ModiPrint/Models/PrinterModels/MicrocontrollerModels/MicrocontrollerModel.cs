using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModiPrint.Models.PrinterModels.MicrocontrollerModels.PinModels;

namespace ModiPrint.Models.PrinterModels.MicrocontrollerModels
{
    public class MicrocontrollerModel
    {
        #region Fields and Properties
        //List of all GPIO Pins on the microcontroller.
        private List<GPIOPinModel> _gPIOPinModelList = new List<GPIOPinModel>();
        public List<GPIOPinModel> GPIOPinModelList
        {
            get { return _gPIOPinModelList; }
        }
        #endregion

        #region Constructor
        public MicrocontrollerModel()
        {            
            //Create PWN Pins
            for (int pinID = 2; pinID <= 13; pinID++)
            {
                string pinName = pinID.ToString();
                //string pinName = "PWM " + pinID;
                _gPIOPinModelList.Add(new PWMPinModel(pinName, pinID));
            }

            //Create Communication Pins
            _gPIOPinModelList.Add(new CommunicationPinModel("1", 1));
            _gPIOPinModelList.Add(new CommunicationPinModel("0", 1));
            for (int pinID = 14; pinID <= 21; pinID++)
            {
                string pinName = pinID.ToString();
                _gPIOPinModelList.Add(new PWMPinModel(pinName, pinID));
            }
            /*
            _gPIOPinModelList.Add(new CommunicationPinModel("TX0 1", 1));
            _gPIOPinModelList.Add(new CommunicationPinModel("RX0 0", 0));
            _gPIOPinModelList.Add(new CommunicationPinModel("TX3 14", 14));
            _gPIOPinModelList.Add(new CommunicationPinModel("RX3 15", 15));
            _gPIOPinModelList.Add(new CommunicationPinModel("TX2 16", 16));
            _gPIOPinModelList.Add(new CommunicationPinModel("RX2 17", 17));
            _gPIOPinModelList.Add(new CommunicationPinModel("TX1 18", 18));
            _gPIOPinModelList.Add(new CommunicationPinModel("RX1 19", 19));
            _gPIOPinModelList.Add(new CommunicationPinModel("SDA 20", 20));
            _gPIOPinModelList.Add(new CommunicationPinModel("SDA 21", 21));
            */

            //Create AnalogIn Pins
            for (int pinID = 54; pinID <= 69; pinID++)
            {
                string pinName = pinID.ToString();
                //string pinName = "AnalogIn " + pinID;
                _gPIOPinModelList.Add(new AnalogInPinModel(pinName, pinID));
            }

            //Create Digital Pins
            for (int pinID = 22; pinID <= 53; pinID++)
            {
                string pinName = pinID.ToString();
                //string pinName = "Digital " + pinID;
                _gPIOPinModelList.Add(new DigitalPinModel(pinName, pinID));
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Finds and returns a reference to a GPIOPin with the matching pin name.
        /// </summary>
        public GPIOPinModel FindPin(string name)
        {
            foreach (GPIOPinModel gPIOPinModel in GPIOPinModelList)
            {
                if (gPIOPinModel.Name == name)
                {
                    return gPIOPinModel;
                }
            }
            return null;
        }

        /// <summary>
        /// If all parameters are set appropriately for a print, then return true.
        /// </summary>
        public bool ReadyToPrint()
        {
            //True by default.
            //Unless some features are implemented later, there is nothing to check.
            return true;
        }
        #endregion
    }
}
