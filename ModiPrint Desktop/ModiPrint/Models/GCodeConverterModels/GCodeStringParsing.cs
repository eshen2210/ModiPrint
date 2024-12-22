using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModiPrint.Models.GCodeConverterModels
{
    /// <summary>
    /// Contains various methods for parsing GCode.
    /// </summary>
    public static class GCodeStringParsing
    {
        /// <summary>
        /// Takes a GCode parameter and returns the double value in it.
        /// Supports negative/positive, integer/decimal numbers in the format of "X#" where X is any letter and # is the double.
        /// </summary>
        public static double ParseDouble(string phrase)
        {
            return Convert.ToDouble(phrase.Substring(1));
        }

        /// <summary>
        /// Converts a string (of GCode) into a 2D array.
        /// </summary>
        /// <remarks>
        /// The 2D array facilitates the sorting and parsing of GCode.
        /// The first dimension is the string delimited by line breaks.
        /// The second dimention is a first dimension element delimited by whitespaces.
        /// </remarks>
        public static string[][] GCodeTo2DArr(string gCodeStr)
        {
            if (!String.IsNullOrWhiteSpace(gCodeStr))
            {
                //First dimenstion.
                //Delimits string by line breaks.
                string[] gCode1DArr = gCodeStr.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

                //Second dimenstion.
                //Delimits each array of the first dimension by whitespaces.
                string[][] gCode2DArr = new string[gCode1DArr.Length][];
                for (int line = 0; line < gCode1DArr.Length; line++)
                { gCode2DArr[line] = gCode1DArr[line].Split(new string[] { }, StringSplitOptions.None); }

                return gCode2DArr;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Stitches back together the second dimension of a GCode 2D array that was originally delimited by whitespace.
        /// </summary>
        public static string GCodeLineArrToStr(string[] gCodeLineFragment)
        {
            string returnString = "";

            for (int i = 0; i < gCodeLineFragment.Length - 1; i++)
            { returnString += gCodeLineFragment[i] + " "; }
            returnString += gCodeLineFragment[gCodeLineFragment.Length - 1]; //So that the last character is not a white space.

            return returnString;
        }

        /// <summary>
        /// Remove ';' and all characters following.
        /// </summary>
        /// <param name="gCodeLine"></param>
        public static string[] RemoveGCodeComments(string[] gCodeLine)
        {
            List<string> phrasesList = new List<string>();

            //Count the number of phrases that are not comments.
            for (int i = 0; i < gCodeLine.Length; i++)
            {
                if (gCodeLine[i].Contains(';'))
                {
                    gCodeLine[i] = gCodeLine[i].Substring(0, gCodeLine[i].IndexOf(';'));
                    if (!String.IsNullOrWhiteSpace(gCodeLine[i]))
                    {
                        phrasesList.Add(gCodeLine[i]);
                    }
                    break;
                }

                if (!String.IsNullOrWhiteSpace(gCodeLine[i]))
                {
                    phrasesList.Add(gCodeLine[i]);
                }
            }

            //Instantiate, populate, and return a GCode line with no comments.
            string[] uncommentedGCodeLine = new string[phrasesList.Count];
            for (int i = 0; i < uncommentedGCodeLine.Length; i++)
            {
                uncommentedGCodeLine[i] = gCodeLine[i];
            }

            return uncommentedGCodeLine;
        }

        /// <summary>
        /// Renive ';' and all characters following.
        /// </summary>
        /// <param name="gCodeLine"></param>
        /// <returns></returns>
        public static string RemoveGCodeComments(string gCodeLine)
        {
            if (gCodeLine.Contains(';'))
            {
                return gCodeLine.Substring(0, gCodeLine.IndexOf(';'));
            }
            else
            {
                //No comment found.
                return gCodeLine;
            }
        }
    }
}
