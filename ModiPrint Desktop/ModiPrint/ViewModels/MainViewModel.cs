using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ModiPrint.Models.GCodeModels;
using ModiPrint.Models.GCodeConverterModels;
using ModiPrint.Models.PrinterModels;
using ModiPrint.Models.RealTimeStatusModels;
using ModiPrint.Models.ManualControlModels;
using ModiPrint.Models.PrintModels;
using ModiPrint.Models.SerialCommunicationModels;
using ModiPrint.ViewModels.PrinterViewModels;
using ModiPrint.ViewModels.PrintViewModels;
using ModiPrint.ViewModels.GCodeManagerViewModels;
using ModiPrint.ViewModels.SerialCommunicationViewModels;
using ModiPrint.ViewModels.RealTimeStatusViewModels;
using ModiPrint.ViewModels.ManualControlViewModels;
using ModiPrint.ViewModels.SettingsViewModels;

namespace ModiPrint.ViewModels
{
    /// <summary>
    /// MainViewModel is a ViewModel that instantiates all Model and all other ViewModel classes.
    /// MainViewModel injects Model references to ViewModels that require them.
    /// </summary>
    public class MainViewModel : ViewModel
    {
        #region Declare Classes
        //Models.
        private SerialCommunicationMainModel _serialCommunicationMainModel;
        public SerialCommunicationMainModel SerialCommunicationMainModel
        {
            get { return _serialCommunicationMainModel; }
        }

        private SerialCommunicationOutgoingMessagesModel _serialCommunicationOutgoingMessagesModel;

        private RealTimeStatusDataModel _realTimeStatusDataModel;

        private ManualControlModel _manualControlModel;

        private CalibrationModel _calibrationModel;

        private RealTimeStatusSerialInterpreterModel _realTimeStatusSerialInterpreterModel;

        private GCodeModel _uploadedGCodeModel;

        private GCodeFileManagerModel _gCodeFileManagerModel;

        private GCodeConverterModel _gCodeConverterModel;

        private PrinterModel _printerModel;

        private PrintModel _printModel;


        //ViewModels.
        private ErrorListViewModel _errorListViewModel;
        public ErrorListViewModel ErrorListViewModel
        {
            get { return _errorListViewModel; }
        }

        private GCodeManagerViewModel _gcodeManagerViewModel;
        public GCodeManagerViewModel GCodeManagerViewModel
        {
            get { return _gcodeManagerViewModel; }
        }

        private SerialCommunicationViewModel _serialCommunicationViewModel;
        public SerialCommunicationViewModel SerialCommunicationViewModel
        {
            get { return _serialCommunicationViewModel; }
        }

        private SerialMessageDisplayViewModel _serialMessageDisplayViewModel;
        public SerialMessageDisplayViewModel SerialMessageDisplayViewModel
        {
            get { return _serialMessageDisplayViewModel; }
        }

        private RealTimeStatusDataViewModel _realTimeStatusDataViewModel;
        public RealTimeStatusDataViewModel RealTimeStatusDataViewModel
        {
            get { return _realTimeStatusDataViewModel; }
        }

        private ManualControlViewModel _manualControlViewModel;
        public ManualControlViewModel ManualControlViewModel
        {
            get { return _manualControlViewModel; }
        }

        private CalibrationViewModel _calibrationViewModel;
        public CalibrationViewModel CalibrationViewModel
        {
            get { return _calibrationViewModel; }
        }

        private PrinterViewModel _printerViewModel;
        public PrinterViewModel PrinterViewModel
        {
            get { return _printerViewModel; }
        }

        private PrintViewModel _printViewModel;
        public PrintViewModel PrintViewModel
        {
            get { return _printViewModel; }
        }

        private PrintExecuteViewModel _printExecuteViewModel;
        public PrintExecuteViewModel PrintExecuteViewModel
        {
            get { return _printExecuteViewModel; }
        }

        private SaveLoadViewModel _saveLoadViewModel;
        public SaveLoadViewModel SaveLoadViewModel
        {
            get { return _saveLoadViewModel; }
        }

        private UnsetMainViewModel _unsetMainViewModel;
        public UnsetMainViewModel UnsetMainViewModel
        {
            get { return _unsetMainViewModel; }
        }
        #endregion

        #region GUI-Related Fields and Properties
        //A string value that controls what menu is displayed in the main window.
        private string _menu;
        public string Menu
        {
            get { return _menu; }
        }
        #endregion

        #region Constructor
        public MainViewModel()
        {                        
            //Error Handling.
            _errorListViewModel = new ErrorListViewModel();
            
            //Printer Model.
            _printerModel = new PrinterModel();

            //Print Model.
            _printModel = new PrintModel(_printerModel);

            //Serial Communication Incoming and Outgoing Message Interpreter.
            _realTimeStatusDataModel = new RealTimeStatusDataModel(_printerModel);

            //Serial Communication.
            _serialCommunicationOutgoingMessagesModel = new SerialCommunicationOutgoingMessagesModel();
            _serialCommunicationMainModel = new SerialCommunicationMainModel(_serialCommunicationOutgoingMessagesModel, _printerModel, _printModel, _realTimeStatusDataModel, _errorListViewModel);
            _serialMessageDisplayViewModel = new SerialMessageDisplayViewModel();
            _serialCommunicationViewModel = new SerialCommunicationViewModel(_serialCommunicationMainModel, _serialCommunicationOutgoingMessagesModel, _serialMessageDisplayViewModel);

            //Printer View Model.
            _printerViewModel = new PrinterViewModel(_printerModel, _serialCommunicationMainModel.SerialCommunicationCommandSetsModel);

            //Print View Model.
            _printViewModel = new PrintViewModel(_printModel, _serialMessageDisplayViewModel);

            //Real Time Status.
            _realTimeStatusSerialInterpreterModel = new RealTimeStatusSerialInterpreterModel(_serialCommunicationMainModel, _printerModel, _printerViewModel, _realTimeStatusDataModel, _errorListViewModel);
            _realTimeStatusDataViewModel = new RealTimeStatusDataViewModel(_realTimeStatusDataModel, _printerViewModel, _serialCommunicationMainModel.SerialCommunicationCommandSetsModel, _errorListViewModel);

            //Manual Commmands and Calibration.
            _manualControlModel = new ManualControlModel(_printerModel, _serialCommunicationOutgoingMessagesModel, _realTimeStatusDataModel, _errorListViewModel);
            _calibrationModel = new CalibrationModel(_realTimeStatusDataModel, _printerModel, _serialCommunicationOutgoingMessagesModel, _errorListViewModel);
            _manualControlViewModel = new ManualControlViewModel(_manualControlModel, _realTimeStatusDataViewModel, _printerViewModel);
            _calibrationViewModel = new CalibrationViewModel(_calibrationModel, _manualControlViewModel, _realTimeStatusDataViewModel, _printerViewModel);

            //GCode.
            _uploadedGCodeModel = new GCodeModel();
            _gCodeFileManagerModel = new GCodeFileManagerModel(_uploadedGCodeModel, _errorListViewModel);
            _gCodeConverterModel = new GCodeConverterModel(_printerModel, _printModel, _realTimeStatusDataModel, _errorListViewModel);
            _gcodeManagerViewModel = new GCodeManagerViewModel(_gCodeFileManagerModel, _gCodeConverterModel, _printViewModel);

            //Printing.
            _printExecuteViewModel = new PrintExecuteViewModel(_gcodeManagerViewModel, _realTimeStatusDataModel, _calibrationViewModel, _serialCommunicationViewModel, _serialCommunicationOutgoingMessagesModel, _serialMessageDisplayViewModel, _printViewModel);

            //Settings.
            _saveLoadViewModel = new SaveLoadViewModel(_gcodeManagerViewModel, _printerViewModel, _printViewModel, _errorListViewModel);

            //Unset Main Window.
            _unsetMainViewModel = new UnsetMainViewModel();
        }
        #endregion

        #region Commands
        /// <summary>
        /// Closes the window.
        /// </summary>
        private RelayCommand<object> _closeWindowCommand;
        public ICommand CloseWindowCommand
        {
            get
            {
                if (_closeWindowCommand == null)
                { _closeWindowCommand = new RelayCommand<object>(ExecuteCloseWindowCommand, CanExecuteCloseWindowCommand); }
                return _closeWindowCommand;
            }
        }

        public bool CanExecuteCloseWindowCommand(object notUsed)
        {
            return true;
        }

        public void ExecuteCloseWindowCommand(object parameter)
        {
            Window mainWindow = (Window)parameter;
            mainWindow.Close();
        }

        /// <summary>
        /// Maxmimizes the window.
        /// </summary>
        private RelayCommand<object> _maximizeWindowCommand;
        public ICommand MaximizeWindowCommand
        {
            get
            {
                if (_maximizeWindowCommand == null)
                { _maximizeWindowCommand = new RelayCommand<object>(ExecuteMaximizeWindowCommand, CanExecuteMaximizeWindowCommand); }
                return _maximizeWindowCommand;
            }
        }

        public bool CanExecuteMaximizeWindowCommand(object notUsed)
        {
            return true;
        }

        public void ExecuteMaximizeWindowCommand(object parameter)
        {
            Window mainWindow = (Window)parameter;
            mainWindow.WindowState = WindowState.Maximized;
        }

        /// <summary>
        /// Minimize the window
        /// </summary>
        private RelayCommand<object> _minimizeWindowCommand;
        public ICommand MinimizeWindowCommand
        {
            get
            {
                if (_minimizeWindowCommand == null)
                { _minimizeWindowCommand = new RelayCommand<object>(ExecuteMinimizeWindowCommand, CanExecuteMinimizeWindowCommand); }
                return _minimizeWindowCommand;
            }
        }

        public bool CanExecuteMinimizeWindowCommand(object notUsed)
        {
            return true;
        }

        public void ExecuteMinimizeWindowCommand(object parameter)
        {
            Window mainWindow = (Window)parameter;
            mainWindow.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Change the menu displayed on the main window.
        /// </summary>
        private RelayCommand<string> _setMenuCommand;
        public ICommand SetMenuCommand
        {
            get
            {
                if (_setMenuCommand == null)
                { _setMenuCommand = new RelayCommand<string>(ExecuteSetMenuCommand, CanExecuteSetMenuCommand); }
                return _setMenuCommand;
            }
        }

        public bool CanExecuteSetMenuCommand(string menu)
        {
            return true;
        }

        public void ExecuteSetMenuCommand(string menu)
        {
            _menu = menu;
            OnPropertyChanged("Menu");

            //Resets the Attention Property in ErrorListViewModel once errors have been given attention.
            if (_menu == "Errors")
            {
                _errorListViewModel.Attention = false;
            }
        }
        #endregion
    }
}
