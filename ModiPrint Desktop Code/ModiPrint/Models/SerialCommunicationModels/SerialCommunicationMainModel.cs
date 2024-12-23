using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Ports;
using ModiPrint.DataTypes.GlobalValues;
using ModiPrint.Models.PrinterModels;
using ModiPrint.Models.PrintModels;
using ModiPrint.Models.SerialCommunicationModels.ReportingModels;
using ModiPrint.Models.RealTimeStatusModels;
using ModiPrint.ViewModels;
using ModiPrint.ViewModels.PrintViewModels;

namespace ModiPrint.Models.SerialCommunicationModels
{
    //Handles event that is fired when serial connection is connected or disconnected.
    public delegate void SerialConnectionChangedEventHandler(object sender);

    //Handles event that is fired with the last Prospective Outgoing Message is sent.
    public delegate void SerialCommunicationCompletedEventHandler(object sender);

    //Event handler that's executed when a line of serial message is sent.
    public delegate void SerialCommunicationMessageSentEventHandler(object sender, SerialMessageEventArgs serialMessageEventArgs);

    //Event handler that's executed when a line of serial message is received.
    public delegate void SerialCommunicationMessageReceivedEventHandler(object sender, SerialMessageEventArgs serialMessageEventArgs);

    //Event handler that's executed when a print sequence pause command is read.
    public delegate void SerialCommunicationPrintSequencePausedEventHandler(object sender);

    //Event handler that's executed when a microcontroller resume command is sent.
    public delegate void SerialCommunicationMicrocontrollerResumedEventHandler(object sender);

    /// <summary>
    /// SerialConnectionModel manages the basic functionalities of System.IO.Ports.SerialPort.
    /// </summary>
    public class SerialCommunicationMainModel
    {
        #region Fields and Properties
        //Holds list of serial messages.
        private SerialCommunicationOutgoingMessagesModel _serialCommunicationOutgoingMessagesModel;

        //Contains functions to interpret and execute serial command sets.
        private SerialCommunicationCommandSetsModel _serialCommunicationCommandSetsModel;
        public SerialCommunicationCommandSetsModel SerialCommunicationCommandSetsModel
        {
            get { return _serialCommunicationCommandSetsModel; }
        }

        //Contains response information from the microcontroller.
        private RealTimeStatusDataModel _realTimeStatusDataModel;

        //SerialCommunication will pass errors to this object.
        private ErrorReporterViewModel _errorReporterViewModel;
        public ErrorReporterViewModel ErrorReporterViewModel
        {
            get { return _errorReporterViewModel; }
        }

        //Serial port where messages will be sent to and received from the microcontroller..
        private SerialPort _serialPort;

        public bool IsPortOpen
        {
            get { return _serialPort.IsOpen ? true : false; }
        }

        //BaudRate in bits per second.
        public int BaudRate
        {
            get { return _serialPort.BaudRate; }
        }

        //ReadTimeout in seconds.
        private int _readTimeout;
        public int ReadTimeout
        {
            get { return _readTimeout; }
            set { _readTimeout = value; }
        }

        //WriteTimeout in seconds.
        private int _writeTimeout;
        public int WriteTimeout
        {
            get { return _writeTimeout; }
            set { _writeTimeout = value; }
        }

        //Returns a string array of availible ports.
        public string[] PortNames
        {
            get { return SerialPort.GetPortNames(); }
        }

        //Flagged true when the serial communication protocol needs to be shut down.
        private bool _shouldDisconnect = false;
        public bool ShouldDisconnect
        {
            set { _shouldDisconnect = value; }
        }

        //Flagged true when the microcontroller is ready to receive another message.
        //Toggled when a task queued message is received or when a new message is sent.
        private bool _shouldSend = true;

        //Used to lock different threads from reading and writing simultaneously.
        private object _readLock = new object();
        private object _writeLock = new object();

        //For some reason, every time a serial connection is established, the microcontroller thinks it's receiving an incorrect command.
        //So this is just some really hacky piece of logic that prevents that error from being recorded.
        private bool _shouldProcessNextError = true;
        #endregion

        #region Events
        //Event that is fired when serial connection is connected or disconnected.
        public event SerialConnectionChangedEventHandler SerialConnectionChanged;
        private void OnSerialConnectionChanged()
        {
            if (SerialConnectionChanged != null)
            { SerialConnectionChanged(this); }
        }

        //Event that is fired with the last Prospective Outgoing Message is sent.
        public event SerialCommunicationCompletedEventHandler SerialCommunicationCompleted;
        private void OnSerialCommunicationCompleted()
        {
            if (SerialCommunicationCompleted != null)
            { SerialCommunicationCompleted(this); }
        }

        //Event that fires when serial comms sends a message.
        public event SerialCommunicationMessageSentEventHandler SerialCommunicationMessageSent;
        private void OnSerialCommunicationMessageSent(SerialMessageEventArgs serialMessageEventArgs)
        {
            if (SerialCommunicationMessageSent != null)
            { SerialCommunicationMessageSent(this, serialMessageEventArgs); }
        }

        //Event that fires after serial comms receives a message.
        public event SerialCommunicationMessageReceivedEventHandler SerialCommunicationMessageReceived;
        private void OnSerialCommunicationMessageReceived(SerialMessageEventArgs serialMessageEventArgs)
        {
            if (SerialCommunicationMessageReceived != null)
            { SerialCommunicationMessageReceived(this, serialMessageEventArgs); }
        }
        
        //Event that's executed when a print sequence pause command is read.
        public event SerialCommunicationPrintSequencePausedEventHandler SerialCommunicationPrintSequencePaused;
        private void OnSerialCommunicationPrintSequencePaused()
        {
            if (SerialCommunicationPrintSequencePaused != null)
            { SerialCommunicationPrintSequencePaused(this); }
        }

        //Event that's executed when a microcontroller resume command is sent.
        public event SerialCommunicationMicrocontrollerResumedEventHandler SerialCommunicationMicrocontrollerResumed;
        private void OnSerialCommunicationMicrocontrollerResumed()
        {
            if (SerialCommunicationMicrocontrollerResumed != null)
            { SerialCommunicationMicrocontrollerResumed(this); }
        }
        #endregion

        #region Constructors
        public SerialCommunicationMainModel(SerialCommunicationOutgoingMessagesModel SerialCommunicationOutgoingMessagesModel,
            PrinterModel PrinterModel, PrintModel PrintModel,
            RealTimeStatusDataModel RealTimeStatusDataModel, ErrorListViewModel ErrorListViewModel)
        {
            _serialCommunicationOutgoingMessagesModel = SerialCommunicationOutgoingMessagesModel;
            _realTimeStatusDataModel = RealTimeStatusDataModel;
            _errorReporterViewModel = new ErrorReporterViewModel(ErrorListViewModel);

            _serialCommunicationCommandSetsModel = new SerialCommunicationCommandSetsModel(_realTimeStatusDataModel, this, PrinterModel, PrintModel, ErrorListViewModel);

            InitializeSerialPort();

            _serialCommunicationOutgoingMessagesModel.OutgoingMessageAdded += new OutgoingMessageAddedEventHandler(SendNextMessage);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Initializes and sets parameters for the serial port.
        /// </summary>
        private void InitializeSerialPort()
        {
            _serialPort = new SerialPort();
            _serialPort.BaudRate = GlobalValues.BaudRate;
            _serialPort.Parity = Parity.None;
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;
            _serialPort.NewLine = SerialMessageCharacters.SerialTerminalCharacter.ToString();
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(ReceiveMessage); //Subscribe to DataReceived event handler.
        }

        /// <summary>
        /// Send the next message through the serial port.
        /// </summary>
        private void SendNextMessage()
        {
            try
            {
                MethodStart:

                //If there is a new message that needs to be sent...
                if ((_serialCommunicationOutgoingMessagesModel.ContainsMessages())
                    && (_serialCommunicationOutgoingMessagesModel.NextMessageType() != MessageType.PausePrintSequence)
                    && ((_realTimeStatusDataModel.PrintStatus != PrintStatus.MicrocontrollerPaused) || (_serialCommunicationOutgoingMessagesModel.NextMessageType() == MessageType.ResumeMicrocontroller))
                    && ((_serialCommunicationOutgoingMessagesModel.NextMessageType() != MessageType.CommandSet) || (_serialCommunicationCommandSetsModel.CanInterpret == true))
                    && (_shouldSend == true))
                {
                    switch (_serialCommunicationOutgoingMessagesModel.NextMessageType())
                    {
                        case MessageType.Normal:
                            if (SendMessage(_serialCommunicationOutgoingMessagesModel.RetrieveNextProspectiveOutgoingMessage()) == false)
                            { goto MethodStart; }
                            break;
                        case MessageType.CommandSet:
                            //Retrieve the command set from the outgoing messages list and parse the command set.
                            string commandSetString;
                            commandSetString = _serialCommunicationOutgoingMessagesModel.RetrieveNextProspectiveOutgoingMessage();
                            List<string> commandSet = _serialCommunicationCommandSetsModel.InterpretCommandSet(commandSetString);

                            //Fill the outgoing messages list with the parsed command set.
                            if (commandSet != null)
                            {
                                _serialCommunicationOutgoingMessagesModel.QueueNextProspectiveOutgoingMessage(commandSet);
                            }

                            goto MethodStart;
                        case MessageType.PausePrintSequence:
                            OnSerialCommunicationPrintSequencePaused();
                            break;
                        case MessageType.PauseMicrocontroller:
                            SendMessage(_serialCommunicationOutgoingMessagesModel.RetrieveNextProspectiveOutgoingMessage());
                            break;
                        case MessageType.ResumeMicrocontroller:
                            SendMessage(_serialCommunicationOutgoingMessagesModel.RetrieveNextProspectiveOutgoingMessage());
                            _serialCommunicationOutgoingMessagesModel.RemoveNextPauseMicrocontroller(); //To Do: Dunno why this line is necessary but it is. Somehow an extra pause hardware char gets generated during pausing.
                            OnSerialCommunicationMicrocontrollerResumed();
                            goto MethodStart;
                        default:
                            //Should not reach this point.
                            break;
                    }
                }
                else
                {
                    if (_serialCommunicationOutgoingMessagesModel.NextMessageType() == MessageType.PausePrintSequence)
                    {
                        //Mainly here to update the UI when a PausePrintSequence command set is interpreted.
                        OnSerialCommunicationPrintSequencePaused();
                    }
                }
            }
            catch (Exception exception)
            {
                HandleSerialException(exception);
            }
        }

        /// <summary>
        /// Pause print sequence.
        /// </summary>
        /// <returns></returns>
        public void PausePrintSequence()
        {
            _serialCommunicationOutgoingMessagesModel.QueueNextProspectiveOutgoingMessage(SerialMessageCharacters.SerialPrintPauseCharacter.ToString());
            OnSerialCommunicationPrintSequencePaused();
        }

        /// <summary>
        /// Resume print sequence.
        /// Should only be called if a print sequence is paused.
        /// </summary>
        public void ResumePrintSequence()
        {
            //Remove the pause messages.
            _serialCommunicationOutgoingMessagesModel.RemoveNextPausePrints();

            if (_serialCommunicationOutgoingMessagesModel.ContainsMessages())
            {
                SendNextMessage();
            }
        }

        /// <summary>
        /// Sends a pause command to the microcontroller.
        /// </summary>
        public void PauseMicrocontroller()
        {
            try
            {
                //Send a message to the microcontroller to stop all hardware operations.
                _serialCommunicationOutgoingMessagesModel.QueueNextProspectiveOutgoingMessage(SerialMessageCharacters.SerialPauseHardwareCharacter.ToString());
                SendNextMessage();
            }
            catch (Exception exception)
            {
                HandleSerialException(exception);
            }
        }

        /// <summary>
        /// Sends a resume command to the microcontroller.
        /// </summary>
        public void ResumeMicrocontroller()
        {
            try
            {
                //Send a message to the microcontroller to resume all hardware operations.
                _serialCommunicationOutgoingMessagesModel.QueueNextProspectiveOutgoingMessage(SerialMessageCharacters.SerialResumeHardwareCharacter.ToString());
                _shouldSend = true;
                SendNextMessage();
            }
            catch (Exception exception)
            {
                HandleSerialException(exception);
            }
        }

        /// <summary>
        /// Delete all entries in outgoing messages and wipe all memory in the microcontroller.
        /// </summary>
        public void SerialAbort()
        {
            _serialCommunicationOutgoingMessagesModel.ClearProspectiveOutgoingMessages();
            _realTimeStatusDataModel.Abort();

            _shouldSend = true;

            //Send a message to the microcontroller to resume all hardware operations.
            _serialCommunicationOutgoingMessagesModel.QueueNextProspectiveOutgoingMessage(SerialMessageCharacters.SerialMovementBufferClearCharacter.ToString());
            SendNextMessage();
        }

        /// <summary>
        /// Triggered when a serial message is received.
        /// Designates the appropriate event handler for the type of serial message received.
        /// </summary>
        /// <returns></returns>
        private void ReceiveMessage(object sender, SerialDataReceivedEventArgs e)
        {
            lock (_readLock)
            {
                try
                {
                    while (_serialPort.BytesToRead != 0)
                    {
                        //Reads a message coming from the serial port.
                        string incomingMessage = _serialPort.ReadLine();

                        if (!String.IsNullOrWhiteSpace(incomingMessage))
                        {
                            //If the micrcontroller has interpreted the previous command, then go ahead and send it new commands.
                            if (incomingMessage[0] == SerialMessageCharacters.SerialTaskQueuedCharacter)
                            {
                                _shouldSend = true;
                            }

                            if (incomingMessage[0] == SerialMessageCharacters.SerialErrorCharacter)
                            {
                                _shouldSend = true;
                                Application.Current.Dispatcher.Invoke(() =>
                                OnSerialCommunicationPrintSequencePaused()); //Pause a print sequence if an error was read.
                            }

                            //Update the serial messages UI.
                            if (incomingMessage[0] != SerialMessageCharacters.SerialErrorCharacter)
                            {
                                //Triggers the appropriate message received event.
                                SerialMessageEventArgs serialMessageEventArgs = new SerialMessageEventArgs(incomingMessage);

                                Application.Current.Dispatcher.Invoke(() =>
                                OnSerialCommunicationMessageReceived(serialMessageEventArgs));
                            }
                            else if ((incomingMessage[0] == SerialMessageCharacters.SerialErrorCharacter) && (_shouldProcessNextError == true))
                            {
                                //Pause the print sequence because of unexpected error.
                                _serialCommunicationOutgoingMessagesModel.QueueNextProspectiveOutgoingMessage(SerialMessageCharacters.SerialPrintPauseCharacter.ToString());

                                //Triggers the appropriate message received event.
                                SerialMessageEventArgs serialMessageEventArgs = new SerialMessageEventArgs(incomingMessage);

                                Application.Current.Dispatcher.Invoke(() =>
                                OnSerialCommunicationMessageReceived(serialMessageEventArgs));
                            }
                            else if ((incomingMessage[0] == SerialMessageCharacters.SerialErrorCharacter) && (_shouldProcessNextError == false))
                            {
                                _shouldProcessNextError = true;
                            }
                        }
                    }

                    if (_shouldSend == true)
                    {
                        SendNextMessage();
                    }

                    //Notify subscribers if all messages were sent and executed by the microcontroller.
                    if ((_serialCommunicationOutgoingMessagesModel.ContainsMessages() == false) 
                     && (_realTimeStatusDataModel.TaskQueuedMessagesListContainsMessages() == false))
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        OnSerialCommunicationCompleted());
                    }
                }
                catch (Exception exception)
                {
                    HandleSerialException(exception);
                }
            }
        }

        /// <summary>
        /// Sends an outgoing message through the serial port.
        /// Returns true if a message has been sent.
        /// </summary>
        /// <param name="outgoingMessage"></param>
        private bool SendMessage(string outgoingMessage)
        {
            lock (_writeLock)
            {
                //Remove all comments (characters after the terminal character) from the outgoing message).
                if (!String.IsNullOrWhiteSpace(outgoingMessage))
                {
                    outgoingMessage = outgoingMessage.Trim();

                    //If message is not a comment...
                    if (!(outgoingMessage[0] == SerialMessageCharacters.SerialTerminalCharacter))
                    {
                        //Remove comments from the message.
                        int terminalIndex = outgoingMessage.IndexOf(SerialMessageCharacters.SerialTerminalCharacter);
                        if (terminalIndex > 0)
                        {
                            outgoingMessage = outgoingMessage.Substring(0, terminalIndex + 1);
                        }

                        //Wait until the microcontroller inteprets the sent message before sending a new message.
                        //Pause and resume messages do not need a reply.
                        if (!((outgoingMessage[0] == SerialMessageCharacters.SerialPauseHardwareCharacter)
                           || (outgoingMessage[0] == SerialMessageCharacters.SerialResumeHardwareCharacter)
                           || (outgoingMessage[0] == SerialMessageCharacters.SerialMovementBufferClearCharacter)))
                        {
                            _shouldSend = false;
                        }
                        else
                        {
                            _shouldSend = true;
                        }

                        //Sends a message through the serial ports.
                        _serialPort.WriteLine(outgoingMessage);

                        //Triggers the message sent event.
                        SerialMessageEventArgs serialMessageEventArgs = new SerialMessageEventArgs(outgoingMessage);
                        Application.Current.Dispatcher.Invoke(() =>
                        OnSerialCommunicationMessageSent(serialMessageEventArgs));

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Attempt a connection to a serial port.
        /// Begins the serial protocol after connecting.
        /// </summary>
        public void SerialConnect(string portName)
        {
            //Disconnect if a port is already open.
            if (_serialPort.IsOpen == true)
            {
                SerialDisconnect();
            }
            
            //Set port name.
            _serialPort.PortName = portName;

            //Connect to port.
            if (_serialPort.IsOpen == false)
            {
                ClearBuffer();
                _shouldProcessNextError = false;
                _serialPort.Open();
                OnSerialConnectionChanged();

                //Wait for the Serial Port on both ends to be fully open.
                Thread.Sleep(1000);

                lock (_writeLock)
                {
                    //Handles the continual sending and receiving of messages.
                    //To Do: Disconnect if return message is not correct.
                    try
                    {
                        //Check that the device connected contains the appropriate functions.
                        string connectionCheckMessage = SerialCommands.CheckConnection + " Connected";
                        _serialCommunicationOutgoingMessagesModel.AppendProspectiveOutgoingMessage(connectionCheckMessage);
                    }
                    catch (Exception exception)
                    {
                        HandleSerialException(exception);
                    }
                }
            }
        }

        /// <summary>
        /// Disconnect from a serial port and wipes the buffer.
        /// </summary>
        public void SerialDisconnect()
        {
            ClearBuffer();
            _serialPort.Close();
            _serialCommunicationOutgoingMessagesModel.ClearProspectiveOutgoingMessages();
            OnSerialConnectionChanged();
        }

        /// <summary>
        /// Wipes the in and out serial port buffer.
        /// </summary>
        public void ClearBuffer()
        {
            if(_serialPort.IsOpen == true)
            {
                _serialPort.DiscardInBuffer();
                _serialPort.DiscardOutBuffer();
            }
        }

        /// <summary>
        /// Exception handling for serial communication methods.
        /// </summary>
        /// <param name="exception"></param>
        public void HandleSerialException(Exception exception)
        {
            if (_serialPort.IsOpen == false)
            {
                //Should never reach this point.
                _errorReporterViewModel.ReportError("Serial Communications Failed: Serial Port Is Not Open", "Ensure That The Device Is Securely Connected");
                SerialDisconnect();
            }
            else if (exception.InnerException is TimeoutException)
            {
                _errorReporterViewModel.ReportError("Serial Communication Failed: Serial Port Timeout", "Ensure That The Device Is Securely Connected");
                SerialDisconnect();
            }
            else
            {
                _errorReporterViewModel.ReportError("Serial Communication Failed: Unspecified Error, Contact The Developer To Report This Error", exception.Message);
                SerialDisconnect();
            }
        }
        #endregion
    }
}
