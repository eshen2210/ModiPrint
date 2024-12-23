using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModiPrint.DataTypes.GlobalValues;
using ModiPrint.Models.GCodeConverterModels.ProcessModels.ProcessG00Models;
using ModiPrint.Models.PrintModels.PrintStyleModels.GradientModels;

namespace ModiPrint.Models.GCodeConverterModels.ProcessModels.ProcessG00Models
{
    /// <summary>
    /// Contains methods that output GCode commands based on input parameters.
    /// </summary>
    public static class WriteG00
    {
        #region Methods
        /// <summary>
        /// Returns a string of converted GCode for Motorized Printhead printing with continuous Axis movement.
        /// </summary>
        /// <param name="xmmPerStep"></param>
        /// <param name="ymmPerStep"></param>
        /// <param name="zmmPerStep"></param>
        /// <param name="xDistance"></param>
        /// <param name="yDistance"></param>
        /// <param name="zDistance"></param>
        /// <param name="xInvertDirection"></param>
        /// <param name="yInvertDirection"></param>
        /// <param name="zInvertDirection"></param>
        /// <param name="xRemainder"></param>
        /// <param name="yRemainder"></param>
        /// <param name="zRemainder"></param>
        /// <returns></returns>
        public static List<ConvertedGCodeLine> WriteAxesMovement(
            double xmmPerStep, double ymmPerStep, double zmmPerStep,
            double xDistance, double yDistance, double zDistance,
            bool xInvertDirection, bool yInvertDirection, bool zInvertDirection,
            ref double xRemainder, ref double yRemainder, ref double zRemainder)
        {
            //The return GCode.
            List<ConvertedGCodeLine> convertedGCodeLinesList = new List<ConvertedGCodeLine>();
            string convertedGCodeLine = "";

            if (((xDistance) != 0) || ((yDistance) != 0) || ((zDistance) != 0))
            {
                convertedGCodeLine += SerialCommands.AxesMovement;

                //Output commands that will be sent through the serial port.
                convertedGCodeLine += G00Calculator.WriteSteps('X', xDistance, xmmPerStep, xInvertDirection, ref xRemainder);
                convertedGCodeLine += G00Calculator.WriteSteps('Y', yDistance, ymmPerStep, yInvertDirection, ref yRemainder);
                convertedGCodeLine += G00Calculator.WriteSteps('Z', zDistance, zmmPerStep, zInvertDirection, ref zRemainder);
            }

            if (convertedGCodeLine != SerialCommands.AxesMovement) //If there are steps to take...
            {
                convertedGCodeLinesList.Add(new ConvertedGCodeLine(convertedGCodeLine));
                return convertedGCodeLinesList;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns a string of converted g-code for Motorized Printhead printing with continuous Axis movement.
        /// </summary>
        /// <param name="emmPerStep"></param>
        /// <param name="xmmPerStep"></param>
        /// <param name="ymmPerStep"></param>
        /// <param name="zmmPerStep"></param>
        /// <param name="eDistancePerMmMovement"></param>
        /// <param name="xDistance"></param>
        /// <param name="yDistance"></param>
        /// <param name="zDistance"></param>
        /// <param name="eInvertDirection"></param>
        /// <param name="xInvertDirection"></param>
        /// <param name="yInvertDirection"></param>
        /// <param name="zInvertDirection"></param>
        /// <param name="eRemainder"></param>
        /// <param name="xRemainder"></param>
        /// <param name="yRemainder"></param>
        /// <param name="zRemainder"></param>
        /// <param name="eModiPrintCoord"></param>
        /// <returns></returns>
        public static List<ConvertedGCodeLine> WriteMotorizedContinuousPrint(
            double emmPerStep, double xmmPerStep, double ymmPerStep, double zmmPerStep,
            double eDistancePerMmMovement, double xDistance, double yDistance, double zDistance,
            bool eInvertDirection, bool xInvertDirection, bool yInvertDirection, bool zInvertDirection,
            ref double eRemainder, ref double xRemainder, ref double yRemainder, ref double zRemainder,
            CoordinateModel eModiPrintCoord)
        {
            //The return GCode.
            List<ConvertedGCodeLine> convertedGCodeLinesList = new List<ConvertedGCodeLine>();

            if (((eDistancePerMmMovement) != 0) || ((xDistance) != 0) || ((yDistance) != 0) || ((zDistance) != 0))
            {
                string convertedGCodeLine = "";
                convertedGCodeLine += SerialCommands.MotorPrintWithMovement;

                //Calculate the distance that the Printhead's motor will move.
                double movementDistance = Math.Sqrt(xDistance * xDistance + yDistance * yDistance + zDistance * zDistance);
                double eDistance = eDistancePerMmMovement * movementDistance;

                //Output commands that will be sent through the serial port.
                convertedGCodeLine += G00Calculator.WriteSteps('X', xDistance, xmmPerStep, xInvertDirection, ref xRemainder);
                convertedGCodeLine += G00Calculator.WriteSteps('Y', yDistance, ymmPerStep, yInvertDirection, ref yRemainder);
                convertedGCodeLine += G00Calculator.WriteSteps('Z', zDistance, zmmPerStep, zInvertDirection, ref zRemainder);
                convertedGCodeLine += G00Calculator.WriteSteps('E', eDistance, emmPerStep, eInvertDirection, ref eRemainder);

                //EModiPrintCoord can be passed into this method as a null object if this step is not required.
                if (eModiPrintCoord != null)
                {
                    eModiPrintCoord.SetCoord(eDistance, false);
                }

                convertedGCodeLinesList.Add(new ConvertedGCodeLine(convertedGCodeLine));
                return convertedGCodeLinesList;
            }
               
            return null;
        }

        /// <summary>
        /// Returns a string of converted g-code for Motorized Printhead printing with stop-and-go interpolated movement.
        /// </summary>
        /// <param name="emmPerStep"></param>
        /// <param name="xmmPerStep"></param>
        /// <param name="ymmPerStep"></param>
        /// <param name="zmmPerStep"></param>
        /// <param name="eDistance"></param>
        /// <param name="xPrevious"></param>
        /// <param name="xCurrent"></param>
        /// <param name="yPrevious"></param>
        /// <param name="yCurrent"></param>
        /// <param name="zPrevious"></param>
        /// <param name="zCurrent"></param>
        /// <param name="xInvertDirection"></param>
        /// <param name="yInvertDirection"></param>
        /// <param name="zInvertDirection"></param>
        /// <param name="eInvertDirection"></param>
        /// <param name="eRemainder"></param>
        /// <param name="xRemainder"></param>
        /// <param name="yRemainder"></param>
        /// <param name="zRemainder"></param>
        /// <param name="eModiPrintCoord"></param>
        /// <param name="dropletModel"></param>
        /// <returns></returns>
        public static List<ConvertedGCodeLine> WriteMotorizedDropletPrint(double emmPerStep, double xmmPerStep, double ymmPerStep, double zmmPerStep,
            double eDistance, double xPrevious, double xCurrent, double yPrevious, double yCurrent, double zPrevious, double zCurrent,
            bool xInvertDirection, bool yInvertDirection, bool zInvertDirection, bool eInvertDirection,
            ref double eRemainder, ref double xRemainder, ref double yRemainder, ref double zRemainder,
            CoordinateModel eModiPrintCoord, DropletModel dropletModel)
        {
            //The return GCode.
            List<ConvertedGCodeLine> convertedGCodeLinesList = new List<ConvertedGCodeLine>();

            //List of all relative positions that require a droplet printed.
            List<double[]> printDistancesList = dropletModel.PrintPosition(xCurrent - xPrevious, yCurrent - yPrevious, zCurrent - zPrevious);

            if (printDistancesList != null) //Returns null if print position is not yet reached.
            {
                for (int i = 0; i < printDistancesList.Count; i++)
                {
                    //First droplet.
                    if (i == 0)
                    {
                        List<ConvertedGCodeLine> appendList = WriteAxesMovement(xmmPerStep, ymmPerStep, zmmPerStep,
                            printDistancesList[i][0], printDistancesList[i][1], printDistancesList[i][2], 
                            xInvertDirection, yInvertDirection, zInvertDirection,
                            ref xRemainder, ref yRemainder, ref zRemainder);
                        if (appendList != null) { convertedGCodeLinesList.AddRange(appendList); }
                    }
                    else
                    {
                        List<ConvertedGCodeLine> appendList = WriteAxesMovement(xmmPerStep, ymmPerStep, zmmPerStep,
                            printDistancesList[i][0] - printDistancesList[i - 1][0], printDistancesList[i][1] - printDistancesList[i - 1][1], printDistancesList[i][2] - printDistancesList[i - 1][2],
                            xInvertDirection, yInvertDirection, zInvertDirection,
                            ref xRemainder, ref yRemainder, ref zRemainder);
                        if (appendList != null) { convertedGCodeLinesList.AddRange(appendList); }
                    }

                    //Droplets 2+.
                    if (dropletModel.GradientShape != GradientShape.None) //Gradient print.
                    {
                        double ePrint = dropletModel.GradientModel.PrintParameterAtDistance(eDistance,
                            xPrevious + printDistancesList[i][0], yPrevious + printDistancesList[i][1], zPrevious + printDistancesList[i][2]);
                        List<ConvertedGCodeLine> appendList = WriteMotorizedPrintWithoutMovement(ePrint, emmPerStep, eInvertDirection, ref eRemainder, null);
                        if (appendList != null) { convertedGCodeLinesList.AddRange(appendList); }
                    }
                    else //No gradient.
                    {
                        List<ConvertedGCodeLine> appendList = WriteMotorizedPrintWithoutMovement(eDistance, emmPerStep, eInvertDirection, ref eRemainder, null);
                        if (appendList != null) { convertedGCodeLinesList.AddRange(appendList); }
                    }
                }

                return convertedGCodeLinesList;
            }

            return null;
        }

        /// <summary>
        /// Returns a converted g-code line for a motorized print without movement.
        /// </summary>
        /// <param name="eDistance"></param>
        /// <param name="emmPerStep"></param>
        /// <param name="invertDirection"></param>
        /// <param name="eRemainder"></param>
        /// <returns></returns>
        public static List<ConvertedGCodeLine> WriteMotorizedPrintWithoutMovement(double eDistance, double emmPerStep, bool invertDirection, ref double eRemainder, CoordinateModel eModiPrintCoord)
        {
            //The return GCode.
            List<ConvertedGCodeLine> convertedGCodeLinesList = new List<ConvertedGCodeLine>();

            if (eDistance != 0)
            {
                string printLine = SerialCommands.MotorPrintWithoutMovement;
                printLine += G00Calculator.WriteSteps('E', eDistance, emmPerStep, invertDirection, ref eRemainder);

                //EModiPrintCoord can be passed into this method as a null object if this step is not required.
                if (eModiPrintCoord != null)
                {
                    eModiPrintCoord.SetCoord(eDistance, false);
                }

                convertedGCodeLinesList.Add(new ConvertedGCodeLine(printLine));
                return convertedGCodeLinesList;
            }

            return null;
        }

        /// <summary>
        /// Returns a string of converted g-code for Valve Printhead printing with continuous Axis movement.
        /// </summary>
        /// <param name="xmmPerStep"></param>
        /// <param name="ymmPerStep"></param>
        /// <param name="zmmPerStep"></param>
        /// <param name="xDistance"></param>
        /// <param name="yDistance"></param>
        /// <param name="zDistance"></param>
        /// <param name="xInvertDirection"></param>
        /// <param name="yInvertDirection"></param>
        /// <param name="zInvertDirection"></param>
        /// <param name="xRemainder"></param>
        /// <param name="yRemainder"></param>
        /// <param name="zRemainder"></param>
        /// <returns></returns>
        public static List<ConvertedGCodeLine> WriteValveContinuousPrint(double xmmPerStep, double ymmPerStep, double zmmPerStep,
            double xDistance, double yDistance, double zDistance,
            bool xInvertDirection, bool yInvertDirection, bool zInvertDirection,
            ref double xRemainder, ref double yRemainder, ref double zRemainder)
        {
            //The return GCode.
            List<ConvertedGCodeLine> convertedGCodeLinesList = new List<ConvertedGCodeLine>();

            if (((xDistance) != 0) || ((yDistance) != 0) || ((zDistance) != 0))
            {
                string convertedGCodeLine = SerialCommands.ValvePrintWithMovement;

                //Calculate the number of steps for each axis.
                //If steps == 0, then do not print a command for that axis.
                convertedGCodeLine += G00Calculator.WriteSteps('X', xDistance, xmmPerStep, xInvertDirection, ref xRemainder);
                convertedGCodeLine += G00Calculator.WriteSteps('Y', yDistance, ymmPerStep, yInvertDirection, ref yRemainder);
                convertedGCodeLine += G00Calculator.WriteSteps('Z', zDistance, zmmPerStep, zInvertDirection, ref zRemainder);

                //Catch the corner case where there is movement that is less than one step.
                if (convertedGCodeLine != SerialCommands.ValvePrintWithMovement)
                {
                    convertedGCodeLinesList.Add(new ConvertedGCodeLine(convertedGCodeLine));
                }
                return convertedGCodeLinesList;
            }

            return null;
        }

        /// <summary>
        /// Returns a string of converted g-code for Valve Printhead printing with stop-and-go interpolated movement.
        /// </summary>
        /// <param name="xmmPerStep"></param>
        /// <param name="ymmPerStep"></param>
        /// <param name="zmmPerStep"></param>
        /// <param name="valveOpenTime"></param>
        /// <param name="xPrevious"></param>
        /// <param name="xCurrent"></param>
        /// <param name="yPrevious"></param>
        /// <param name="yCurrent"></param>
        /// <param name="zPrevious"></param>
        /// <param name="zCurrent"></param>
        /// <param name="xInvertDirection"></param>
        /// <param name="yInvertDirection"></param>
        /// <param name="zInvertDirection"></param>
        /// <param name="xRemainder"></param>
        /// <param name="yRemainder"></param>
        /// <param name="zRemainder"></param>
        /// <param name="dropletModel"></param>
        /// <returns></returns>
        public static List<ConvertedGCodeLine> WriteValveDropletPrint(
            double xmmPerStep, double ymmPerStep, double zmmPerStep,
            int valveOpenTime, double xPrevious, double xCurrent, double yPrevious, double yCurrent, double zPrevious, double zCurrent,
            bool xInvertDirection, bool yInvertDirection, bool zInvertDirection,
            ref double xRemainder, ref double yRemainder, ref double zRemainder,
            DropletModel dropletModel)
        {
            //The return GCode.
            List<ConvertedGCodeLine> convertedGCodeLinesList = new List<ConvertedGCodeLine>();

            //List of all positions that require a droplet printed.
            List<double[]> printDistancesList = dropletModel.PrintPosition(xCurrent - xPrevious, yCurrent - yPrevious, zCurrent - zPrevious);

            if (printDistancesList != null) //Returns null if print position is not yet reached.
            {
                for (int i = 0; i < printDistancesList.Count; i++)
                {
                    //First droplet.
                    if (i == 0)
                    {
                        List<ConvertedGCodeLine> appendList = WriteAxesMovement(xmmPerStep, ymmPerStep, zmmPerStep,
                            printDistancesList[i][0], printDistancesList[i][1], printDistancesList[i][2],
                            xInvertDirection, yInvertDirection, zInvertDirection,
                            ref xRemainder, ref yRemainder, ref zRemainder);
                        if (appendList != null) { convertedGCodeLinesList.AddRange(appendList); }
                        //To Do: Handle error if appendList == null
                    }
                    else
                    {
                        List<ConvertedGCodeLine> appendList = WriteAxesMovement(xmmPerStep, ymmPerStep, zmmPerStep,
                            printDistancesList[i][0] - printDistancesList[i - 1][0], printDistancesList[i][1] - printDistancesList[i - 1][1], printDistancesList[i][2] - printDistancesList[i - 1][2],
                            xInvertDirection, yInvertDirection, zInvertDirection,
                            ref xRemainder, ref yRemainder, ref zRemainder);
                        if (appendList != null) { convertedGCodeLinesList.AddRange(appendList); }
                    }

                    //Droplets 2+.
                    if (dropletModel.GradientShape != GradientShape.None) //Gradient print.
                    {
                        int valvePrintTime = (int)dropletModel.GradientModel.PrintParameterAtDistance(valveOpenTime,
                            xPrevious + printDistancesList[i][0], yPrevious + printDistancesList[i][1], zPrevious + printDistancesList[i][2]);
                        List<ConvertedGCodeLine> appendList = WriteValvePrintWithoutMovement(valvePrintTime);
                        if (appendList != null) { convertedGCodeLinesList.AddRange(appendList); }
                    }
                    else //No gradient.
                    {
                        List<ConvertedGCodeLine> appendList = WriteValvePrintWithoutMovement(valveOpenTime);
                        if (appendList != null) { convertedGCodeLinesList.AddRange(appendList); }
                    }
                }

                return convertedGCodeLinesList;
            }

            return null;
        }

        /// <summary>
        /// Returns a converted g-code line for a valve print without movement command.
        /// </summary>
        /// <returns></returns>
        public static List<ConvertedGCodeLine> WriteValvePrintWithoutMovement(int valveOpenTime)
        {
            //The return GCode.
            List<ConvertedGCodeLine> convertedGCodeLinesList = new List<ConvertedGCodeLine>();

            string printLine = SerialCommands.ValvePrintWithoutMovement;
            printLine += " O";
            if (valveOpenTime != 0)
            { printLine += valveOpenTime; }

            convertedGCodeLinesList.Add(new ConvertedGCodeLine(printLine));
            return convertedGCodeLinesList;
        }

        /// <summary>
        /// Returns a converted g-code line for a valve close command.
        /// </summary>
        /// <returns></returns>
        public static List<ConvertedGCodeLine> WriteValveClose()
        {
            //The return GCode.
            List<ConvertedGCodeLine> convertedGCodeLinesList = new List<ConvertedGCodeLine>();

            string printLine = SerialCommands.ValvePrintWithoutMovement;
            printLine += " C";

            convertedGCodeLinesList.Add(new ConvertedGCodeLine(printLine));
            return convertedGCodeLinesList;
        }
        #endregion
    }
}
