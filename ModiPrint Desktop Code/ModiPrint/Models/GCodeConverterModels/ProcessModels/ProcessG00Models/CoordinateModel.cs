using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModiPrint.DataTypes.GlobalValues;


namespace ModiPrint.Models.GCodeConverterModels.ProcessModels.ProcessG00Models
{
    //Associated with the CoordinateModel class (GCodeConverter).
    //States the function of the Coordinate and also serves as its GCode identifier.
    public enum CoordinateType
    {
        Unset, //This should never be the case.
        X, //This Coordinate tracks the position of the X Axis.
        Y, //This Coordinate tracks the position of the Y Axis.
        Z, //This Coordinate tracks the position of the Z Axes.
        E, //This Coordinate tracks the position of Motorized Printheads.
        S //This Coordinate tracks the position of RepRap's extrusion position.
    }

    /// <summary>
    /// Contains information about coordinate positions of an axis.
    /// </summary>
    public class CoordinateModel
    {
        #region Fields
        //Class containing pseudo-global parameters for the GCodeConverter that are passed around various GCodeConverter classes.
        private ParametersModel _parametersModel;

        //GCode identifier.
        private CoordinateType _type;
        public CoordinateType Type
        {
            get { return _type; }
        }

        //Coordinates for tracking X, Y, or Z Axes (movement) are the only positional changes that should be converted to GCode (_writable == true);
        //Changes in Coordinates for tracking E Axes (printing) should not be converted to GCode (_writable == false);
        private bool _writable;

        //Previous position of this printhead.
        private double _previousCoord = 0;
        public double PreviousCoord
        {
            get { return _previousCoord; }
        }

        //Current position of this printhead.
        private double _currentCoord = 0;
        public double CurrentCoord
        {
            get { return _currentCoord; }
        }

        //Change in position since the last update.
        public double DeltaCoord
        {
            get { return (_currentCoord - _previousCoord); }
        }

        //Since a movement may not fit perfectly into a step, store the remainder of the delta.
        //Should be retrieved and calculated outside this class during movement calculations.
        //Set as a public variable without get/set because this needs to be pass as a reference.
        public double DeltaCoordRemainder = 0;

        //Coord values should not be below this MinPosition.
        private double _minPosition;
        public double MinPosition
        {
            get { return _minPosition; }
            set { _minPosition = value; }
        }

        //Coord values should not exceed this MaxPosition.
        private double _maxPosition;
        public double MaxPosition
        {
            get { return _maxPosition; }
            set { _maxPosition = value; }
        }

        //Did the coordinates change since the last GCode line?
        private bool _changed = false;
        public bool Changed
        {
            get { return _changed; }
        }

        //If the coordinates did change, then was the change positive or negative?
        //Only relevant to E Coordinates.
        private bool _positiveChanged = false;
        public bool PositiveChanged
        {
            get { return _positiveChanged; }
        }
        #endregion

        #region Constructor
        public CoordinateModel(CoordinateType Type, bool Writable, ParametersModel ParametersModel, double Position, double MinPosition, double MaxPosition)
        {
            _type = Type;
            _writable = Writable;
            _parametersModel = ParametersModel;

            _previousCoord = _currentCoord = Position;
            _minPosition = MinPosition;
            _maxPosition = MaxPosition;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets current, previous, and changed coordinate information.
        /// </summary>
        /// <param name="currentCoord"></param>
        /// <param name="absCoord">Absolute coordinate or relative coordinate system?</param>
        public void SetCoord(double currentCoordInput, bool absCoord)
        {
            //Set the new Coord.
            _currentCoord = (absCoord == true) ? currentCoordInput : _previousCoord + currentCoordInput;

            //If the position changed, then mark that change occurred.
            _changed = (_previousCoord != _currentCoord) ? true : false;
            _positiveChanged = (_currentCoord > _previousCoord) ? true : false;
        }

        /// <summary>
        /// Sets the absolute current and previous coordinate information.
        /// </summary>
        /// <param name="initialCoordInput"></param>
        public void SetInitialCoord(double initialCoordInput)
        {
            _currentCoord = _previousCoord = initialCoordInput;
        }

        /// <summary>
        /// Sets new a new absolute value for the coordinate.
        /// Meant to be called and used with the RepRap G92 command.
        /// </summary>
        /// <param name="newCoordInput"></param>
        public void NewCoord(double newCoordInput)
        {
            _currentCoord = newCoordInput;

            //If the position changed, then mark that change occurred.
            _changed = (_previousCoord != _currentCoord) ? true : false;
            _positiveChanged = (_currentCoord > _previousCoord) ? true : false;
        }

        /// <summary>
        /// Prepares the coordinate for the next line of GCode processing.
        /// </summary>
        public void ResolveCoord()
        {
            _previousCoord = _currentCoord;
            _changed = false;
            _positiveChanged = false;
        }
        #endregion
    }
}
