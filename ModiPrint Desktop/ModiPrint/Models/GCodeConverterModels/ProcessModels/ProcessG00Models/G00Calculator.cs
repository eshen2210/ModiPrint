using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace ModiPrint.Models.GCodeConverterModels.ProcessModels.ProcessG00Models
{
    /// <summary>
    /// Contains methods that perform calculations for G00 command conversion.
    /// </summary>
    public static class G00Calculator
    {
        #region Methods
        /// <summary>
        /// Outputs a GCode segment containing information about an Axis and its relative movement (e.g. X#).
        /// </summary>
        /// <param name="axisName"></param>
        /// <param name="distance"></param>
        /// <param name="mmPerStep"></param>
        /// <param name="invertDirection"></param>
        /// <param name="remainder"></param>
        /// <returns></returns>
        public static string WriteSteps(char axisName, double distance, double mmPerStep, bool invertDirection, ref double remainder)
        {
            int steps = 0;
            if (mmPerStep != 0)
            {
                distance += remainder;
                steps = (int)Math.Floor((distance) / mmPerStep);
                remainder = distance - steps * mmPerStep;
                steps = (invertDirection == false) ? steps : (-1 * steps);
            }
            
            if (steps != 0)
            {
                return " " + axisName.ToString() + steps.ToString();
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Calculates distance (in mm) based on steps and Axis parameters.
        /// </summary>
        /// <param name="steps"></param>
        /// <param name="mmPerStep"></param>
        /// <param name="invertDirection"></param>
        /// <returns></returns>
        public static double StepsToDistance(int steps, double mmPerStep, bool invertDirection)
        {
            return (invertDirection == false) ? (steps * mmPerStep) : (-1 * steps * mmPerStep);
        }

        /// <summary>
        /// Calculates the magnitude based on 3 directions of a vector.
        /// </summary>
        /// <param name="xDistance"></param>
        /// <param name="yDistance"></param>
        /// <param name="zDistance"></param>
        /// <returns></returns>
        public static double CalculateDistance(double xDistance, double yDistance, double zDistance)
        {
            return Math.Sqrt(Math.Pow((xDistance), 2) + Math.Pow((yDistance), 2) + Math.Pow((zDistance), 2));
        }

        /// <summary>
        /// Calculates the magnitude based on the 4 directions of a vector.
        /// </summary>
        /// <param name="eDistance"></param>
        /// <param name="xDistance"></param>
        /// <param name="yDistance"></param>
        /// <param name="zDistance"></param>
        /// <returns></returns>
        public static double CalculateDistance(double eDistance, double xDistance, double yDistance, double zDistance)
        {
            return Math.Sqrt(Math.Pow((eDistance), 2) + Math.Pow((xDistance), 2) + Math.Pow((yDistance), 2) + Math.Pow((zDistance), 2));
        }
        #endregion
    }
}
