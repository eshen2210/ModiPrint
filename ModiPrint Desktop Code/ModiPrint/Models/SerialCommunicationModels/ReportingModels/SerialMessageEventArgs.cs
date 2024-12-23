using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModiPrint.Models.SerialCommunicationModels.ReportingModels
{
    /// <summary>
    /// SerialCommunication will pass this class as an argument when triggering events that update serial messages sending and received to the GUI.
    /// </summary>
    public class SerialMessageEventArgs : EventArgs
    {
        //The message sending or received by SerialCommunication.
        private string _message;
        public string Message
        {
            get { return _message; }
        }

        public SerialMessageEventArgs(string Message)
        {
            _message = Message;
        }
    }
}
