using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ModiPrint.Models.ManualControlModels;
using ModiPrint.ViewModels.RealTimeStatusViewModels;
using ModiPrint.ViewModels.PrinterViewModels;

namespace ModiPrint.ViewModels.ManualControlViewModels
{
    /// <summary>
    /// Interface between CalibrationModel and the GUI.
    /// </summary>
    public class CalibrationViewModel : ViewModel
    {
        #region Fields and Properties
        //Contains functions that automatically calibrate the Printer's position and range.
        private CalibrationModel _calibrationModel;
        public CalibrationModel CalibrationModel
        {
            get { return _calibrationModel; }
        }
        private ManualControlViewModel _manualControlViewModel;

        //Displays information on the Printer's parameters during operations.
        private RealTimeStatusDataViewModel _realTimeStatusDataViewModel;

        //Contains Printer parameters.
        private PrinterViewModel _printerViewModel;

        //Parameters for setting the movement speeds of hitting all limit switches.
        //In mm/s.
        private double _xCalibrationSpeed = 10;
        public double XCalibrationSpeed
        {
            get { return _xCalibrationSpeed;  }
            set { _xCalibrationSpeed = value; }
        }

        private double _yCalibrationSpeed = 10;
        public double YCalibrationSpeed
        {
            get { return _yCalibrationSpeed; }
            set { _yCalibrationSpeed = value; }
        }

        private double _zCalibrationSpeed = 10;
        public double ZCalibrationSpeed
        {
            get { return _zCalibrationSpeed; }
            set { _zCalibrationSpeed = value; }
        }

        //Parameters for setting the origin.
        //Distance from the center.
        //In mm.
        private double _xDistanceFromCenter = 0;
        public double XDistanceFromCenter
        {
            get { return _xDistanceFromCenter; }
            set { _xDistanceFromCenter = value; }
        }

        private double _yDistanceFromCenter = 0;
        public double YDistanceFromCenter
        {
            get { return _yDistanceFromCenter; }
            set { _yDistanceFromCenter = value; }
        }
        #endregion

        #region Constructor
        public CalibrationViewModel(CalibrationModel CalibrationModel, ManualControlViewModel ManualControlViewModel, RealTimeStatusDataViewModel RealTimeStatusDataViewModel, PrinterViewModel PrinterViewModel)
        {
            _calibrationModel = CalibrationModel;
            _manualControlViewModel = ManualControlViewModel;
            _realTimeStatusDataViewModel = RealTimeStatusDataViewModel;
            _printerViewModel = PrinterViewModel;
        }
        #endregion

        #region Commands
        /// <summary>
        /// Sends outgoing commands that retracts all Z Axes and moves X and Y Axes to the limit switches.
        /// Then moves the X and Y axes to the center of the print surface and set min and max positions such that the current position is the origin.
        /// </summary>
        private RelayCommand<object> _homeCommand;
        public ICommand HomeCommand
        {
            get
            {
                if (_homeCommand == null)
                { _homeCommand = new RelayCommand<object>(ExecuteHomeCommand, CanExecuteHomeCommand); }
                return _homeCommand;
            }
        }

        public bool CanExecuteHomeCommand(object notUsed)
        {
            return true;
        }

        public void ExecuteHomeCommand(object notUsed)
        {
            _calibrationModel.Home(_xCalibrationSpeed, _yCalibrationSpeed, _zCalibrationSpeed, _xDistanceFromCenter, _yDistanceFromCenter);
            _manualControlViewModel.Menu = "Base";
        }

        /// <summary>
        /// Sends outgoing commands that mark the position of the currently active Printhead's point to printing (centered and closest to the print surface).
        /// Should be called directly after setting the XY origin to zero.
        /// </summary>
        private RelayCommand<object> _calibratePrintheadOffsetCommand;
        public ICommand CalibratePrintheadOffsetCommand
        {
            get
            {
                if (_calibratePrintheadOffsetCommand == null)
                { _calibratePrintheadOffsetCommand = new RelayCommand<object>(ExecuteCalibratePrintheadOffsetCommand, CanExecuteCalibratePrintheadOffsetCommand); }
                return _calibratePrintheadOffsetCommand;
            }
        }

        public bool CanExecuteCalibratePrintheadOffsetCommand(object notUsed)
        {
            return true;
        }

        public void ExecuteCalibratePrintheadOffsetCommand(object notUsed)
        {
            _calibrationModel.CalibratePrintheadOffset();
        }
        #endregion
    }
}
