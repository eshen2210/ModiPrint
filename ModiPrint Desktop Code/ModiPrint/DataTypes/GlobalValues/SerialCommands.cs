using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModiPrint.DataTypes.GlobalValues
{
    public class SerialCommands
    {
        //Check Connection.
        private static string _checkConnection = "C00";
        public static string CheckConnection
        {
            get { return _checkConnection; }
        }

        //Set Axis.
        private static string _setAxis = "C10";
        public static string SetAxis
        {
            get { return _setAxis; }
        }

        //Set Motor-Driven Printhead.
        private static string _setMotorPrinthead = "C11";
        public static string SetMotorPrinthead
        {
            get { return _setMotorPrinthead; }
        }

        //Set Valve Printhead.
        private static string _setValvePrinthead = "C12";
        public static string SetValvePrinthead
        {
            get { return _setValvePrinthead; }
        }

        //Axes Movement.
        private static string _axesMovement = "G00";
        public static string AxesMovement
        {
             get { return _axesMovement; }
        }

        //Motor Print With Movement.
        private static string _motorPrintWithMovement = "G01";
        public static string MotorPrintWithMovement
        {
            get { return _motorPrintWithMovement; }
        }

        //Valve Print With Movement.
        private static string _valvePrintWithMovement = "G02";
        public static string ValvePrintWithMovement
        {
            get { return _valvePrintWithMovement; }
        }

        //Motor Print Without Movement.
        private static string _motorPrintWithoutMovement = "G11";
        public static string MotorPrintWithoutMovement
        {
            get { return _motorPrintWithoutMovement; }
        }

        //Valve Print WithoutMovement.
        private static string _valvePrintWithoutMovement = "G12";
        public static string ValvePrintWithoutMovement
        {
            get { return _valvePrintWithoutMovement; }
        }
    }
}
