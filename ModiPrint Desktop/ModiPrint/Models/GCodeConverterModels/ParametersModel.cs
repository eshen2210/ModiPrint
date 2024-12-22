using System.Windows;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using ModiPrint.Models.PrinterModels;
using ModiPrint.Models.PrinterModels.PrintheadModels;
using ModiPrint.Models.PrinterModels.PrintheadModels.PrintheadTypeModels;
using ModiPrint.Models.PrintModels.MaterialModels;
using ModiPrint.Models.PrintModels.PrintStyleModels;
using ModiPrint.Models.GCodeConverterModels.ProcessModels.ProcessG00Models;
using ModiPrint.ViewModels;

namespace ModiPrint.Models.GCodeConverterModels
{
    /// <summary>
    /// Pseudo-global parameters for the GCodeConverter that are passed around various GCodeConverter classes.
    /// </summary>
    public class ParametersModel
    {
        #region Fields and Properties
        //Contains parameters for the Printer.
        PrinterModel _printerModel;
        
        //The positions of the coordinates are kept in these classes.
        //References are set in the constructor.
        //The XYZ positions in the GCode.
        private CoordinateModel _xCoord;
        public CoordinateModel XCoord
        {
            get { return _xCoord; }
        }
        private CoordinateModel _yCoord;
        public CoordinateModel YCoord
        {
            get { return _yCoord; }
        }
        private CoordinateModel _zCoord;
        public CoordinateModel ZCoord
        {
            get { return _zCoord; }
        }
        private CoordinateModel _eRepRapCoord;
        public CoordinateModel ERepRapCoord
        {
            get { return _eRepRapCoord; }
        }

        //The XYZ coordinates for RepRap should match the XYZ coordinates in ModiPrint.
        //However, the E coordinates are different for the different software.
        //Therefore, this program needs to track the E coordinates for each of its printheads.
        private List<CoordinateModel> _eModiPrintCoordList = new List<CoordinateModel>();
        public List<CoordinateModel> EModiPrintCoordList
        {
            get { return _eModiPrintCoordList; }
        }

        //Used if a Print Style is set to Droplet.
        //Keeps track of the position of the last droplet and ensures the interpolate distance parameters is not violated.
        private DropletModel _dropletModel;
        public DropletModel DropletModel
        {
            get { return _dropletModel; }
        }

        //If true, then this line should use G01/G02/G11/G12, etc.
        //If false, then this line should use G00.
        private bool _isPrinting;
        public bool IsPrinting
        {
            get { return _isPrinting; }
            set { _isPrinting = value; }
        }

        //Is the Motorized Printhead currently retracted?
        private bool _isRetracted;
        public bool IsRetracted
        {
            get { return _isRetracted; }
            set { _isRetracted = value; }
        }

        //True = absolute coordinate system for Axis movement interpretation where the origin (0, 0, 0, 0) is set to the position at the start of this Print.
        //False = relative coordinate system for Axis movement interpretation.
        private bool _absCoordAxis = true;
        public bool AbsCoordAxis
        {
            get { return _absCoordAxis; }
            set { _absCoordAxis = value; }
        }

        //True = absolute coordinate system for extruder movement interpretation.
        //False = relative coordinate system for extruder movement interpretation.
        private bool _absCoordExtruder = true;
        public bool AbsCoordExtruder
        {
            get { return _absCoordAxis; }
            set { _absCoordExtruder = value; }
        }

        //Contains functions required to report errors to the GUI.
        private ErrorReporterViewModel _errorReporterViewModel;
        public ErrorReporterViewModel ErrorReporterViewModel
        {
            get { return _errorReporterViewModel; }
        }

        //The current line of RepRap GCode that is being processed.
        private int _repRapLine = 0;
        public int RepRapLine
        {
            get { return _repRapLine; }
            set { _repRapLine = value; }
        }

        //Activates whenever the GCode Converter has processed more lines of code.
        public event GCodeConverterLineConvertedEventHandler LineConverted;
        private void OnLineConverted(string progress)
        {
            if (LineConverted != null)
            { LineConverted(this, progress); }
        }
        #endregion

        #region Constructor
        public ParametersModel(PrinterModel PrinterModel, ErrorListViewModel ErrorListViewModel)
        {
            _printerModel = PrinterModel;

            _xCoord = new CoordinateModel(CoordinateType.X, true, this, 0, 0, 0);
            _yCoord = new CoordinateModel(CoordinateType.Y, true, this, 0, 0, 0);
            _zCoord = new CoordinateModel(CoordinateType.Z, true, this, 0, 0, 0);
            _eRepRapCoord = new CoordinateModel(CoordinateType.S, false, this, 0, 0, 0);

            // _dropletModel is not set yet. A new material must be set before this class is instantiated.

            //This program needs to track the E coordinates for each of its printheads.
            //Some printheads, such as the valve-based ones, do not need to track E.
            //It's easier to just create an E coordinate for every printhead though.
            //The index of the E coordinate list equals the index of the Printer's Printheads list.
            foreach (PrintheadModel printheadModel in _printerModel.PrintheadModelList)
            {
                _eModiPrintCoordList.Add(new CoordinateModel(CoordinateType.E, true, this, 0, 0, 0));
            }

            _errorReporterViewModel = new ErrorReporterViewModel(ErrorListViewModel);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Create a new Coordinate Model for an Axis.
        /// Typically used when switching Axes or setting new Axis parameters.
        /// </summary>
        /// <param name="axisID"></param>
        /// <param name="position"></param>
        /// <param name="minPosition"></param>
        /// <param name="maxPosition"></param>
        public void SetNewXYZCoord(char axisID, double position, double minPosition, double maxPosition)
        {
            switch(axisID)
            {
                case 'X':
                    _xCoord = new CoordinateModel(CoordinateType.X, true, this, position, minPosition, maxPosition);
                    break;
                case 'Y':
                    _yCoord = new CoordinateModel(CoordinateType.Y, true, this, position, minPosition, maxPosition);
                    break;
                case 'Z':
                    _zCoord = new CoordinateModel(CoordinateType.Z, true, this, position, minPosition, maxPosition);
                    break;
            }
        }

        /// <summary>
        /// Create a new Coordinate Model for a Motorized Printhead.
        /// Typically used when switching Printheads.
        /// </summary>
        /// <param name="printheadModel"></param>
        /// <param name="position"></param>
        /// <param name="minPosition"></param>
        /// <param name="maxPosition"></param>
        public void SetNewECoord(PrintheadModel printheadModel, double position, double minPosition, double maxPosition)
        {
            _eModiPrintCoordList[FindEModiPrintCoordIndex(printheadModel)] = new CoordinateModel(CoordinateType.E, true, this, position, minPosition, maxPosition);
        }

        /// <summary>
        /// Reset parameters when a non-Droplet Print action is used.
        /// Returns the remaining movement (in mm) that has not yet been covered by droplet printing.
        /// Index 0 of the return value is X, 1 is Y, 2 is Z.
        /// When calling this method, the remaining movement in the return value should be executed.
        /// </summary>
        /// <param name="materialModel"></param>
        public double[] ResetDropletPrintParameters(MaterialModel currentMaterial, MaterialModel newMaterial)
        {
            //Return the remaining movement (in mm) from the previous material that was not covered by droplet printing.
            double[] remainingDistanceToMove = { 0, 0, 0 };
            if ((currentMaterial != null)
             && (currentMaterial.PrintStyle == PrintStyle.Droplet))
            {
                remainingDistanceToMove = _dropletModel.PreDistance;
            }

            //Set new droplet parameters if applicable.
            if (newMaterial == null)
            {
                //A new material is not being set but Droplet parameters still need to be reset.
                DropletPrintStyleModel dropletPrintStyleModel = (DropletPrintStyleModel)currentMaterial.PrintStyleModel;
                _dropletModel = new DropletModel(dropletPrintStyleModel);
            }
            else if (newMaterial.PrintStyle == PrintStyle.Droplet)
            {
                //A new material is being set. Create new Droplet parameters.
                DropletPrintStyleModel dropletPrintStyleModel = (DropletPrintStyleModel)newMaterial.PrintStyleModel;
                _dropletModel = new DropletModel(dropletPrintStyleModel);
            }

            return remainingDistanceToMove;
        }

        /// <summary>
        /// Returns an index for eModiPrintCoordList which contains the eModiPrintCoord associated with the current eRepRapCoord.
        /// </summary>
        /// <param name="currentMaterial"></param>
        /// <returns></returns>
        public int FindEModiPrintCoordIndex(PrintheadModel printheadModel)
        {
            //The List index of eModiPrintCoordList.
            //This same index in the Printer's PrintheadModelList has the CoordinateModel's associated Printhead.
            int eModiPrintCoordIndex = 0;
            for (eModiPrintCoordIndex = 0; eModiPrintCoordIndex < (_printerModel.PrintheadModelList.Count - 1); eModiPrintCoordIndex++)
            {
                if (printheadModel == _printerModel.PrintheadModelList[eModiPrintCoordIndex])
                { break; }
            }
            return eModiPrintCoordIndex;
        }

        /// <summary>
        /// Triggers the LineConverted event which displays information to the GUI.
        /// </summary>
        /// <param name="progress"></param>
        public void ReportProgress(string progress)
        {
            OnLineConverted(progress);
        }
        #endregion
    }
}
