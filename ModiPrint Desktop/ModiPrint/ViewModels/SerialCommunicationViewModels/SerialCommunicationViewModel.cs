using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using ModiPrint.DataTypes.GlobalValues;
using ModiPrint.Models.RealTimeStatusModels;
using ModiPrint.Models.SerialCommunicationModels;
using ModiPrint.Models.SerialCommunicationModels.ReportingModels;

namespace ModiPrint.ViewModels.SerialCommunicationViewModels
{
    /// <summary>
    /// SerialCommunicationViewModel links the SerialCommunicationView to the SerialCommunicationMainModel
    /// </summary>
    public class SerialCommunicationViewModel : ViewModel
    {
        #region Fields and Properties
        //Provides functions related to serial communication.
        private SerialCommunicationMainModel _serialCommunicationMainModel;
        public SerialCommunicationMainModel SerialCommunicationMainModel
        {
            get { return _serialCommunicationMainModel; }
        }
        private SerialCommunicationOutgoingMessagesModel _serialCommunicationOutgoingMessagesModel;

        //Manages the queue of outgoing and incoming serial messages.
        private SerialMessageDisplayViewModel _serialMessageDisplayViewModel;

        //The message that the user can manually send to the microcontroller.
        private string _manualSerialSendMessage;
        public string ManualSerialSendMessage
        {
            get { return _manualSerialSendMessage; }
            set
            {
                _manualSerialSendMessage = value;
                OnPropertyChanged("ManualSerialSendMessage");
            }
        }

        //Get array of all availible serial ports
        public string[] PortNames
        {
            get { return _serialCommunicationMainModel.PortNames; }
        }

        //Is the Serial Port open?
        public bool IsPortOpen
        {
            get { return _serialCommunicationMainModel.IsPortOpen; }
        }
        #endregion

        #region Constructors
        public SerialCommunicationViewModel(SerialCommunicationMainModel SerialCommunicationMainModel, 
            SerialCommunicationOutgoingMessagesModel SerialCommunicationOutgoingMessagesModel, 
            SerialMessageDisplayViewModel SerialMessageDisplayModel)
        {
            _serialCommunicationMainModel = SerialCommunicationMainModel;
            _serialCommunicationOutgoingMessagesModel = SerialCommunicationOutgoingMessagesModel;
            _serialMessageDisplayViewModel = SerialMessageDisplayModel;

            _serialCommunicationMainModel.SerialCommunicationMessageSent += new SerialCommunicationMessageSentEventHandler(SerialCommunicationMessageSent);
            _serialCommunicationMainModel.SerialCommunicationMessageReceived += new SerialCommunicationMessageReceivedEventHandler(SerialCommunicationMessageReceived);
            _serialCommunicationMainModel.SerialConnectionChanged += new SerialConnectionChangedEventHandler(UpdateSerialConnectionStatus);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Executes with the opening or closing of the serial port.
        /// </summary>
        /// <param name="sender"></param>
        private void UpdateSerialConnectionStatus(object sender)
        {
            OnPropertyChanged("IsPortOpen");
        }

        /// <summary>
        /// Executes with the termination of the serial communication background worker thread.
        /// Updates the GUI with the closing of the serial port.
        /// </summary>
        /// <param name="sender"></param>
        private void SerialCommunicationBGWTerminated(object sender)
        {
            _serialDisconnectCommand.RaiseCanExecuteChanged();
            _serialConnectCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Executes whenever a s erial message is sent.
        /// Updates Serial Messages Display.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialCommunicationMessageSent(object sender, SerialMessageEventArgs e)
        {
            string outgoingMessage = e.Message;

            _serialMessageDisplayViewModel.AppendOutgoingMessage(outgoingMessage);
        }

        /// <summary>
        /// Executes whenever a serial message is received.
        /// Updates Serial Messages Display.
        /// </summary>
        private void SerialCommunicationMessageReceived(object sender, SerialMessageEventArgs e)
        {
            string incomingMessage = e.Message;

            _serialMessageDisplayViewModel.AppendIncomingMessage(incomingMessage);
        }
        #endregion

        #region Commands
        /// <summary>
        /// Refreshes PortNames.
        /// </summary>
        private RelayCommand<object> _refreshPortNamesCommand;
        public ICommand RefreshPortNamesCommand
        {
            get
            {
                if (_refreshPortNamesCommand == null)
                { _refreshPortNamesCommand = new RelayCommand<object>(ExecuteRefreshPortNamesCommand, CanExecuteRefreshPortNamesCommand); }
                return _refreshPortNamesCommand;
            }
        }

        public bool CanExecuteRefreshPortNamesCommand(object notUsed)
        {
            return true;
        }

        public void ExecuteRefreshPortNamesCommand(object notUsed)
        {
            OnPropertyChanged("PortNames");
        }

        /// <summary>
        /// On another thread, connects to a serial port then handles sending and receiving serial data.
        /// </summary>
        private RelayCommand<string> _serialConnectCommand;
        public ICommand SerialConnectCommand
        {
            get
            {
                if (_serialConnectCommand == null)
                { _serialConnectCommand = new RelayCommand<string>(ExecuteSerialConnectCommand, CanSerialConnectCommand); }
                return _serialConnectCommand;
            }
        }

        public bool CanSerialConnectCommand(string portName)
        {
            return true;
        }

        public void ExecuteSerialConnectCommand(string portName)
        {
            if (!String.IsNullOrWhiteSpace(portName))
            {
                //Begins serial communications.
                _serialCommunicationMainModel.SerialConnect(portName);
            }
            else
            {
                _serialCommunicationMainModel.ErrorReporterViewModel.ReportError("Serial Communication Failed: Could Not Connect", "Ensure The Device Is Securely Connected");
            }
            OnPropertyChanged("IsPortOpen");
            _serialConnectCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Disconnects the serial port.
        /// </summary>
        private RelayCommand<object> _serialDisconnectCommand;
        public ICommand SerialDisconnectCommand
        {
            get
            {
                if (_serialDisconnectCommand == null)
                { _serialDisconnectCommand = new RelayCommand<object>(ExecuteSerialDisconnectCommand, CanExecuteSerialDisconnectCommand); }
                return _serialDisconnectCommand;
            }
        }

        public bool CanExecuteSerialDisconnectCommand(object notUsed)
        {
            return IsPortOpen == true ? true : false;
        }

        public void ExecuteSerialDisconnectCommand(object notUsed)
        {
            _serialCommunicationMainModel.SerialDisconnect();
            OnPropertyChanged("IsPortOpen");
        }

        /// <summary>
        /// Queues text from the associated textbox to be sent through the serial port.
        /// </summary>
        private RelayCommand<string> _manualAppendMessageCommand;
        public ICommand ManualAppendMessageCommand
        {
            get
            {
                if (_manualAppendMessageCommand == null)
                { _manualAppendMessageCommand = new RelayCommand<string>(ExecuteManualAppendMessageCommand, CanExecuteManualAppendMessageCommand); }
                return _manualAppendMessageCommand;
            }
        }

        public bool CanExecuteManualAppendMessageCommand(string outgoingMessage)
        {
            if (!String.IsNullOrWhiteSpace(outgoingMessage)
            && (IsPortOpen == true))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ExecuteManualAppendMessageCommand(string outgoingMessage)
        {
            _serialCommunicationOutgoingMessagesModel.QueueNextProspectiveOutgoingMessage(outgoingMessage);
            ManualSerialSendMessage = "";
        }
        #endregion
    }
}
