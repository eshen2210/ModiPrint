using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ModiPrint.DataTypes.GlobalValues;
using ModiPrint.Models.SerialCommunicationModels;
using ModiPrint.Models.SerialCommunicationModels.ReportingModels;
using ModiPrint.Models.PrinterModels;
using ModiPrint.Models.PrinterModels.AxisModels;
using ModiPrint.Models.PrinterModels.PrintheadModels;
using ModiPrint.Models.PrinterModels.PrintheadModels.PrintheadTypeModels;
using ModiPrint.Models.RealTimeStatusModels.RealTimeStatusAxisModels;
using ModiPrint.Models.RealTimeStatusModels.RealTimeStatusPrintheadModels;
using ModiPrint.ViewModels;
using ModiPrint.ViewModels.PrinterViewModels;
using ModiPrint.ViewModels.PrinterViewModels.AxisViewModels;
using ModiPrint.ViewModels.PrinterViewModels.PrintheadViewModels;

namespace ModiPrint.Models.RealTimeStatusModels
{
    /// <summary>
    /// Interprets incoming serial messages into data regarding the printer during operation.
    /// </summary>
    public class RealTimeStatusSerialInterpreterModel
    {
        #region Fields and Properties
        //Contains events that pass incoming messages to this class.
        private SerialCommunicationMainModel _serialCommunicationMainModel;

        //Contains information on the Printer.
        private PrinterModel _printerModel;
        private PrinterViewModel _printerViewModel;

        //Information is stored in this class after serial messages are interpreted.
        private RealTimeStatusDataModel _realTimeStatusDataModel;

        //Reports errors to the GUI.
        private ErrorListViewModel _errorListViewModel;
        #endregion

        #region Constructor
        public RealTimeStatusSerialInterpreterModel(SerialCommunicationMainModel SerialCommunicationMainModel, PrinterModel PrinterModel, PrinterViewModel PrinterViewModel, 
            RealTimeStatusDataModel RealTimeStatusDataModel, ErrorListViewModel ErrorListViewModel)
        {
            _serialCommunicationMainModel = SerialCommunicationMainModel;
            _printerModel = PrinterModel;
            _printerViewModel = PrinterViewModel;
            _realTimeStatusDataModel = RealTimeStatusDataModel;
            _errorListViewModel = ErrorListViewModel;

            _serialCommunicationMainModel.SerialCommunicationMessageReceived += new SerialCommunicationMessageReceivedEventHandler(SerialCommunicationMessageReceived);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Executes when a message has been received from serial communication.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="serialMessageEventArgs"></param>
        private void SerialCommunicationMessageReceived(object sender, SerialMessageEventArgs serialMessageEventArgs)
        {
            string incomingMessage = serialMessageEventArgs.Message;

            //Filter message by type.
            if (incomingMessage[0] == SerialMessageCharacters.SerialTaskQueuedCharacter) //Task queued.
            {
                InterpretTaskQueuedMessage(incomingMessage);
            }
            else if (incomingMessage[0] == SerialMessageCharacters.SerialTaskCompletedCharacter) //Task completed.
             {
                //Earliest queued tasks are completed first.
                if (_realTimeStatusDataModel.IsTaskQueuedEmpty() == false)
                {
                    InterpretTaskCompletedMessage(_realTimeStatusDataModel.RetrieveNextTaskQueuedMessage());
                    _realTimeStatusDataModel.RecordTaskCompleted();
                }
                else
                {
                    _errorListViewModel.AddError("Serial communication out of sync: ", "Task completed return does not correspond to a task");
                }
            }
            else if (incomingMessage[0] == SerialMessageCharacters.SerialStatusCharacter) //Status.
            {
                InterpretStatusMessage(incomingMessage);
                _realTimeStatusDataModel.RecordStatusMessage(incomingMessage.Substring(1));
            }
            else if (incomingMessage[0] == SerialMessageCharacters.SerialErrorCharacter) //Error.
            {
                _realTimeStatusDataModel.RecordErrorMessage(incomingMessage.Substring(1));
            }
            else //Also an error.
            {
                _realTimeStatusDataModel.RecordErrorMessage("Microcontroller return message unrecognized: " + incomingMessage);
            }
        }

        /// <summary>
        /// Translates raw task queued serial messages into a readable phrase.
        /// Sets the Real Time Status based on the message.
        /// </summary>
        /// <param name="incomingMessage"></param>
        private void InterpretTaskQueuedMessage(string incomingMessage)
        {
            //First letter of the incoming message should be '@', which signifies a task queued message.
            if ((incomingMessage.Length >= 11)
             && ((incomingMessage.Substring(1, 3) == "Set") && (incomingMessage.Substring(5, 4) == "Axs("))) //Set Axis.
            {
                InterpretSetAxisQueued(incomingMessage);
            }
            else if ((incomingMessage.Length >= 10)
                  && (incomingMessage.Substring(1, 9) == "SetMtrPh(")) //Set Motor Printhead.
            {
                InterpretSetMotorizedPrintheadQueued(incomingMessage);
            }
            else if ((incomingMessage.Length >= 10)
                  && (incomingMessage.Substring(1, 9) == "SetVlvPh(")) //Set Valve Printhead.
            {
                InterpretSetValvePrintheadQueued(incomingMessage);
            }
            else if ((incomingMessage.Length >= 4)
                  && (incomingMessage.Substring(1, 3) == "Mv(")) //Axes Movement.
            {
                InterpretAxesMovementQueued(incomingMessage);
            }
            else if ((incomingMessage.Length >= 10)
                  && (incomingMessage.Substring(1, 9) == "MvPrnMtr(")) //Motorized Print With Movement.
            {
                InterpretMotorizedPrintWithMovementQueued(incomingMessage);
            }
            else if ((incomingMessage.Length >= 10)
                  && (incomingMessage.Substring(1, 9) == "MvPrnVlv(")) //Valve Print With Movement.
            {
                InterpretValvePrintWithMovementQueued(incomingMessage);
            }
            else if ((incomingMessage.Length >= 8)
                  && (incomingMessage.Substring(1, 7) == "PrnMtr(")) //Motorized Printhead Print without Movement.
            {
                InterpretMotorizedPrintWithoutMovementQueued(incomingMessage);
            }
            else if ((incomingMessage.Length >= 7)
                  && (incomingMessage.Substring(1, 6) == "PrnVlv")) //Valve Printhead Print without Movement.
            {
                InterpretValvePrintWithoutMovementQueued(incomingMessage);
            }
            else if ((incomingMessage.Length >= 7)
                  && (incomingMessage.Substring(1, 6) == "StpVlv")) //Valve Close.
            {
                InterpretValveCloseQueued(incomingMessage);
            }
        }

        /// <summary>
        /// Translates task queued messages when task is complete.
        /// Sets parameters for the Real Time Status.
        /// </summary>
        /// <param name="taskQueuedMessage"></param>
        private void InterpretTaskCompletedMessage(string taskQueuedMessage)
        {
            if ((taskQueuedMessage.Length >= 11)
             && ((taskQueuedMessage.Substring(0, 4) == "Set ") && (taskQueuedMessage.Substring(6, 5) == "Axis:")))
            {
                InterpretSetAxisComplete(taskQueuedMessage);
            }
            else if ((taskQueuedMessage.Length >= 24)
                  && (taskQueuedMessage.Substring(0, 24) == "Set Motorized Printhead:"))
            {
                InterpretSetMotorizedPrintheadComplete(taskQueuedMessage);
            }
            else if ((taskQueuedMessage.Length >= 20)
                  && (taskQueuedMessage.Substring(0, 20) == "Set Valve Printhead:"))
            {
                InterpretSetValvePrintheadComplete(taskQueuedMessage);
            }
            else if ((taskQueuedMessage.Length >= 5)
                  && (taskQueuedMessage.Substring(0, 5) == "Move:"))
            {
                InterpretAxesMovementComplete(taskQueuedMessage);
            }
            else if ((taskQueuedMessage.Length >= 30)
                  && (taskQueuedMessage.Substring(0, 30) == "Motorized Print With Movement:"))
            {
                InterpretMotorizedPrintWithMovementComplete(taskQueuedMessage);
            }
            else if ((taskQueuedMessage.Length >= 26)
                  && (taskQueuedMessage.Substring(0, 26) == "Valve Print With Movement:"))
            {
                InterpretValvePrintWithMovementComplete(taskQueuedMessage);
            }
            else if ((taskQueuedMessage.Length >= 33)
                  && (taskQueuedMessage.Substring(0, 33) == "Motorized Print Without Movement:"))
            {
                InterpretMotorizedPrintWithoutMovementComplete(taskQueuedMessage);
            }
            else if ((taskQueuedMessage.Length >= 29)
                  && (taskQueuedMessage.Substring(0, 29) == "Valve Print Without Movement:"))
            {
                InterpretValvePrintWithoutMovementComplete(taskQueuedMessage);
            }
            else if ((taskQueuedMessage.Length >= 11)
                  && (taskQueuedMessage.Substring(0, 11) == "Valve Close"))
            {
                InterpretValveCloseComplete(taskQueuedMessage);
            }
        }

        /// <summary>
        /// Translsates status messages when received.
        /// Sets parameters for the Real Time Status.
        /// </summary>
        /// <param name="statusMessage"></param>
        private void InterpretStatusMessage(string statusMessage)
        {
            //First letter of the incoming message should be '!', which signifies a status message.
            if ((statusMessage.Length >= 4)
             && (statusMessage.Substring(1, 3) == "Lmt"))
            {
                InterpretLimit(statusMessage);

                //Remove the movement command that was never completed because a Limit Switch was hit.
                _realTimeStatusDataModel.RemoveNextTaskQueuedMessage();
            }
        }

        /// <summary>
        /// Translates raw task queued message for an Axes Movement command into a readable phrase.
        /// Sets the Real Time Status based on the message.
        /// </summary>
        /// <param name="incomingMessage"></param>
        public void InterpretAxesMovementQueued(string incomingMessage)
        {
            string taskQueuedMessage = "Move:"; //Return string.
            double[] parameterValues = ParseDoubleArray(incomingMessage); //Array of numerical values in incomingMessage.

            int xStepsTaken = (int)parameterValues[0];
            int yStepsTaken = (int)parameterValues[1];
            int zStepsTaken = (int)parameterValues[2];

            taskQueuedMessage += " X" + xStepsTaken.ToString(); //X
            taskQueuedMessage += " Y" + yStepsTaken.ToString(); //Y
            taskQueuedMessage += " Z" + zStepsTaken.ToString(); //Z

            _realTimeStatusDataModel.RecordTaskQueued(taskQueuedMessage);
        }

        /// <summary>
        /// Translates raw task queued message for an Axes Movement command into a readable phrase.
        /// Sets the Real Time Status based on the message.
        /// </summary>
        /// <param name="taskQueuedMessage"></param>
        public void InterpretAxesMovementComplete(string taskQueuedMessage)
        {
            double[] parameterValues = ParseDoubleArray(taskQueuedMessage); //Array of numerical values in incomingMessage.

            int xStepsTaken = (int)parameterValues[0];
            int yStepsTaken = (int)parameterValues[1];
            int zStepsTaken = (int)parameterValues[2];

            _realTimeStatusDataModel.RecordMoveAxes(xStepsTaken, yStepsTaken, zStepsTaken);
        }

        /// <summary>
        /// Translates raw task queued message for a Motorized Printhead Print With Movement command into a readable phrase.
        /// Sets the Real Time Status based on the message.
        /// </summary>
        /// <param name="incomingMessage"></param>
        public void InterpretMotorizedPrintWithMovementQueued(string incomingMessage)
        {
            string taskQueuedMessage = "Motorized Print With Movement:"; //Return string.
            double[] parameterValues = ParseDoubleArray(incomingMessage); //Array of numerical values in incomingMessage.

            int eStepsTaken = (int)parameterValues[0];
            int xStepsTaken = (int)parameterValues[1];
            int yStepsTaken = (int)parameterValues[2];
            int zStepsTaken = (int)parameterValues[3];

            taskQueuedMessage += " E" + eStepsTaken.ToString(); //E
            taskQueuedMessage += " X" + xStepsTaken.ToString(); //X
            taskQueuedMessage += " Y" + yStepsTaken.ToString(); //Y
            taskQueuedMessage += " Z" + zStepsTaken.ToString(); //Z

            _realTimeStatusDataModel.RecordTaskQueued(taskQueuedMessage);
        }

        /// <summary>
        /// Translates task queued message when a Motorized Print With Movement task is complete.
        /// Sets parameters for the Real Time Status.
        /// </summary>
        /// <param name="taskQueuedMessage"></param>
        public void InterpretMotorizedPrintWithMovementComplete(string taskQueuedMessage)
        {
            double[] parameterValues = ParseDoubleArray(taskQueuedMessage); //Array of numerical values in incomingMessage.

            int eStepsTaken = (int)parameterValues[0];
            int xStepsTaken = (int)parameterValues[1];
            int yStepsTaken = (int)parameterValues[2];
            int zStepsTaken = (int)parameterValues[3];

            _realTimeStatusDataModel.RecordMotorizedPrintWithMovement(eStepsTaken, xStepsTaken, yStepsTaken, zStepsTaken);
        }

        /// <summary>
        /// Translates raw task queued message for a Motorized Printhead Print without Movement command into a readable phrase.
        /// Sets the Real Time Status based on the message.
        /// </summary>
        /// <param name="incomingMessage"></param>
        public void InterpretMotorizedPrintWithoutMovementQueued(string incomingMessage)
        {
            string taskQueuedMessage = "Motorized Print Without Movement:"; //Return string.
            double[] parameterValues = ParseDoubleArray(incomingMessage); //Array of numerical values in incomingMessage.

            int eStepsTaken = (int)parameterValues[0];

            taskQueuedMessage += " E" + eStepsTaken.ToString(); //E

            _realTimeStatusDataModel.RecordTaskQueued(taskQueuedMessage);
        }

        /// <summary>
        /// Translates task queued message when a Motorized Print Without Movement task is complete.
        /// Sets parameters for the Real Time Status.
        /// </summary>
        /// <param name="taskQueuedMessage"></param>
        public void InterpretMotorizedPrintWithoutMovementComplete(string taskQueuedMessage)
        {
            double[] parameterValues = ParseDoubleArray(taskQueuedMessage); //Array of numerical values in incomingMessage.

            int eStepsTaken = (int)parameterValues[0];

            _realTimeStatusDataModel.RecordMotorizedPrintWithoutMovement(eStepsTaken);
        }

        /// <summary>
        /// Translates raw task queued message for a Set Axis command into a readable phrase.
        /// Sets the Real Time Status' Task Queued List based on the message.
        /// </summary>
        /// <param name="incomingMessage"></param>
        public void InterpretSetAxisQueued(string incomingMessage)
        {
            string taskQueuedMessage = "Set "; //Return string.
            char axisID = 'A'; //X, Y, or Z. A is just a temporary placeholder.
            string axisName = "Unknown Axis"; //Name of the Axis. Labelled as unknown if Axis cannot be found.
            double[] parameterValues = ParseDoubleArray(incomingMessage); //Array of numerical values in incomingMessage.

            int stepPin = (int)parameterValues[0];
            int maxSpeed = (int)parameterValues[4];
            int acceleration = (int)parameterValues[5];

            switch (incomingMessage.Substring(4, 1)) //Axis Name.
            {
                case "X":
                    axisID = 'X';
                    axisName = _printerModel.AxisModelList[0].Name;
                    taskQueuedMessage += "X Axis: ";
                    break;
                case "Y":
                    axisID = 'Y';
                    axisName = _printerModel.AxisModelList[1].Name;
                    taskQueuedMessage += "Y Axis: ";
                    break;
                case "Z":
                    axisID = 'Z';
                    //Search for the Z Axis with the matching Step Pin ID then return the Axis Name.
                    foreach (AxisModel zAxis in _printerModel.ZAxisModelList)
                    {
                        if (stepPin == zAxis.AttachedMotorStepGPIOPinModel.PinID)
                        {
                            axisName = zAxis.Name;
                            break;
                        }
                    }
                    taskQueuedMessage += "Z Axis: ";
                    break;
                default:
                    //If Axis was not found.
                    //Default Name is "Unknown Axis".
                    break;
            }

            taskQueuedMessage += axisName;
            taskQueuedMessage += " S" + maxSpeed;
            taskQueuedMessage += " A" + acceleration;

            //Set Real Time Status parameters based on this incomingMessage.
            if ((axisID != 'A')
             && (axisName != "Unknown Axis"))
            {
                _realTimeStatusDataModel.RecordTaskQueued(taskQueuedMessage);
                _printerViewModel.FindAxis(axisName).AxisStatus = AxisStatus.BeingSet;
            }
        }

        /// <summary>
        /// Translates task queued message when a Set Axis task is complete.
        /// Sets parameters for the Real Time Status.
        /// </summary>
        /// <param name="taskQueuedMessage"></param>
        public void InterpretSetAxisComplete(string taskQueuedMessage)
        {
            char axisID = 'A'; //X, Y, or Z. A is just a temporary placeholder.
            string axisName = taskQueuedMessage.Substring(12, taskQueuedMessage.IndexOf(" S") - 12); //Name of the Axis. Labelled as unknown if Axis cannot be found.
            double[] parameterValues = ParseDoubleArray(taskQueuedMessage.Substring(taskQueuedMessage.IndexOf(" S"))); //Array of numerical values in incomingMessage.

            axisName = (_printerModel.FindAxis(axisName) != null) ? axisName : "Unknown Axis";
            int maxSpeed = (int)parameterValues[0];
            int acceleration = (int)parameterValues[1];

            //The old Axis is used to revert the active status of the old Axis.
            AxisViewModel oldAxisViewModel = null;

            switch (taskQueuedMessage.Substring(4, 1)) //Axis Name.
            {
                case "X":
                    axisID = 'X';
                    break;
                case "Y":
                    axisID = 'Y';
                    break;
                case "Z":
                    axisID = 'Z';
                    oldAxisViewModel = _printerViewModel.FindAxis(_realTimeStatusDataModel.ZRealTimeStatusAxisModel.Name);
                    break;
                default:
                    //If Axis was not found.
                    axisName = "Unknown Axis";
                    break;
            }

            //Set Real Time Status parameters based on this incomingMessage.
            if ((axisID != 'A')
             && (axisName != "Unknown Axis"))
            {
                if (oldAxisViewModel != null)
                { oldAxisViewModel.AxisStatus = AxisStatus.Idle; }
                _realTimeStatusDataModel.RecordSetAxis(axisID, axisName, maxSpeed, acceleration);
                _printerViewModel.FindAxis(axisName).AxisStatus = AxisStatus.Active;
            }
        }

        /// <summary>
        /// Translates raw task queued message for a Set Motorized Printhead command into a readable phrase.
        /// Sets the Real Time Status based on the message.
        /// </summary>
        /// <param name="incomingMessage"></param>
        /// <returns></returns>
        public void InterpretSetMotorizedPrintheadQueued(string incomingMessage)
        {
            string taskQueuedMessage = "Set Motorized Printhead: "; //Return string.
            string printheadName = "Unknown Motorized Printhead"; //Name of the Printhead. Labelled as unknown if Axis cannot be found.
            double[] parameterValues = ParseDoubleArray(incomingMessage); //Array of numerical values in incomingMessage.

            int stepPin = (int)parameterValues[0];
            int maxSpeed = (int)parameterValues[4];
            int acceleration = (int)parameterValues[5];

            //Search for the PrintheadModel with the matching Step Pin ID then return the Printhead Name.
            foreach (PrintheadModel printheadModel in _printerModel.PrintheadModelList)
            {
                if (printheadModel.PrintheadType == PrintheadType.Motorized)
                {
                    MotorizedPrintheadTypeModel motorizedPrintheadTypeModel = (MotorizedPrintheadTypeModel)printheadModel.PrintheadTypeModel;
                    if ((int)stepPin == motorizedPrintheadTypeModel.AttachedMotorStepGPIOPinModel.PinID)
                    {
                        printheadName = printheadModel.Name;
                        break;
                    }
                }
            }

            taskQueuedMessage += printheadName;
            taskQueuedMessage += " S" + maxSpeed;
            taskQueuedMessage += " A" + acceleration;

            //Set Real Time Status parameters based on this incomingMessage.
            if (printheadName != "Unknown Motorized Printhead")
            {
                _realTimeStatusDataModel.RecordTaskQueued(taskQueuedMessage);
                _printerViewModel.FindPrinthead(printheadName).PrintheadStatus = PrintheadStatus.BeingSet;
            }
        }

        /// <summary>
        /// Translates task queued message when a Set Motorized Printhead task is complete.
        /// Sets parameters for the Real Time Status.
        /// </summary>
        /// <param name="taskQueuedMessage"></param>
        /// <returns></returns>
        public void InterpretSetMotorizedPrintheadComplete(string taskQueuedMessage)
        {
            string printheadName = taskQueuedMessage.Substring(25, taskQueuedMessage.IndexOf(" S") - 25); // Name of the Printhead. Labelled as unknown if Printhead cannot be found.
            double[] parameterValues = ParseDoubleArray(taskQueuedMessage.Substring(taskQueuedMessage.IndexOf(" S"))); //Array of numerical values in incomingMessage.

            printheadName = (_printerModel.FindPrinthead(printheadName) != null) ? printheadName : "Unknown Motorized Printhead";
            int maxSpeed = (int)parameterValues[0];
            int acceleration = (int)parameterValues[1];

            string oldPrintheadName = _realTimeStatusDataModel.ActivePrintheadModel.Name; //The old Printhead Name is used to revert the active status of the old Printhead.

            //Set Real Time Status parameters based on this incomingMessage.
            if (printheadName != "Unknown Motorized Printhead")
            {
                PrintheadViewModel oldPrintheadViewModel = _printerViewModel.FindPrinthead(oldPrintheadName);
                if (oldPrintheadViewModel != null)
                { oldPrintheadViewModel.PrintheadStatus = PrintheadStatus.Idle; }

                _realTimeStatusDataModel.RecordSetMotorizedPrinthead(printheadName, maxSpeed, acceleration);
                _printerViewModel.FindPrinthead(printheadName).PrintheadStatus = PrintheadStatus.Active;
            }
        }

        /// <summary>
        /// Translates raw task queued message for a Set Valve Printhead command into a readable phrase.
        /// Sets the Real Time Status based on the message.
        /// </summary>
        /// <param name="incomingMessage"></param>
        /// <returns></returns>
        public void InterpretSetValvePrintheadQueued(string incomingMessage)
        {
            string taskQueuedMessage = "Set Valve Printhead: "; //Return string.
            string printheadName = "Unknown Valve Printhead"; //Name of the Printhead. Labelled as unknown if Axis cannot be found.
            double[] parameterValues = ParseDoubleArray(incomingMessage); //Array of numerical values in incomingMessage.

            int valvePinID = (int)parameterValues[0];

            //Search for the PrintheadModel with the matching Valve Pin ID then return the Printhead Name.
            foreach (PrintheadModel printheadModel in _printerModel.PrintheadModelList)
            {
                if (printheadModel.PrintheadType == PrintheadType.Valve)
                {
                    ValvePrintheadTypeModel valvePrintheadTypeModel = (ValvePrintheadTypeModel)printheadModel.PrintheadTypeModel;
                    if (valvePinID == valvePrintheadTypeModel.AttachedValveGPIOPinModel.PinID)
                    {
                        printheadName = printheadModel.Name;
                        break;
                    }
                }
            }

            taskQueuedMessage += printheadName;

            //Set Real Time Status parameters based on this incomingMessage.
            if (printheadName != "Unknown Valve Printhead")
            {
                _realTimeStatusDataModel.RecordTaskQueued(taskQueuedMessage);
                _printerViewModel.FindPrinthead(printheadName).PrintheadStatus = PrintheadStatus.BeingSet;
            }
        }

        /// <summary>
        /// Translates task queued message when a Set Valve Printhead task is complete.
        /// Sets parameters for the Real Time Status.
        /// </summary>
        /// <param name="taskQueuedMessage"></param>
        /// <returns></returns>
        public void InterpretSetValvePrintheadComplete(string taskQueuedMessage)
        {
            string printheadName = taskQueuedMessage.Substring(21); //Name of the Printhead. Labelled as unknown if Axis cannot be found.
            double[] parameterValues = ParseDoubleArray(taskQueuedMessage); //Array of numerical values in incomingMessage.

            printheadName = (_printerModel.FindPrinthead(printheadName) != null) ? printheadName : "Unknown Valve Printhead";

            string oldPrintheadName = _realTimeStatusDataModel.ActivePrintheadModel.Name; //The old Printhead Name is used to revert the active status of the old Printhead.

            //Set Real Time Status parameters based on this incomingMessage.
            if (printheadName != "Unknown Valve Printhead")
            {
                PrintheadViewModel oldPrintheadViewModel = _printerViewModel.FindPrinthead(oldPrintheadName);
                if (oldPrintheadViewModel != null)
                { oldPrintheadViewModel.PrintheadStatus = PrintheadStatus.Idle; }

                _realTimeStatusDataModel.RecordSetValvePrinthead(printheadName);
                _printerViewModel.FindPrinthead(printheadName).PrintheadStatus = PrintheadStatus.Active;
            }
        }

        /// <summary>
        /// Translates raw task queued message for a Valve Printhead Close command into a readable phrase.
        /// Sets the Real Time Status based on the message.
        /// </summary>
        /// <param name="incomingMessage"></param>
        public void InterpretValveCloseQueued(string incomingMessage)
        {
            string taskQueuedMessage = "Valve Close"; //Return string.

            //Not much to really record or check here.

            _realTimeStatusDataModel.RecordTaskQueued(taskQueuedMessage);
        }

        /// <summary>
        /// Translates raw task queued message for a Valve Printhead Close command into a readable phrase.
        /// Sets the Real Time Status based on the message.
        /// </summary>
        /// <param name="taskQueuedMessage"></param>
        public void InterpretValveCloseComplete(string taskQueuedMessage)
        {
            double[] parameterValues = ParseDoubleArray(taskQueuedMessage); //Array of numerical values in incomingMessage.

            //Not much to really record or check here.

            _realTimeStatusDataModel.RecordValveClose();
        }

        /// <summary>
        /// Translates raw task queued message for a Valve Printhead Print With Movement command into a readable phrase.
        /// Sets the Real Time Status based on the message.
        /// </summary>
        /// <param name="incomingMessage"></param>
        public void InterpretValvePrintWithMovementQueued(string incomingMessage)
        {
            string taskQueuedMessage = "Valve Print With Movement:"; //Return string.
            double[] parameterValues = ParseDoubleArray(incomingMessage); //Array of numerical values in incomingMessage.
            RealTimeStatusValvePrintheadModel realTimeStatusValvePrintheadModel = (RealTimeStatusValvePrintheadModel)_realTimeStatusDataModel.ActivePrintheadModel;

            int xStepsTaken = (int)parameterValues[0];
            int yStepsTaken = (int)parameterValues[1];
            int zStepsTaken = (int)parameterValues[2];

            bool isXYZMove = ((xStepsTaken != 0) || (yStepsTaken != 0) || (zStepsTaken != 0)) ? true : false;
            if (isXYZMove == true)
            {
                realTimeStatusValvePrintheadModel.IsValveOn = true;
            }

            taskQueuedMessage += " X" + xStepsTaken.ToString(); //X
            taskQueuedMessage += " Y" + yStepsTaken.ToString(); //Y
            taskQueuedMessage += " Z" + zStepsTaken.ToString(); //Z

            _realTimeStatusDataModel.RecordTaskQueued(taskQueuedMessage);
        }

        /// <summary>
        /// Translates task queued message when a Valve Print With Movement task is complete.
        /// Sets parameters for the Real Time Status.
        /// </summary>
        /// <param name="taskQueuedMessage"></param>
        public void InterpretValvePrintWithMovementComplete(string taskQueuedMessage)
        {
            double[] parameterValues = ParseDoubleArray(taskQueuedMessage); //Array of numerical values in incomingMessage.

            int xStepsTaken = (int)parameterValues[0];
            int yStepsTaken = (int)parameterValues[1];
            int zStepsTaken = (int)parameterValues[2];

            _realTimeStatusDataModel.RecordValvePrintWithMovement(xStepsTaken, yStepsTaken, zStepsTaken);
        }

        /// <summary>
        /// Translates raw task queued message for a Valve Printhead Print without Movement command into a readable phrase.
        /// Sets the Real Time Status based on the message.
        /// </summary>
        /// <param name="incomingMessage"></param>
        public void InterpretValvePrintWithoutMovementQueued(string incomingMessage)
        {
            string taskQueuedMessage = "Valve Print Without Movement:"; //Return string.
            double[] parameterValues = ParseDoubleArray(incomingMessage); //Array of numerical values in incomingMessage.
            RealTimeStatusValvePrintheadModel realTimeStatusValvePrintheadModel = (RealTimeStatusValvePrintheadModel)_realTimeStatusDataModel.ActivePrintheadModel;

            int openTime = (parameterValues.Length > 0) ? (int)parameterValues[0] : 0;

            if (openTime > 0) //Valve is opening for a set period of time.
            {
                realTimeStatusValvePrintheadModel.IsValveOn = true;

                taskQueuedMessage += " " + openTime.ToString() + " us";
            }
            else if (openTime == 0) //Valve is about to open until further commands.
            {
                taskQueuedMessage += " Open Valve";
            }

            _realTimeStatusDataModel.RecordTaskQueued(taskQueuedMessage);
        }

        /// <summary>
        /// Translates task queued message when a Valve Print Without Movement task is complete.
        /// Sets parameters for the Real Time Status.
        /// </summary>
        /// <param name="taskQueuedMessage"></param>
        public void InterpretValvePrintWithoutMovementComplete(string taskQueuedMessage)
        {
            double[] parameterValues = ParseDoubleArray(taskQueuedMessage); //Array of numerical values in incomingMessage.

            int openTime = (parameterValues.Length > 0) ? (int)parameterValues[0] : 0;

            _realTimeStatusDataModel.RecordValvePrintWithoutMovement(openTime);
        }

        /// <summary>
        /// Translates raw status message for a Limit Switch status into a readable phrase.
        /// Sets the Real Time Status' Task Queued List based on the message.        
        /// </summary>
        /// <param name="statusMessage"></param>
        public void InterpretLimit(string statusMessage)
        {
            double[] parameterValues = ParseDoubleArray(statusMessage); //Array of numerical values in statusMessage.

            bool eLimit = (statusMessage.Contains("E")) ? true : false;
            bool xLimit = (statusMessage.Contains("X")) ? true : false;
            bool yLimit = (statusMessage.Contains("Y")) ? true : false;
            bool zLimit = (statusMessage.Contains("Z")) ? true : false;

            int eStepsTaken = (int)parameterValues[0];
            int xStepsTaken = (int)parameterValues[1];
            int yStepsTaken = (int)parameterValues[2];
            int zStepsTaken = (int)parameterValues[3];

            _realTimeStatusDataModel.RecordLimit(xLimit, yLimit, zLimit, eLimit, xStepsTaken, yStepsTaken, zStepsTaken, eStepsTaken);
        }

        /// <summary>
        /// Takes a string and returns only the first number within the string.
        /// Supports negative/positive, integer/decimal numbers.
        /// </summary>
        private double[] ParseDoubleArray(string incomingMessage)
        {
            //Parse numbers from incomingMessage.
            MatchCollection regexMatches = Regex.Matches(incomingMessage, @"-?\d+");
            double[] doubleArray = new double[regexMatches.Count];

            int index = 0;
            foreach (Match match in regexMatches)
            {
                doubleArray[index] = Convert.ToDouble(match.Value);
                index++;
            }

            return doubleArray;
        }
        #endregion
    }
}
