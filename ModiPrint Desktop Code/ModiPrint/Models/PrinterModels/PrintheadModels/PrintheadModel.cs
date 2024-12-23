using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ModiPrint.Models.PrinterModels.AxisModels;
using ModiPrint.Models.PrinterModels.PrintheadModels.PrintheadTypeModels;

namespace ModiPrint.Models.PrinterModels.PrintheadModels
{
    /// <summary>
    /// Manages all of the properties of a printhead.
    /// </summary>
    public class PrintheadModel
    {
        #region Fields and Properties
        //How ModiPrint identifies this printhead in the GUI.
        private string _name;
        public string Name
        {
            get { return _name; }
        }

        //What type of dispensing mechanism does this printhead have?
        //Type specific properties are contained in PrintheadType class.
        private PrintheadTypeModel _printheadTypeModel;
        public PrintheadTypeModel PrintheadTypeModel
        {
            get { return _printheadTypeModel; }
        }
        private PrintheadType _printheadType = PrintheadType.Unset;
        public PrintheadType PrintheadType
        {
            get { return _printheadType; }
            set
            {
                _printheadType = value;
                switch (_printheadType)
                {
                    case PrintheadType.Motorized:
                        _printheadTypeModel = new MotorizedPrintheadTypeModel(_name);
                        break;
                    case PrintheadType.Valve:
                        _printheadTypeModel = new ValvePrintheadTypeModel(_name);
                        break;
                    case PrintheadType.Custom:
                        _printheadTypeModel = new CustomPrintheadTypeModel(_name);
                        break;
                    case PrintheadType.Unset:
                        _printheadTypeModel = null;
                        break;
                }
            }
        }

        //Name of the Z Axis that this printhead is attached to.
        private AxisModel _attachedZAxisModel;
        public AxisModel AttachedZAxisModel
        {
            get { return _attachedZAxisModel; }
            set { _attachedZAxisModel = value; }
        }

        //Offset of the Printhead.
        //In milimeters.
        //Note: These values are relative and do not specify a specific position on the print surface.
        //Offset values are compared to other offset values to determine new positioning when switching Printheads.
        private double _xOffset = 0;
        public double XOffset
        {
            get { return _xOffset; }
            set { _xOffset = value; }
        }

        private double _yOffset = 0;
        public double YOffset
        {
            get { return _yOffset; }
            set { _yOffset = value; }
        }
        #endregion

        #region Constructors
        public PrintheadModel(string Name)
        {
            _name = Name;
        }
        #endregion

        #region Methods
        /// <summary>
        /// If all parameters are set appropriately for a print, then return true.
        /// </summary>
        /// <returns></returns>
        public bool ReadyToPrint()
        {
            if ((_printheadType != PrintheadType.Unset)
             && (_attachedZAxisModel != null)
             && (_printheadTypeModel.ReadyToPrint() == true))
            {
                return true;
            }
            else
            {
                return false;
            }
             
        }

        /// <summary>
        /// Resets the parameters of all GPIO pins.
        /// Typically, this method is called when the equipment is removed.
        /// </summary>
        public void UnattachGPIOPins()
        {
            if (_printheadTypeModel != null)
            {
                _printheadTypeModel.UnattachGPIOPins();
            }
        }
        #endregion
    }
}

