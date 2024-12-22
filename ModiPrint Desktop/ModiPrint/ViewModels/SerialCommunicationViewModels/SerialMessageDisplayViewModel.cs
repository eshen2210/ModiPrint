using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
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

namespace ModiPrint.ViewModels.SerialCommunicationViewModels
{
    /// <summary>
    /// Manages the observable collections that contain queues of serial messages.
    /// </summary>
    public class SerialMessageDisplayViewModel : ViewModel
    {
        #region Fields and Properties
        //String that is meant to be bound to a display for outgoing serial messages.
        //This string is emptied as soon as it is retrieved and displayed.
        private ObservableCollection<string> _outgoingMessageList = new ObservableCollection<string>();
        public ObservableCollection<string> OutgoingMessageList
        {
            get { return _outgoingMessageList; }
        }

        //String that is meant to be bound to a display for incoming serial messages.
        //This string is emptied as soon as it is retrieved and displayed.
        private ObservableCollection<string> _incomingMessageList = new ObservableCollection<string>();
        public ObservableCollection<string> IncomingMessageList
        {
            get { return _incomingMessageList; }
        }
        #endregion

        #region Constructor
        public SerialMessageDisplayViewModel()
        {

        }
        #endregion

        #region Methods
        /// <summary>
        /// Updates the observable collection containing incoming messages.
        /// </summary>
        /// <param name="incomingMessage"></param>
        public void AppendIncomingMessage(string incomingMessage)
        {
            _incomingMessageList.Add(incomingMessage);
            //Keep the list to the 300 most recent messages.
            if (_incomingMessageList.Count > 300)
            {
                _incomingMessageList.RemoveAt(0);
            }
            OnPropertyChanged("IncomingMessageList");
        }

        /// <summary>
        /// Adds a message to the end of entry observable collection containing outgoing messages.
        /// This outgoing message would be sent after all previous messages have been sent.
        /// </summary>
        /// <param name="outgoingMessage"></param>
        public void AppendOutgoingMessage(string outgoingMessage)
        {
            string[] outgoingMessageArr = ParseMessage(outgoingMessage);

            foreach (string message in outgoingMessageArr)
            {
                if (!String.IsNullOrWhiteSpace(message))
                {
                    _outgoingMessageList.Add(message);
                    //Keep the list to the 300 most recent messages.
                    if (_outgoingMessageList.Count > 300)
                    {
                        _outgoingMessageList.RemoveAt(0);
                    }
                }
            }
            OnPropertyChanged("OutgoingMessageList");
        }

        /// <summary>
        /// Delimits a message by newlines.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private string[] ParseMessage(string message)
        {
            string[] messageArr = { "" };
            if (!String.IsNullOrWhiteSpace(message))
            {
                messageArr = message.Split(new string[] { "\r\n", "\n", ";" }, StringSplitOptions.None);
            }
            return messageArr;
        }
        #endregion
    }
}
