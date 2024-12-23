using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModiPrint.Models.PrinterModels.AxisModels;
using ModiPrint.Models.PrinterModels.PrintheadModels;
using ModiPrint.Models.PrinterModels.MicrocontrollerModels;

namespace ModiPrint.Models.PrinterModels
{
    /// <summary>
    /// Contains parameters of the printer.
    /// </summary>
    public class PrinterModel
    {
        #region Fields and Properties
        //Microcontroller and its GPIO Pins.
        private MicrocontrollerModel _microcontrollerModel;
        public MicrocontrollerModel MicroControllerModel
        {
            get { return _microcontrollerModel; }
        }
        
        //List of all AxisModels.
        private List<AxisModel> _axisModelList = new List<AxisModel>();
        public List<AxisModel> AxisModelList
        {
            get { return _axisModelList; }
        }

        //List of all Z AxisModels.
        private List<AxisModel> _zAxisModelList = new List<AxisModel>();
        public List<AxisModel> ZAxisModelList
        {
            get { return _zAxisModelList; }
        }

        //Keeps track of the number of Z Axes that have been created.
        //Does not decrease when Printheads are removed.
        //Used such that Z Axes Names do not overlap.
        private int _zAxesCreatedCount = 0;
        public int ZAxesCreatedCount
        {
            get { return _zAxesCreatedCount; }
            set { _zAxesCreatedCount = value; }
        }

        //List of all PrintheadModels.
        private List<PrintheadModel> _printheadModelList = new List<PrintheadModel>();
        public List<PrintheadModel> PrintheadModelList
        {
            get { return _printheadModelList; }
        }

        //Keeps track of the number of Printheads that have been created.
        //Does not decrease when Printheads are removed.
        //Used such that Printhead Names do not overlap.
        private int _printheadsCreatedCount = 0;
        public int PrintheadsCreatedCount
        {
            get { return _printheadsCreatedCount; }
            set { _printheadsCreatedCount = value; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Builds a minimal printer with an X Axis, a Y Axis, a Z Axis, and one Printhead.
        /// </summary>
        public PrinterModel()
        {
            _microcontrollerModel = new MicrocontrollerModel();

            _axisModelList.Add(new AxisModel("X Actuator", 'X'));
            _axisModelList.Add(new AxisModel("Y Actuator", 'Y'));
            AddZAxis();

            AddPrinthead();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds one Printhead to the Printer.
        /// </summary>
        public void AddPrinthead()
        {
            string newPrintheadName = "Printhead " + ++_printheadsCreatedCount;

            PrintheadModel newPrinthead = new PrintheadModel(newPrintheadName);
            _printheadModelList.Add(newPrinthead);
        }

        /// <summary>
        /// Adds one Printhead with a Name to the Printer.
        /// </summary>
        /// <param Name="printheadName"></param>
        /// <remarks>
        /// This method was created for the XML Serializers.
        /// </remarks>
        public void AddPrinthead(string printheadName)
        {
            PrintheadModel newPrinthead = new PrintheadModel(printheadName);
            _printheadModelList.Add(newPrinthead);
        }

        /// <summary>
        /// Removes one Printhead with the specified Name from the Printer.
        /// Returns true if the Printhead was removed. Otherwise return false.
        /// </summary>
        /// <param name="printheadName"></param>
        public bool RemovePrinthead(string printheadName)
        {
            if (_printheadModelList.Count > 1) //The Printer must have at least 1 Printhead.
            {
                for (int index = 0; index < _printheadModelList.Count; index++)
                {
                    if (printheadName == _printheadModelList[index].Name)
                    {
                        _printheadModelList[index].UnattachGPIOPins();
                        _printheadModelList.RemoveAt(index);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Finds and returns a reference to an PrintheadModel with the matching Name property.
        /// </summary>
        /// <param name="printheadName"></param>
        public PrintheadModel FindPrinthead(string printheadName)
        {
            foreach (PrintheadModel printheadModel in _printheadModelList)
            {
                if (printheadName == printheadModel.Name)
                {
                    return printheadModel;
                }
            }
            return null;
        }

        /// <summary>
        /// Adds one Z Axis to the Printer.
        /// </summary>
        public void AddZAxis()
        {
            AxisModel newZAxis = new AxisModel("Z Actuator " + ++_zAxesCreatedCount, 'Z');
            _axisModelList.Add(newZAxis);
            _zAxisModelList.Add(newZAxis);
        }

        /// <summary>
        /// Adds one Z Axis with a Name to the Printer.
        /// </summary>
        /// <param name="name"></param>
        public void AddZAxis(string name)
        {
            AxisModel newZAxis = new AxisModel(name, 'Z');
            _axisModelList.Add(newZAxis);
            _zAxisModelList.Add(newZAxis);
        }

        /// <summary>
        /// Removes one Z Axis with the specified name from the printer.
        /// Return true if Z Axis was removed. Otherwise return false.
        /// </summary>
        /// <param name="axisName"></param>
        public bool RemoveZAxis(string axisName)
        {
            if (_axisModelList.Count > 3) //The printer must have at least 3 axes.
            {
                for (int index = 0; index < _axisModelList.Count; index++)
                {
                    if (axisName == _axisModelList[index].Name
                        && _axisModelList[index].IsRemovable)
                    {
                        _axisModelList[index].UnattachGPIOPins();
                        _axisModelList.RemoveAt(index);
                        _zAxisModelList.RemoveAt(index - 2);
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Finds and returns a reference to an PrintheadModel with the matching Name property.
        /// </summary>
        /// <param name="axisName"></param>
        /// <returns></returns>
        public AxisModel FindAxis(string axisName)
        {
            foreach (AxisModel axisModel in _axisModelList)
            {
                if (axisName == axisModel.Name)
                {
                    return axisModel;
                }
            }
            return null;
        }

        /// <summary>
        /// If all parameters are set appropriately for a print, then return true.
        /// </summary>
        public bool ReadyToPrint()
        {
            //Are all Axes ready to print?
            foreach (AxisModel axisModel in _axisModelList)
            {
                if (axisModel.ReadyToPrint() == false)
                { return false; }
            }

            //Are all Printheads ready to print?
            foreach (PrintheadModel printheadModel in _printheadModelList)
            {
                if (printheadModel.ReadyToPrint() == false)
                { return false; }
            }

            return true;
        }
        #endregion
    }
}
