using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModiPrint.Models.PrinterModels;
using ModiPrint.Models.PrinterModels.PrintheadModels;
using ModiPrint.Models.PrintModels.PrintStyleModels;

namespace ModiPrint.Models.PrintModels.MaterialModels
{
    public class MaterialModel
    {
        #region Fields and Properties
        //Name of this Material.
        private string _name;
        public string Name
        {
            get { return _name; }
        }
        
        //How RepRap identifies this material (e.g. "T0" for the first material).
        private string _repRapID;
        public string RepRapID
        {
            get { return _repRapID; }
            set { _repRapID = value; }
        }

        //Contains parameters regarding the printer.
        private PrinterModel _printerModel;

        //Which printhead will be dispensing this material?
        private PrintheadModel _printheadModel = null;
        public PrintheadModel PrintheadModel
        {
            get { return _printheadModel; }
            set { _printheadModel = value; }
        }

        //How is this Material being printed?
        //Type specific properties are contained in the PrintStyle object.
        private PrintStyleModel _printStyleModel;
        public PrintStyleModel PrintStyleModel
        {
            get { return _printStyleModel; }
            set { _printStyleModel = value; }
        }

        //How is this Material being printed?
        private PrintStyle _printStyle = PrintStyle.Unset;
        public PrintStyle PrintStyle
        {
            get { return _printStyle; }
            set
            {
                _printStyle = value;
                switch (_printStyle)
                {
                    case PrintStyle.Continuous:
                        _printStyleModel = new ContinuousPrintStyleModel();
                        break;
                    case PrintStyle.Droplet:
                        _printStyleModel = new DropletPrintStyleModel();
                        break;
                    case PrintStyle.Unset:
                        _printStyleModel = null;
                        break;
                }
            }
        }

        //Printing speed for XY axes and Z axes.
        private double _xyPrintSpeed;
        public double XYPrintSpeed
        {
            get { return _xyPrintSpeed; }
            set
            {
                if ((value >= 0)
                 && (value <= _printerModel.AxisModelList[0].MaxSpeed)
                 && (value <= _printerModel.AxisModelList[1].MaxSpeed))
                {
                    _xyPrintSpeed = value;
                }
            }
        }

        private double _zPrintSpeed;
        public double ZPrintSpeed
        {
            get { return _zPrintSpeed; }
            set
            {
                if ((_printheadModel != null)
                 && (value >= 0)
                 && (value <= _printheadModel.AttachedZAxisModel.MaxSpeed))
                {
                    _zPrintSpeed = value;
                }
            }
        }

        //Printing acceleration for each axis.
        private double _xyPrintAcceleration;
        public double XYPrintAcceleration
        {
            get { return _xyPrintAcceleration; }
            set
            {
                if ((value >= 0)
                 && (value <= _printerModel.AxisModelList[0].MaxAcceleration)
                 && (value <= _printerModel.AxisModelList[1].MaxAcceleration))
                {
                    _xyPrintAcceleration = value;
                }
            }
        }

        private double _zPrintAcceleration;
        public double ZPrintAcceleration
        {
            get { return _zPrintAcceleration; }
            set
            {
                if ((_printheadModel != null)
                 && (value >= 0)
                 && (_printheadModel.AttachedZAxisModel != null)
                 && (value <= _printheadModel.AttachedZAxisModel.MaxAcceleration))
                {
                    _zPrintAcceleration = value;
                }
            }
        }

        //Junction deviation determines junction speed of movemements.
        //The higher the value, the faster the junction speeds.
        //However, high values can cause harsh transitions in between movements and missed steps.
        //Value must be between 0 to 1 and is usually on the very low end (< 0.1).
        private double _junctionDeviation = 0;
        public double JunctionDeviation
        {
            get { return _junctionDeviation; }
            set { _junctionDeviation = value; }
        }

        //Pause a print sequence after switching to or before switching from this Material.
        //Manual actions may still be fire during this pause.
        private bool _pauseAfterActivating = false;
        public bool PauseAfterActivating
        {
            get { return _pauseAfterActivating; }
            set { _pauseAfterActivating = value; }
        }

        private bool _pauseBeforeDeactivating = false;
        public bool PauseBeforeDeactivating
        {
            get { return _pauseBeforeDeactivating; }
            set { _pauseBeforeDeactivating = value; }
        }
        #endregion

        #region Constructor
        public MaterialModel(string Name, PrinterModel PrinterModel)
        {
            _name = Name;

            _printerModel = PrinterModel;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Are all of the parameters set correctly such that printing can occur?
        /// </summary>
        /// <returns></returns>
        public bool ReadyToPrint()
        {
            if(!String.IsNullOrWhiteSpace(_repRapID)
            && (_printheadModel != null)
            && (_printStyle != PrintStyle.Unset)
            && (_xyPrintSpeed > 0)
            && (_zPrintSpeed > 0)
            && (_xyPrintAcceleration > 0)
            && (_zPrintAcceleration > 0))
            {
                return true;
            }
            return false;
        }
        #endregion
    }
}
