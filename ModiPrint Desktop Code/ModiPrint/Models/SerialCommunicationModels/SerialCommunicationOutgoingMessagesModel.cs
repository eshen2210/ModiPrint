using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModiPrint.DataTypes.GlobalValues;


namespace ModiPrint.Models.SerialCommunicationModels
{
    //Fires when a new message is added to the ProspectiveOutgoingMessagesList.
    public delegate void OutgoingMessageAddedEventHandler();
    
    /// <summary>
    /// States the type of message that can be found in the outgoing messages list.
    /// </summary>
    public enum MessageType
    {
        Unset, //Should never be the case
        Normal, //Send the command as is to the microcontroller.
        CommandSet, //Command set that needs to be interpreted into normal commands.
        PausePrintSequence, //Pause the print sequence. No message is sent through the microcontroller.
        ResumeMicrocontroller, //Resume print sequence. No message is sent through the microcontroller.
        PauseMicrocontroller, //Pause print sequence.
    }

    /// <summary>
    /// Manages the message that need to be sent through the serial port.
    /// </summary>
    public class SerialCommunicationOutgoingMessagesModel
    {
        #region Fields and Properties 
        //List of message that are about to be sent through the serial port.
        private List<string> _prospectiveOutgoingMessageList = new List<string>();
        #endregion

        #region Constructor
        public SerialCommunicationOutgoingMessagesModel()
        {

        }
        #endregion

        #region Events
        //Fires when a new message is added to the ProspectiveOutgoingMessagesList.
        public OutgoingMessageAddedEventHandler OutgoingMessageAdded;
        private void OnOutgoingMessageAdded()
        {
            if (OutgoingMessageAdded != null)
            { OutgoingMessageAdded(); }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds an entry to the end of ProspectiveOutgoingMessagesList.
        /// </summary>
        /// <param name="prospectiveOutgoingMessage"></param>
        public void AppendProspectiveOutgoingMessage(string prospectiveOutgoingMessage)
        {
            lock (_prospectiveOutgoingMessageList)
            {
                if (!String.IsNullOrWhiteSpace(prospectiveOutgoingMessage))
                { _prospectiveOutgoingMessageList.Add(prospectiveOutgoingMessage); }
            }
            OnOutgoingMessageAdded();
        }

        /// <summary>
        /// Adds a series of entries to the end of ProspectiveOutgoingMessagesList.
        /// </summary>
        /// <param name="prospectiveOutgoingMessages"></param>
        public void AppendProspectiveOutgoingMessage(List<string> prospectiveOutgoingMessages)
        {
            lock (_prospectiveOutgoingMessageList)
            {
                if ((prospectiveOutgoingMessages != null)
                 && (prospectiveOutgoingMessages.Count > 0))
                {
                    _prospectiveOutgoingMessageList.AddRange(prospectiveOutgoingMessages);
                }
            }
            OnOutgoingMessageAdded();
        }

        /// <summary>
        /// Adds an entry to the beginning of ProspectiveOutgoingMessagesList.
        /// </summary>
        /// <param name="prospectiveOutgoingMessage"></param>
        public void QueueNextProspectiveOutgoingMessage(string prospectiveOutgoingMessage)
        {
            lock (_prospectiveOutgoingMessageList)
            {
                if (!String.IsNullOrWhiteSpace(prospectiveOutgoingMessage))
                { _prospectiveOutgoingMessageList.Insert(0, prospectiveOutgoingMessage); }
            }
            OnOutgoingMessageAdded();
        }

        /// <summary>
        /// Adds a series of entries to the beginning of ProspectiveOutgoingMessagesList.
        /// </summary>
        /// <param name="prospectiveOutgoingMessageList"></param>
        public void QueueNextProspectiveOutgoingMessage(List<string> prospectiveOutgoingMessages)
        {
            lock (_prospectiveOutgoingMessageList)
            {
                if ((prospectiveOutgoingMessages != null)
                 && (prospectiveOutgoingMessages.Count > 0))
                {
                    //Inserts the outgoing messages in reverse order to the front of the list.
                    //This preserves the ordering of the outgoing messages while still shoving them all to the front.
                    for (int i = prospectiveOutgoingMessages.Count - 1; i >= 0; i--)
                    {
                        _prospectiveOutgoingMessageList.Insert(0, prospectiveOutgoingMessages[i]);
                    }
                }
            }
            OnOutgoingMessageAdded();
        }

        /// <summary>
        /// Returns the earliest entry of the ProspectiveOutgoingMessageList and then removes it from the list.
        /// This class should be locked before attempting to access this method.
        /// </summary>
        /// <returns></returns>
        public string RetrieveNextProspectiveOutgoingMessage()
        {
            lock (_prospectiveOutgoingMessageList)
            {
                string prospectiveOutgoingMessage = "";

                prospectiveOutgoingMessage = _prospectiveOutgoingMessageList[0];
                _prospectiveOutgoingMessageList.RemoveAt(0);

                return prospectiveOutgoingMessage;
            }
        }

        /// <summary>
        /// Returns true if there is a message to send from the PropsectiveOutgoingMessageList.
        /// </summary>
        /// <returns></returns>
        public bool ContainsMessages()
        {
            lock (_prospectiveOutgoingMessageList)
            {
                return (_prospectiveOutgoingMessageList.Count == 0) ? false : true;
            }
        }

        /// <summary>
        /// Returns the type of message in the zero index of the ProspectiveOutgoingMessageList.
        /// </summary>
        /// <returns></returns>
        public MessageType NextMessageType()
        {
            lock(_prospectiveOutgoingMessageList)
            {
                if ((_prospectiveOutgoingMessageList.Count > 0)
                 && (!String.IsNullOrWhiteSpace(_prospectiveOutgoingMessageList[0])))
                {
                    if (_prospectiveOutgoingMessageList[0][0] == SerialMessageCharacters.SerialCommandSetCharacter)
                    {
                        return MessageType.CommandSet;
                    }
                    else if (_prospectiveOutgoingMessageList[0][0] == SerialMessageCharacters.SerialPrintPauseCharacter)
                    {
                        return MessageType.PausePrintSequence;
                    }
                    else if (_prospectiveOutgoingMessageList[0][0] == SerialMessageCharacters.SerialResumeHardwareCharacter)
                    {
                        return MessageType.ResumeMicrocontroller;
                    }
                    else if (_prospectiveOutgoingMessageList[0][0] == SerialMessageCharacters.SerialPauseHardwareCharacter)
                    {
                        return MessageType.PauseMicrocontroller;
                    }
                    else
                    {
                        return MessageType.Normal;
                    }
                }

                //Reaches this point if this is a blank message.
                return MessageType.Normal;
            }
        }

        /// <summary>
        /// Removes the index zero of the ProspectiveOutgoingMessagesList.
        /// </summary>
        public void RemoveNextMessage()
        {
            lock (_prospectiveOutgoingMessageList)
            {
                _prospectiveOutgoingMessageList.RemoveAt(0);
            }
        }

        /// <summary>
        /// Remove the next messages that are pause print sequence characters.
        /// </summary>
        public void RemoveNextPausePrints()
        {
            lock (_prospectiveOutgoingMessageList)
            {
                //Remove the pause message.
                while (NextMessageType() == MessageType.PausePrintSequence)
                {
                    RemoveNextMessage();
                }
            }
        }

        /// <summary>
        /// Remove the next messages that are pause microcontroller characters.
        /// </summary>
        public void RemoveNextPauseMicrocontroller()
        {
            lock (_prospectiveOutgoingMessageList)
            {
                //Remove the pause message.
                while (NextMessageType() == MessageType.PauseMicrocontroller)
                {
                    RemoveNextMessage();
                }
            }
        }

        /// <summary>
        /// Clears the ProspectiveOutgoingMessageList.
        /// </summary>
        public void ClearProspectiveOutgoingMessages()
        {
            lock (_prospectiveOutgoingMessageList)
            {
                _prospectiveOutgoingMessageList.Clear();
            }
        }
        #endregion
    }
}
