using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using ModiPrint.Models.GCodeModels;
using ModiPrint.Models.SerialCommunicationModels;
using ModiPrint.Models.RealTimeStatusModels;
using ModiPrint.Models.PrinterModels.PrintheadModels.PrintheadTypeModels;
using ModiPrint.Models.ManualControlModels;
using ModiPrint.ViewModels.PrintViewModels.MaterialViewModels;
using ModiPrint.ViewModels.GCodeManagerViewModels;
using ModiPrint.ViewModels.SerialCommunicationViewModels;
using ModiPrint.ViewModels.ManualControlViewModels;

namespace ModiPrint.ViewModels.PrintViewModels
{
    /// <summary>
    /// Associated with PrintExecuteViewModel.
    /// Represents the current actions of the Printer.
    /// </summary>
    public enum PrintStatus
    {
        Manual, //Idle or executing manual commands.
        Calibration, //Executing calibration sequence.
        Printing, //In the process of printing.
        MicrocontrollerPaused, //In the process of printing but the microcontroller protocol is on pause. No actions can be taken except unpausing.
        PrintSequencePaused //In the process of printing but this program's protocol is on pause. Manual actions may be taken before resuming.
    }

    /// <summary>
    /// Contains functions to begin Printing and perform pre-Print checks.
    /// </summary>
    public class PrintExecuteViewModel : ViewModel
    {
        #region Fields and Properties
        //Contains information about the state of the Printer, Print, and Serial Communication settings.
        //This information will be used to determine if the Printer is ready to print.
        private GCodeModel _uploadedGCodeModel;
        private GCodeManagerViewModel _gCodeManagerViewModel;
        private SerialCommunicationViewModel _serialCommunicationViewModel;
        private RealTimeStatusDataModel _realTimeStatusDataModel;
        private CalibrationViewModel _calibrationViewModel;
        private PrintViewModel _printViewModel;

        //Contains function for outputting and receiving messages from the serial communicator.
        private SerialCommunicationOutgoingMessagesModel _serialCommunicationOutgoingMessagesModel;
        private SerialMessageDisplayViewModel _serialMessageDisplayViewModel;

        //Current state of operations of the Printer.
        public PrintStatus PrintStatus
        {
            get { return _realTimeStatusDataModel.PrintStatus; }
            set
            {
                _previousPrintStatus = PrintStatus;
                _realTimeStatusDataModel.PrintStatus = value;
                OnPropertyChanged("PrintStatus");
                OnPropertyChanged("IsReadyToPrint");
            }
        }

        //Previous state of operation of the Printer.
        private PrintStatus _previousPrintStatus = PrintStatus.Manual;

        //Are various aspects of this program ready to execute a Print?
        public bool IsReadyToPrint
        {
            get
            {
                if ((IsGCodeReadyToPrint == true)
                 && (IsSerialCommunicationReadyToPrint == true)
                 && (IsActivePrintheadReadyToPrint)
                 && (PrintStatus == PrintStatus.Manual))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        //If true, display a menu to execute a print sequence abort.
        private bool _abortPrompt = false;
        public bool AbortPrompt
        {
            get { return _abortPrompt; }
        }

        public bool IsGCodeReadyToPrint
        {
            get
            {
                if ((!String.IsNullOrWhiteSpace(_gCodeManagerViewModel.UploadedGCodeModel.GCodeStr))
                 && ((_printViewModel.AvailibleRepRapIDSet() == true)
                  || (_gCodeManagerViewModel.UploadedGCodeType == GCodeType.ModiPrint)))
                {
                    return true;    
                }
                else
                {
                    return false;
                }
            }
        }

        public bool IsSerialCommunicationReadyToPrint
        {
            get { return _serialCommunicationViewModel.IsPortOpen; }
        }

        public bool IsActivePrintheadReadyToPrint
        {
            get { return (_realTimeStatusDataModel.ActivePrintheadType != PrintheadType.Unset) ? true : false; }
        }
        #endregion

        #region Constructor
        public PrintExecuteViewModel(GCodeManagerViewModel GCodeManagerViewModel, 
            RealTimeStatusDataModel RealTimeStatusDataModel, CalibrationViewModel CalibrationViewModel,
            SerialCommunicationViewModel SerialCommunicationViewModel, SerialCommunicationOutgoingMessagesModel SerialCommunicationOutgoingMessagesModel, 
            SerialMessageDisplayViewModel SerialMessageDisplayViewModel, PrintViewModel PrintViewModel)
        {
            _gCodeManagerViewModel = GCodeManagerViewModel;
            _realTimeStatusDataModel = RealTimeStatusDataModel;
            _calibrationViewModel = CalibrationViewModel;
            _serialCommunicationViewModel = SerialCommunicationViewModel;
            _serialCommunicationOutgoingMessagesModel = SerialCommunicationOutgoingMessagesModel;
            _serialMessageDisplayViewModel = SerialMessageDisplayViewModel;
            _printViewModel = PrintViewModel;

            _gCodeManagerViewModel.GCodeFileUploaded += new GCodeFileUploadedEventHandler(UpdateUploadedGCode);
            _printViewModel.PrintViewModelRepRapIDChanged += new PrintViewModelRepRapIDChanged(UpdateRepRapIDSet);

            _serialCommunicationViewModel.SerialCommunicationMainModel.SerialConnectionChanged += new SerialConnectionChangedEventHandler(UpdateSerialConnection);
            _serialCommunicationViewModel.SerialCommunicationMainModel.SerialCommunicationCompleted += new SerialCommunicationCompletedEventHandler(UpdatePrintFinished);
            _serialCommunicationViewModel.SerialCommunicationMainModel.SerialCommunicationPrintSequencePaused += new SerialCommunicationPrintSequencePausedEventHandler(UpdatePrintSequencePaused);
            _serialCommunicationViewModel.SerialCommunicationMainModel.SerialCommunicationMicrocontrollerResumed += new SerialCommunicationMicrocontrollerResumedEventHandler(UpdateMicrocontrollerResumed);

            _realTimeStatusDataModel.RecordSetMotorizedPrintheadExecuted += new RecordSetMotorizedPrintheadExecutedEventHandler(UpdateActivePrintheadType);
            _realTimeStatusDataModel.RecordSetValvePrintheadExecuted += new RecordSetValvePrintheadExecutedEventHandler(UpdateActivePrintheadType);
            _realTimeStatusDataModel.RecordLimitExecuted += new RecordLimitExecutedEventHandler(UpdateLimitHit);

            _calibrationViewModel.CalibrationModel.CalibrationBegun += new CalibrationBegunEventHandler(UpdateCalibrationBegun);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Queues each line of g-code for printing.
        /// </summary>
        public void Print()
        {
            //Queue each line of GCode for printing.
            string modiPrintGCodeStr = _gCodeManagerViewModel.GetModiPrintGCode();
            string[] modiPrintGCodeArr = modiPrintGCodeStr.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            List<string> modiPrintGCodeList = modiPrintGCodeArr.ToList<string>();
            _serialCommunicationOutgoingMessagesModel.AppendProspectiveOutgoingMessage(modiPrintGCodeList);
        }

        /// <summary>
        /// Linked to ModiPrintGCodeChangedEventHandler.
        /// Fires when ModiPrint g-code is converted or loaded.
        /// </summary>
        /// <param name="sender"></param>
        private void UpdateUploadedGCode(object sender)
        {
            OnPropertyChanged("IsGCodeReadyToPrint");
            OnPropertyChanged("IsReadyToPrint");
        }

        /// <summary>
        /// Linked to 
        /// </summary>
        /// <param name="sender"></param>
        private void UpdateRepRapIDSet(object sender)
        {
            OnPropertyChanged("IsGCodeReadyToPrint");
            OnPropertyChanged("IsReadyToPrint");
        }

        /// <summary>
        /// Linked to SerialConnectionChangedEventHandler.
        /// Fires when serial port is connected or disconnected.
        /// </summary>
        /// <param name="sender"></param>
        private void UpdateSerialConnection(object sender)
        {
            OnPropertyChanged("IsSerialCommunicationReadyToPrint");
            OnPropertyChanged("IsReadyToPrint");
        }

        /// <summary>
        /// Linked to RecordSetMotorizedPrintheadExecutedEventHandler and RecordSetValvePrintheadExecutedEventHandler.
        /// Fires when a new Printhead is set.
        /// </summary>
        private void UpdateActivePrintheadType(string printheadName)
        {
            OnPropertyChanged("IsActivePrintheadReadyToPrint");
            OnPropertyChanged("IsReadyToPrint");
        }

        /// <summary>
        /// Linked to SerialCommunicationFinishedEventHandler.
        /// Fires when a print sequence is finished.
        /// </summary>
        private void UpdatePrintFinished(object sender)
        {
            PrintStatus = PrintStatus.Manual;
            if (_printCommand != null)
            { _printCommand.RaiseCanExecuteChanged(); }
        }

        /// <summary>
        /// Linked to SerialCommunicationPrintSequencePausedEventHandler.
        /// Fires when a print sequence is paused.
        /// </summary>
        /// <param name="sender"></param>
        private void UpdatePrintSequencePaused(object sender)
        {
            if (PrintStatus == PrintStatus.Printing)
            {
                PrintStatus = PrintStatus.PrintSequencePaused;
                if (_resumeCommand != null)
                { _resumeCommand.RaiseCanExecuteChanged(); }
            }
        }

        /// <summary>
        /// Linked to RecordLimitExecutedEventHandler.
        /// Fires when a Limit Switch is hit.
        /// </summary>
        private void UpdateLimitHit()
        {
            if (PrintStatus == PrintStatus.Printing)
            {
                PrintStatus = PrintStatus.MicrocontrollerPaused;
                if (_pauseCommand != null)
                { _pauseCommand.RaiseCanExecuteChanged(); }
                if (_resumeCommand != null)
                { _resumeCommand.RaiseCanExecuteChanged(); }
            }
            else if ((PrintStatus == PrintStatus.Calibration)
                  || (PrintStatus == PrintStatus.Manual))
            {
                _serialCommunicationViewModel.SerialCommunicationMainModel.ResumeMicrocontroller();
            }
        }

        /// <summary>
        /// Linked to the SerialCommunicationsMicrocontrollerResumedEventHandler.
        /// Fires when a microcontroller resumed message is sent.
        /// </summary>
        private void UpdateMicrocontrollerResumed(object sender)
        {
            PrintStatus = _previousPrintStatus;
        }

        /// <summary>
        /// Lined to the CalibrationBegunEventHandler.
        /// Fires when a calibration sequence has begun.
        /// </summary>
        private void UpdateCalibrationBegun()
        {
            PrintStatus = PrintStatus.Calibration;
        }
        #endregion

        #region Commands
        /// <summary>
        /// Begins printing.
        /// </summary>
        private RelayCommand<object> _printCommand;
        public ICommand PrintCommand
        {
            get
            {
                if (_printCommand == null)
                { _printCommand = new RelayCommand<object>(ExecutePrintCommand, CanExecutePrintCommand); }
                return _printCommand;
            }
        }

        //To Do: grey out the print button once printing has begun.
        public bool CanExecutePrintCommand(object notUsed)
        {
            return IsReadyToPrint;
        }

        public void ExecutePrintCommand(object notUsed)
        {
            PrintStatus = PrintStatus.Printing;
            Print();
        }

        /// <summary>
        /// Pauses printing.
        /// </summary>
        private RelayCommand<object> _pauseCommand;
        public ICommand PauseCommand
        {
            get
            {
                if (_pauseCommand == null)
                { _pauseCommand = new RelayCommand<object>(ExecutePauseCommand, CanExecutePauseCommand); }
                return _pauseCommand;
            }
        }

        public bool CanExecutePauseCommand(object notUsed)
        {
            return (PrintStatus == PrintStatus.Printing) ? true : false;
        }

        public void ExecutePauseCommand(object notUsed)
        {
            _serialCommunicationViewModel.SerialCommunicationMainModel.PausePrintSequence();
            PrintStatus = PrintStatus.PrintSequencePaused;
        }

        /// <summary>
        /// Resumes a paused print.
        /// </summary>
        private RelayCommand<object> _resumeCommand;
        public ICommand ResumeCommand
        {
            get
            {
                if (_resumeCommand == null)
                { _resumeCommand = new RelayCommand<object>(ExecuteResumeCommand, CanExecuteResumeCommand); }
                return _resumeCommand;
            }
        }

        public bool CanExecuteResumeCommand(object notUsed)
        {
            return ((PrintStatus == PrintStatus.MicrocontrollerPaused) || (PrintStatus == PrintStatus.PrintSequencePaused)) ? true : false;
        }

        public void ExecuteResumeCommand(object notUsed)
        {
            if (PrintStatus == PrintStatus.MicrocontrollerPaused)
            {
                _serialCommunicationViewModel.SerialCommunicationMainModel.ResumeMicrocontroller();
            }
            else if (PrintStatus == PrintStatus.PrintSequencePaused)
            {
                _serialCommunicationViewModel.SerialCommunicationMainModel.ResumePrintSequence();
            }

            PrintStatus = (_serialCommunicationOutgoingMessagesModel.ContainsMessages()) ? PrintStatus.Printing : PrintStatus.Manual;
        }

        /// <summary>
        /// Delete all entries in outgoing messages and wipe all memory in the microcontroller.
        /// </summary>
        private RelayCommand<bool> _abortCommand;
        public ICommand AbortCommand
        {
            get
            {
                if (_abortCommand == null)
                { _abortCommand = new RelayCommand<bool>(ExecuteAbortCommand, CanExecuteAbortCommand); }
                return _abortCommand;
            }
        }

        public bool CanExecuteAbortCommand(bool shouldAbort)
        {
            return (_serialCommunicationViewModel.SerialCommunicationMainModel.IsPortOpen) ? true : false;
        }

        public void ExecuteAbortCommand(bool shouldAbort)
        {
            if (shouldAbort == true)
            {
                _serialCommunicationViewModel.SerialCommunicationMainModel.SerialAbort();
            }
            _abortPrompt = false;
            OnPropertyChanged("AbortPrompt");

            PrintStatus = PrintStatus.Manual;

            OnPropertyChanged("IsModiPrintGCodeReadyToPrint");
            OnPropertyChanged("IsSerialCommunicationReadyToPrint");
            OnPropertyChanged("IsActivePrintheadReadyToPrint");
        }

        /// <summary>
        /// Switch menu to confirm abort.
        /// </summary>
        private RelayCommand<bool> _abortPromptCommand;
        public ICommand AbortPromptCommand
        {
            get
            {
                if (_abortPromptCommand == null)
                { _abortPromptCommand = new RelayCommand<bool>(ExecuteAbortPromptCommand, CanExecuteAbortPromptCommand); }
                return _abortPromptCommand;
            }
        }

        public bool CanExecuteAbortPromptCommand(bool shouldPrompt)
        {
            return (_serialCommunicationViewModel.SerialCommunicationMainModel.IsPortOpen) ? true : false;
        }

        public void ExecuteAbortPromptCommand(bool shouldPrompt)
        {
            _abortPrompt = shouldPrompt;
            OnPropertyChanged("AbortPrompt");
        }
        #endregion
    }
}