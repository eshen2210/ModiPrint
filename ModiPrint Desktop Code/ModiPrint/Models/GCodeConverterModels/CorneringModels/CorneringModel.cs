using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using ModiPrint.DataTypes.GlobalValues;
using ModiPrint.Models.PrinterModels;
using ModiPrint.Models.PrinterModels.AxisModels;
using ModiPrint.Models.PrinterModels.PrintheadModels;
using ModiPrint.Models.PrinterModels.PrintheadModels.PrintheadTypeModels;
using ModiPrint.Models.PrintModels;
using ModiPrint.Models.PrintModels.MaterialModels;
using ModiPrint.Models.GCodeConverterModels.ProcessModels.ProcessG00Models;

namespace ModiPrint.Models.GCodeConverterModels.CorneringModels
{
    public class CorneringModel
    {
        #region Fields and Properties
        //Contains information on the print parameters.
        PrinterModel _printerModel;
        PrintModel _printModel;

        //Contains pseudo-global parameters for the GCodeConverter that are passed around various GCodeConverter classes.
        private ParametersModel _parametersModel;

        //Numerator value for the progress bar.
        //Should tick up every time a continuous movement has a set of parameters calculated.
        private int _progressBarTick = 0;
        #endregion

        #region Contructor
        public CorneringModel(PrinterModel PrinterModel, PrintModel PrintModel, ParametersModel ParametersModel)
        {
            _printerModel = PrinterModel;
            _printModel = PrintModel;

            _parametersModel = ParametersModel;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Add the deceleration steps parameter to all G commands.
        /// Deceleration steps are calculated such that steppers move as quickly as possible without violating max speeds and junction speeds.
        /// </summary>
        /// <param name="convertedGCodeLinesList"></param>
        public void AddDecelerationStepsParameter(ref List<ConvertedGCodeLine> convertedGCodeLinesList)
        {
            //Reset the progress bar.
            _progressBarTick = 0;
            //A list of all the indeces where a new Material is set.
            List<int> materialIndecesList = new List<int>();
            //The reference of the corresponding Material of the same index in the MaterialIndecesList.
            List<MaterialModel> materialModelsList = new List<MaterialModel>();

            //Starting from the beginning of the list...
            //Search for the settings of a new Material.
            for (int i = 0; i < convertedGCodeLinesList.Count; i ++)
            {
                //Found a new Material.
                if ((convertedGCodeLinesList[i].GCode.Length >= 15)
                  && (convertedGCodeLinesList[i].GCode.Substring(1, 14) == "SwitchMaterial"))
                {
                    materialIndecesList.Add(i);
                    int materialNameStartIndex = convertedGCodeLinesList[i].GCode.IndexOf('"') + 1;
                    string materialName = convertedGCodeLinesList[i].GCode.Substring(materialNameStartIndex, convertedGCodeLinesList[i].GCode.Length - (materialNameStartIndex + 1));
                    materialModelsList.Add(_printModel.FindMaterialByName(materialName));
                }
            }
            
            //For each Material...
            //Calculate the number of steps of each movement until deceleration such that smooth cornerning is possible.
            for (int m = 0; m < materialIndecesList.Count; m++)
            {
                int materialStartIndex = materialIndecesList[m];
                int materialEndIndex = ((m + 1) >= materialIndecesList.Count) ? (convertedGCodeLinesList.Count - 1) : materialIndecesList[m + 1];

                List<List<MovementModel>> continuousMovementsList = FindContinuousMovements(convertedGCodeLinesList, materialModelsList[m], materialStartIndex, materialEndIndex);
                CalculateJunctionSpeeds(continuousMovementsList, materialModelsList[m]);
                CalculateVelocityProfile(continuousMovementsList, materialModelsList[m]);
                WriteExitSpeed(continuousMovementsList, ref convertedGCodeLinesList, materialModelsList[m]);
            }
        }

        /// <summary>
        /// Find all of the coordinates of a continuous movement in relative positions.
        /// A continuous movement is defined any series of individual movements where each movement can be executed consecutively without coming to a complete stop.
        /// Continuous movements enable the XYZ stage to transition between each movement without coming to a complete stop.
        /// </summary>
        /// <param name="convertedGCodeLineList"></param>
        /// <param name="materialStartIndex"></param>
        /// <param name="materialEndIndex"></param>
        /// <returns></returns>
        private List<List<MovementModel>> FindContinuousMovements(List<ConvertedGCodeLine> convertedGCodeLineList, MaterialModel materialModel, int materialStartIndex, int materialEndIndex)
        {
            //The outer list contains sets of individual movements. Each set represents a continuous movement.
            //The inner list contains a single movement.
            //The [0][0] would contain the coordinates for the first movement starting from the initial position of the Material.
            List<List<MovementModel>> continuousMovementsList = new List<List<MovementModel>>();

            //If true, then make a new inner list.
            bool startNewContinuousMovement = true;

            //Iterate through the entire Material's GCode.
            //Note the coordinates of every set of continuous movements.
            for (int i = materialStartIndex; i <= materialEndIndex; i++)
            {
                //Find the next coordinate.
                //If it is a part of a continuous movement, then add it to the list.
                if ((convertedGCodeLineList[i].GCode.Length > 2) 
                 && ((convertedGCodeLineList[i].GCode.Substring(0, 3) == "G00")
                  || (convertedGCodeLineList[i].GCode.Substring(0, 3) == "G01")
                  || (convertedGCodeLineList[i].GCode.Substring(0, 3) == "G02"))) //Is a movement or print command.
                {
                    //Read the coordinates. 
                    MovementModel newMovementModel = ReadCoord(convertedGCodeLineList[i].GCode, i, materialModel);

                    //Make a new inner list if applicable.
                    if (startNewContinuousMovement == true)
                    {
                        continuousMovementsList.Add(new List<MovementModel>());
                        startNewContinuousMovement = false;
                    }

                    //Add continuous movement.
                    continuousMovementsList[continuousMovementsList.Count - 1].Add(newMovementModel);

                }
                else
                {
                    //End of the movement.
                    startNewContinuousMovement = true;
                }
            }

            return continuousMovementsList;
        }

        /// <summary>
        /// Calculate the junction speeds of movements.
        /// Junction speeds are maximized but do not exceed specified parameters.
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name=""></param>
        /// <param name="materialModel"></param>
        private void CalculateJunctionSpeeds(List<List<MovementModel>> continuousMovementsList, MaterialModel materialModel)
        {
            //The junction speed is limited by how quickly all of the steppers can decelerate to zero by the very end of the continuous movement assuming they travelled at max speed until the last possible moment.
            //The junction speed is also limited by the centripedial acceleration of the XYZ stage during cornering.
            //The junction speed is also limited by the adjacent movement's entry and exit speeds.
            //In other words, the more continous movement length remaining, the higher the junction speed. 
            //The faster the stepper's acceleration/deceleration, the higher the junction speed.
            //The higher the junction deviation, the higher the junction speed.
            //Junction speed is in units of steps / s.

            //For each continuous movement...
            for (int i = 0; i < continuousMovementsList.Count; i++)
            {
                //Calculate the junction speed such that:
                //1. Centripedal acceleration is not exceeded.
                //2. Enough time is given for the XYZ stage to decelerate to the exit junction speed.
                //Exit speeds for the last movement of each continuous movement is zero as all Axes come to a halt. 
                //3. Maximum speed of the movement is not exceeded.
                //Start from the movement that will be executed last and proceed towards the first movement.
                continuousMovementsList[i][continuousMovementsList[i].Count - 1].ExitSpeed = 0; //Last movement's exit speed is always zero.
                for (int j = continuousMovementsList[i].Count - 1; j >= 0; j--)
                {
                    MovementModel currentMovement = continuousMovementsList[i][j];

                    if (j > 0)
                    {
                        MovementModel previousMovement = continuousMovementsList[i][j - 1];

                        //1. Calculate junction speed based on centipedal acceleration.
                        double junctionAcceleration = Math.Min(currentMovement.Acceleration, previousMovement.Acceleration);
                        double junctionAngle = CalculateJunctionAngle(currentMovement, previousMovement); //In radians.
                        double centripedalAccelerationSpeed = CalculateJunctionSpeed(junctionAcceleration, materialModel.JunctionDeviation, junctionAngle);
                        previousMovement.ExitSpeed = (Double.IsNaN(centripedalAccelerationSpeed)) ? double.MaxValue : centripedalAccelerationSpeed;

                        //2. Calculate junction speed based on distance to exit.
                        //Minimal entry speed required to finish deceleration by movement's end.
                        double entrySpeedToDecelerate = Math.Sqrt(Math.Pow(currentMovement.ExitSpeed, 2) + 2 * currentMovement.TotalDistance * currentMovement.Acceleration); 
                        previousMovement.ExitSpeed = Math.Min(entrySpeedToDecelerate, previousMovement.ExitSpeed);

                        //3. Calculate junction speed based on max speed of adjacent movements.
                        //Ensure neither movement's max speed is exceeded.
                        double junctionMaxSpeed = Math.Min(previousMovement.MaxSpeed, currentMovement.MaxSpeed);
                        previousMovement.ExitSpeed = Math.Min(junctionMaxSpeed, previousMovement.ExitSpeed);
                    }
                    else
                    {
                        //First movement in this continuous movement.
                        //No previous movement. Nothing to calculate.
                    }
                }

                //Calculate the junction speed such that:
                //1. Enough time is given for the XYZ stage to accelerate to the junction speed.
                //Entry speed for the first movement for each continuous movement is zero.
                //Start from the first movement that will be execute and proceed towards the last.
                //The first movement's entry speed is always zero.
                for (int j = 0; j < continuousMovementsList[i].Count - 1; j++)
                {
                    MovementModel currentMovement = continuousMovementsList[i][j];

                    //1. Calculate junction speed based on distance from entry.
                    if (j > 0)
                    {
                        MovementModel previousMovement = continuousMovementsList[i][j - 1];

                        double exitSpeedFromAcceleration = Math.Sqrt(Math.Pow(previousMovement.ExitSpeed, 2) + 2 * currentMovement.TotalDistance * currentMovement.Acceleration);
                        currentMovement.ExitSpeed = Math.Min(exitSpeedFromAcceleration, currentMovement.ExitSpeed);
                    }
                    else
                    {
                        //First movement in this continuous movement.
                        double exitSpeedFromAcceleration = Math.Sqrt(2 * currentMovement.TotalDistance * currentMovement.Acceleration);
                        currentMovement.ExitSpeed = Math.Min(exitSpeedFromAcceleration, currentMovement.ExitSpeed);
                    }
                }

                TickProgressBar(continuousMovementsList);
            }
        }

        /// <summary>
        /// Calculate the velocity profiles and the distances required to reach acceleration and deceleration of each movement.
        /// </summary>
        /// <param name="continuousMovementsList"></param>
        /// <param name="convertedGCodeLineList"></param>
        /// <param name="materialModel"></param>
        /// <returns></returns>
        private void CalculateVelocityProfile(List<List<MovementModel>> continuousMovementsList, MaterialModel materialModel)
        {
            foreach (List<MovementModel> continuousMovement in continuousMovementsList)
            {
                for (int j = 0; j < continuousMovement.Count; j++)
                {
                    double entrySpeed = (j > 0) ? continuousMovement[j - 1].ExitSpeed : 0;

                    //All Axes can have two different profiles:
                    //1. Max speed is attainable and runs through acceleration, cruise at max speed, then deceleration.
                    //2. Max speed is never reached because the number of steps in the movement is too small and deceleration occurs immediately after acceleration.
                    //3. Continuous acceleration/deceleration/no velocity change throughout the entirety of the movement.

                    //Distance to go from entry speed to the maximum allowable exit speed assuming pure acceleration/deceleration throughout the entire movement.
                    double presumedEntryToExitDistance = Math.Abs((continuousMovement[j].ExitSpeed - entrySpeed) / continuousMovement[j].Acceleration) * (entrySpeed + 0.5 * (continuousMovement[j].ExitSpeed - entrySpeed));
                    //Distance required for the movement to attain max speed from entry speed.
                    double presumedEntryToCruiseDistance = ((continuousMovement[j].MaxSpeed - entrySpeed) / continuousMovement[j].Acceleration) * (entrySpeed + 0.5 * (continuousMovement[j].MaxSpeed - entrySpeed));
                    //Distance to decelerate from max speed to exit speed.
                    double presumedCruiseToExitDistance = (continuousMovement[j].MaxSpeed != continuousMovement[j].ExitSpeed)
                        ? ((continuousMovement[j].MaxSpeed - continuousMovement[j].ExitSpeed) / continuousMovement[j].Acceleration) * (continuousMovement[j].ExitSpeed + 0.5 * (continuousMovement[j].MaxSpeed - continuousMovement[j].ExitSpeed))
                        : 0;

                    //Account for the rounding errors of floating point variables.
                    presumedEntryToExitDistance = Math.Round(presumedEntryToExitDistance, 8);
                    presumedEntryToCruiseDistance = Math.Round(presumedEntryToCruiseDistance, 8);
                    presumedCruiseToExitDistance = Math.Round(presumedCruiseToExitDistance, 8);
                    double totalDistanceRounded = Math.Round(continuousMovement[j].TotalDistance, 8);

                    //Is there enough distance to accelerate to max speed and form a trapezoid velocity profile?
                    if ((presumedEntryToCruiseDistance + presumedCruiseToExitDistance) < totalDistanceRounded)
                    {
                        //Scenario 1.

                        continuousMovement[j].VelocityProfileType = VelocityProfileType.Trapezoid;
                        continuousMovement[j].DistanceToCruise = presumedEntryToCruiseDistance;
                        continuousMovement[j].DistanceToDeceleration = continuousMovement[j].TotalDistance - presumedCruiseToExitDistance;
                    }
                    //Is there enough distance to accelerate then decelerate and form a triangle velocity profile?
                    else if (presumedEntryToExitDistance < totalDistanceRounded)
                    {
                        //Scenario 2.

                        continuousMovement[j].VelocityProfileType = VelocityProfileType.Triangle;
                        //Movement can be divided into two parts for this profile.
                        //One half of the movement involves accelerating to an arbitrary speed, then decelerating back to entry speed. Acceleration == -1 * deceleration so acceleration and deceleration share exactly half of this section's distance.
                        //The other half of the movement involves travelling from the entry speed to the exit speed across presumedEntryToExitDistnce.
                        //Whichever part occurs first depends on whether or not the exit speed is greater than the movement speed.
                        if (entrySpeed >= continuousMovement[j].ExitSpeed)
                        {
                            //Acceleration and deceleration occurs before deceleration to exit speed.
                            continuousMovement[j].DistanceToCruise = continuousMovement[j].DistanceToDeceleration = (continuousMovement[j].TotalDistance - presumedEntryToExitDistance) / 2;
                        }
                        else
                        {
                            //Acceleration to exit speed occurs before further acceleration and deceleration.
                            continuousMovement[j].DistanceToCruise = continuousMovement[j].DistanceToDeceleration = presumedEntryToExitDistance + (continuousMovement[j].TotalDistance - presumedEntryToExitDistance) / 2;
                        }
                    }
                    //Is there enough distance for linear acceleration/deceleration from entry to exit?
                    //Should not be no. This minimal junction speed should have been caught earlier.
                    else if (presumedEntryToExitDistance == totalDistanceRounded)
                    {
                        //Scenario 3.

                        continuousMovement[j].VelocityProfileType = VelocityProfileType.Linear;
                        continuousMovement[j].DistanceToCruise = continuousMovement[j].DistanceToDeceleration = continuousMovement[j].TotalDistance;
                    }
                    else
                    {
                        //Should not happen.

                        continuousMovement[j].VelocityProfileType = VelocityProfileType.Unset;
                        _parametersModel.ErrorReporterViewModel.ReportError("G-Code Conversion Failed: Junction Calculations Error, Should Not Happen, Please Report This Error To The Developer", "ContinuousMovementIndex: " + continuousMovement[j].GCodeIndex + " ");
                    }
                }

                TickProgressBar(continuousMovementsList);
            }
        }

        /// <summary>
        /// Reads the coordinate values in a movement or print command.
        /// </summary>
        /// <param name="convertedGCodeLine"></param>
        /// <param name="gCodeIndex"></param>
        /// <param name="materialModel"></param>
        /// <returns></returns>
        private MovementModel ReadCoord(string convertedGCodeLine, int gCodeIndex, MaterialModel materialModel)
        {
            //G Commands from Converted GCode are in relative positions.
            string[] gCodePhrases = GCodeStringParsing.GCodeTo2DArr(convertedGCodeLine)[0];

            double xDistance = 0;
            double yDistance = 0;
            double zDistance = 0;
            double eDistance = 0;

            //Read the coordinates from the GCode.
            for (int phrase = 1; phrase < gCodePhrases.Length; phrase++)
            {
                switch (gCodePhrases[phrase][0])
                {
                    case 'X':
                        int xSteps = (int)GCodeStringParsing.ParseDouble(gCodePhrases[phrase]);
                        AxisModel xAxisModel = _printerModel.AxisModelList[0];
                        xDistance = G00Calculator.StepsToDistance(xSteps, xAxisModel.MmPerStep, xAxisModel.IsDirectionInverted);
                        break;
                    case 'Y':
                        int ySteps = (int)GCodeStringParsing.ParseDouble(gCodePhrases[phrase]);
                        AxisModel yAxisModel = _printerModel.AxisModelList[1];
                        yDistance = G00Calculator.StepsToDistance(ySteps, yAxisModel.MmPerStep, yAxisModel.IsDirectionInverted);
                        break;
                    case 'Z':
                        int zSteps = (int)GCodeStringParsing.ParseDouble(gCodePhrases[phrase]);
                        AxisModel zAxisModel = _printerModel.FindAxis(materialModel.PrintheadModel.AttachedZAxisModel.Name);
                        zDistance = G00Calculator.StepsToDistance(zSteps, zAxisModel.MmPerStep, zAxisModel.IsDirectionInverted);
                        break;
                    case 'E':
                        int eSteps = (int)GCodeStringParsing.ParseDouble(gCodePhrases[phrase]);
                        MotorizedPrintheadTypeModel motorizedPrintheadTypeModel = (MotorizedPrintheadTypeModel)materialModel.PrintheadModel.PrintheadTypeModel;
                        eDistance = G00Calculator.StepsToDistance(eSteps, motorizedPrintheadTypeModel.MmPerStep, motorizedPrintheadTypeModel.IsDirectionInverted);
                        break;
                    default:
                        //Do nothing.
                        break;
                }
            }

            MovementModel movementModel = new MovementModel(xDistance, yDistance, zDistance, eDistance, gCodeIndex, materialModel, _printerModel);
            return movementModel;
        }

        /// <summary>
        /// Calculate a junction speed based on junction angle and junction deviation.
        /// Junction speed increases with wider angles and larger junction deviations.
        /// https://onehossshay.wordpress.com/2011/09/24/improving_grbl_cornering_algorithm/
        /// </summary>
        /// <param name="acceleration"></param>
        /// <param name="junctionDeviation"></param>
        /// <param name="angleRadians"></param>
        /// <returns></returns>
        private double CalculateJunctionSpeed(double acceleration, double junctionDeviation, double angleRadians)
        {
            //Radius is in whatever length unit given in the acceleration parameter.
            //Angle in degrees.

            double radius = junctionDeviation * Math.Sin(angleRadians / 2) / (1 - Math.Sin(angleRadians / 2));
            return Math.Sqrt(Math.Abs(acceleration * radius));
        }

        /// <summary>
        /// Find the angle between the line created between two movements
        /// Return value is in units of degrees.
        /// </summary>
        /// <param name="newCoord"></param>
        /// <param name="coordList"></param>
        /// <returns></returns>
        private double CalculateJunctionAngle(MovementModel movement1, MovementModel movement2)
        {
            //The E Axis is not taken into account here because I don't want to implement a Vector4D.
            //Why an E angle does not need to be calculated is because the E Axis' distance moved should be always proportional to the distance moved by XYZ.
                
            //Find the angle between the line created by the new movement and the previous movement.
            Vector3D line1 = new Vector3D(movement1.X *-1, movement1.Y *-1, movement1.Z * -1);
            Vector3D line2 = new Vector3D(movement2.X, movement2.Y, movement2.Z);

            double dotProduct = Vector3D.DotProduct(line1, line2);
            double angleRadians = Math.Acos(dotProduct / (line1.Length * line2.Length));

            if (angleRadians != double.NaN) //Should not fail this check.
            {
                return angleRadians;
            }
            else
            {
                return double.NaN;
            }
        }

        /// <summary>
        /// Returns true if printing or not printing across both movements.
        /// </summary>
        /// <param name="newCoord"></param>
        /// <param name="continuousMovement"></param>
        /// <returns></returns>
        private bool IsPrintingConstant(MovementModel newMovementModel, List<MovementModel> continuousMovement)
        {
            //If this returns true, then continuous movement is possible.

            MovementModel previousMovementModel = continuousMovement[continuousMovement.Count - 1];

            if (((previousMovementModel.E == 0) && (newMovementModel.E != 0))
                || (previousMovementModel.E != 0) && (newMovementModel.E == 0))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Adds the "T" (exit speed) parameter to the converted GCode.
        /// </summary>
        /// <param name="continuousMovementsList"></param>
        /// <param name="convertedGCodeLinesList"></param>
        /// <param name="printerModel"></param>
        /// <param name="materialModel"></param>
        private void WriteExitSpeed(List<List<MovementModel>> continuousMovementsList, ref List<ConvertedGCodeLine> convertedGCodeLinesList, MaterialModel materialModel)
        {
            foreach (List<MovementModel> movementsList in continuousMovementsList)
            {
                foreach (MovementModel movement in movementsList)
                {
                    //Append the 'T' exit speed parameter to this movement's converted GCode.
                    if (movement.ExitSpeed != 0)
                    {
                        int intExitSpeed = Convert.ToInt32(Math.Floor(movement.ExitSpeed));
                        string exitSpeedParameter = " T" + intExitSpeed.ToString();
                        convertedGCodeLinesList[movement.GCodeIndex].GCode += (exitSpeedParameter.ToString());
                    }
                }

                TickProgressBar(continuousMovementsList);
            }
        }

        /// <summary>
        /// Increases the progress bar and reports it to the GUI.
        /// </summary>
        private void TickProgressBar(List<List<MovementModel>> continuousMovementsList)
        {
            //The numerator is the number of time a continuous movement was processed.
            //Processed can mean any function calculating some parameter for the continuous movement.
            //Since there are three functions going through the entire list, the denominator is total continuous movements times 3.
            _progressBarTick++;
            int percentCompleted = (_progressBarTick * 100) / (continuousMovementsList.Count * 3);
            _parametersModel.ReportProgress("Calculating Cornering Speeds " + percentCompleted + "%");
        }
        #endregion
    }
}
