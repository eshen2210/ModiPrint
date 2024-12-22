using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModiPrint.DataTypes.GlobalValues
{
    public static class GlobalValues
    {
        //PinID property of a GPIOPin that has not been set.
        //Because 0 is a valid PinID.
        private static int _nullPinID = 70;
        public static int PinIDNull
        {
            get { return _nullPinID; }
        }

        //Baud rate for serial communication.
        private static int _baudRate = 115200; //115200 is chosen for its low error rate with the Mega2560.
        public static int BaudRate
        {
            get { return _baudRate; }
        }

        //Maximum number of characters in a serial incoming message.
        private static int _incomingSerialBuffer = 64;
        public static int IncomingSerialBuffer
        {
            get { return _incomingSerialBuffer; }
        }

        //The distance which actuators move away from the Limit Switch after hitting the Limit.
        //In mm.
        private static double _limitBuffer = 7.5;
        public static double LimitBuffer
        {
            get { return _limitBuffer; }
        }
    }
}
