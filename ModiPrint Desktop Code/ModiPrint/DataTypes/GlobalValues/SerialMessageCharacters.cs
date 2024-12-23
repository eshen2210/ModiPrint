using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModiPrint.DataTypes.GlobalValues
{
    public class SerialMessageCharacters
    {
        //Character at the beginning of incoming serial message that signifies the message is a response to a command.
        private static char _serialTaskQueuedCharacter = '@';
        public static char SerialTaskQueuedCharacter
        {
            get { return _serialTaskQueuedCharacter; }
        }

        //Character in incoming serial message that signifies the completion of a queued task.
        private static char _serialTaskCompletedCharacter = '$';
        public static char SerialTaskCompletedCharacter
        {
            get { return _serialTaskCompletedCharacter; }
        }

        //Character at the beginning of outgoing serial message that signifies an error message.
        private static char _serialErrorCharacter = '^';
        public static char SerialErrorCharacter
        {
            get { return _serialErrorCharacter; }
        }

        //Character at the beginning of incoming serial message that signifies a status message.
        private static char _serialStatusCharacter = '!';
        public static char SerialStatusCharacter
        {
            get { return _serialStatusCharacter; }
        }

        //Character that signifies that this message will be replaced by other commands.
        //Used when a set of commands need to depend on incoming status messages that come immediately before.
        private static char _serialCommandSetCharacter = '*';
        public static char SerialCommandSetCharacter
        {
            get { return _serialCommandSetCharacter; }
        }

        //Character in an outgoing serial message for pausing all hardware operations.
        //The current commands will finish execution.
        //Any continuous movements (sequence of movements with exit speeds greater than zero) will also complete before pausing comes into effect.
        private static char _serialPauseHardwareCharacter = '#';
        public static char SerialPauseHardwareCharacter
        {
            get { return _serialPauseHardwareCharacter; }
        }

        //Character in an outgoing serial message for resuming paused hardware operations.
        private static char _serialResumeHardwareCharacter = '%';
        public static char SerialResumeHardwareCharacter
        {
            get { return _serialResumeHardwareCharacter; }
        }

        //Character in an outgoing serial message for clearing all movements in the microcontroller's movement buffer. Typically called after an unexpected occurrence during print operations.
        private static char _serialMovementBufferClearCharacter = '&';
        public static char SerialMovementBufferClearCharacter
        {
            get { return _serialMovementBufferClearCharacter; }
        }

        //Character indicating that the serial communication protocol should stop sending messages past this one.
        //Messages inserted into the front of the outgoing queue can still be executed.
        private static char _serialPrintPauseCharacter = '?';
        public static char SerialPrintPauseCharacter
        {
            get { return _serialPrintPauseCharacter; }
        }

        //The terminal character for all outgoing and incoming messages.
        private static char _serialTerminalCharacter = ';';
        public static char SerialTerminalCharacter
        {
            get { return _serialTerminalCharacter; }
        }
    }
}
