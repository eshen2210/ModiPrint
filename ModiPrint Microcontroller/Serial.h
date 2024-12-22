#ifndef SERIAL_H_
#define SERIAL_H_

#define BAUD 115200UL // See http://wormfood.net/avrbaudcalc.php for error rates associated with baud values.

#define SERIAL_RECEIVE_BUFFER_SIZE 64 //Maximum storage (in bytes) of the serial receive buffer.
#define SERIAL_TRANSMIT_BUFFER_SIZE 64 //Maximum storage (in bytes) of the serial transmit buffer.

#define SERIAL_TASK_QUEUED_CHAR '@' //Character at the beginning of outgoing serial message that signifies the message is a response to a command.
#define SERIAL_TASK_COMPLETED_CHAR '$' //Character in outgoing serial message that signifies the completion of a queued task.
#define SERIAL_ERROR_CHAR '^' //Character at the beginning of outgoing serial message that signifies an error message.
#define SERIAL_STATUS_CHAR '!' //Character at the beginning of outgoing serial message that signifies a nonresponse status message.
#define SERIAL_HARDWARE_PAUSE_CHAR '#' //Character in an incoming serial message for pausing of all hardware activities. 
#define SERIAL_HARDWARE_RESUME_CHAR '%' //Character in an incoming serial message for resuming all hardware activities. Does nothing unless preceded by the hardware pause character.
#define SERIAL_ABORT_CHAR '&' //Character in an incoming serial message for clearing all movements in the movement buffer. Typically called after an unexpected occurrence during print operations.

#define SERIAL_TERMINAL_CHAR ';' //Character that marks the end of a received and transmitted GCode.

#define SERIAL_TERMINAL_CYCLE 100UL //Number of serial cycles until program determines end of message. An arbitrary, empirically determined value.

typedef struct{
	
	//Ring buffer for the received serial data.
	volatile char ReceiveBuffer[SERIAL_RECEIVE_BUFFER_SIZE];
	volatile uint8_t ReceiveBufferHead; //Indexes the next unused element of ReceiveBuffer.
	volatile uint8_t ReceiveBufferTail; //Indexes the earliest unread element of the ReceiveBuffer.

	//Ring buffer for the transmitting serial data.
	volatile char TransmitBuffer[SERIAL_TRANSMIT_BUFFER_SIZE];
	volatile uint8_t TransmitBufferHead; //Indexes the next unused element of the TransmitBuffer.
	volatile uint8_t TransmitBufferTail; //Indexes the earliest unsent element of TransmitBuffer.

	//Asynchronous stepper operations will store transmit messages in this buffer.
	//The main thread will transfer this buffer to the main transmit buffer when it is free.
	//This exists to prevent asynchronous stepper operations and the main thread from writing simultaneously in the same transmit buffer.
	volatile char ISRTransmitBuffer[SERIAL_TRANSMIT_BUFFER_SIZE];
	volatile uint8_t ISRTransmitBufferHead; //Indexes the next unused element of the ISRTransmitBuffer.
	volatile uint8_t ISRTransmitBufferTail; //Indexes the earliest unsent element of ISRTransmitBuffer.

}serial_t;
serial_t Serial;

//Establish serial port connection with USART0.
void InitializeSerial();

//Set default values for serial buffer.
void IntializeSerialBuffer();

//Receive a message from the serial port receive buffer and stores it in incomingMessage.
void SerialReadString(char * receiveMessage);

//Output a message to the serial port transmit buffer.
//Be sure to end the message with the serial terminal character (see Serial.h for definition).
void SerialWriteString(char * transmitMessage);

//Returns true if read buffer is not empty.
uint8_t SerialReadAvailible();

//Returns true if the data register is empty.
uint8_t SerialWriteAvailible();

//Sends a message that signifies that a task has been completed.
void SerialWriteTaskCompleted();

//The purpose of the ISR Transmit Buffer is to prevent ISRs from overwriting the regular serial buffers during buffer operations.
//Messages are written into the ISR Transmit Buffers from ISRs.
//Messages are unloaded from ISR Transmit Buffers only from the main loop when the regular Transmit Buffer is not in operation.

//Same as SerialWriteString but for asynchronous ISR operations.
//Only call this function from within an ISR.
void SerialISRWriteString(char * transmitMessage);

//Returns false if there is an ISR message to transmit.
uint8_t SerialISRTransmitBufferEmpty();

//Loads the ISRTransmitBuffer into the TransmitBuffer and resets the ISRTransmitBuffer.
//Only call this function from the main loop.
void SerialLoadISRTransmitMessage();

//Same as SerialWriteTaskCompleted but for asynchronous ISR operations.
//Only call this function from within an ISR.
void SerialISRWriteTaskCompleted();

//Returns true if the character is the serial terminal terminal character or a null character.
uint8_t IsTerminalOrNull(char c);

#endif