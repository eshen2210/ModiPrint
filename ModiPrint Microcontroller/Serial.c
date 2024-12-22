#include "ModiPrintMicro.h"

//Upon receiving data in serial communication, an interrupt will store data in a ring buffer which will be read on a function call.
//When trying to transmit data in serial communication, the program will store data in a ring buffer which will be transmitted via interrupts.
//For more information on ring buffers, see: http://www.simplyembedded.org/tutorials/interrupt-free-ring-buffer/.
//Note: Each buffer will contain one fewer usable element than the defined buffer size (to distinguish between empty and full buffers).
//Tail == head means the buffer is empty. Head + 1 == tail means the buffer is full.

void InitializeSerial()
{
	//Set baud rate.
	UCSR0A |= (1 << U2X0);  //Enable baud doubler.
	uint16_t UBRR0Value = ((F_CPU / (4L * BAUD)) - 1) / 2; //For UBRR values see: http://wormfood.net/avrbaudcalc.php.
	UBRR0H = UBRR0Value >> 8; //8 most significant bits of value.
	UBRR0L = UBRR0Value; //Least significant bits of value.

	//Serial settings.
	UCSR0C &= ~(1 << UMSEL00) & ~(1 << UMSEL01); //Asynchronous USART mode.
	UCSR0C |= (1 << UCSZ01) | (1 << UCSZ00); //Communicate with 8-bit data.
	UCSR0C &= ~(1 << UPM00) & ~(1 << UPM01); //No parity.
	UCSR0C |= (1 << USBS0); //1 stop bit.

	//Flush serial buffers.
	UCSR0B &= ~(1 << RXEN0) & ~(1 << TXEN0);

	//Enable serial communication.
	UCSR0B |= (1 << RXEN0) | (1 << TXEN0); //Enable RX (receive) and TX (transmit) pins.

	//Begin serial receive stream.
	UCSR0B &= ~(1 << UDRIE0); //Disable USART0 Transmit Data Register Empty interrupt for transmitting (USART0_TX_vect).
	UCSR0B |= (1 << RXCIE0); //Enable USART0 Receive Complete interrupt for receiving (USART0_RX_vect).

	//Set receive and transmit buffers for serial communication.
	IntializeSerialBuffer();
}

void IntializeSerialBuffer()
{
	Serial.ReceiveBufferHead = 0;
	Serial.ReceiveBufferTail = 0;
	Serial.TransmitBufferHead = 0;
	Serial.TransmitBufferTail = 0;
	Serial.ISRTransmitBufferHead = 0;
	Serial.ISRTransmitBufferTail = 0;
}

void SerialReadString(char * receiveMessage)
{
	uint8_t tail = Serial.ReceiveBufferTail;
	
	if (tail != Serial.ReceiveBufferHead) //If buffer is not empty...
	{
		//Stores ReceiveBuffer contents into receiveMessage.
		for (uint8_t i = 0; i < SERIAL_RECEIVE_BUFFER_SIZE; i++)
		{
			//Stores an element of ReceiveBuffer into the corresponding element of incomingMessage.
			receiveMessage[i] = Serial.ReceiveBuffer[tail];

			//Update tail.
			tail++;
			if (tail == SERIAL_RECEIVE_BUFFER_SIZE) { tail = 0; } //Loops back to the beginning of the ring if it is the last element.

			//Stop storing elements at the end of the message.
			if (IsTerminalOrNull(receiveMessage[i]) == TRUE) { break; }

			//Wait until the receive stream has caught up with this function.
			for (float waitCounter = 0; tail == Serial.ReceiveBufferHead; waitCounter++) //While the buffer is not full and the message is incomplete.
			{
				//This function has waited long enough and determined that the message has ended.
				if (waitCounter > (F_CPU / BAUD * SERIAL_TERMINAL_CYCLE)) //Semi-arbitrary equation to determine if the serial messenger has waited long enough.
				{
					receiveMessage[i + 1] = SERIAL_TERMINAL_CHAR; //Terminate receiveMessage.
					
					//End 2 levels of loops.
					goto EndOfSerialReadLoop;
				}
			}
		}

		EndOfSerialReadLoop:
		Serial.ReceiveBufferTail = tail;
	}
}

//USART0 Rx Complete interrupt handler.
//Triggered when USART0 has received a byte.
ISR(USART0_RX_vect)
{
	uint8_t nextHead = Serial.ReceiveBufferHead + 1;
	if (nextHead == SERIAL_RECEIVE_BUFFER_SIZE) { nextHead = 0; } //Loops back to the beginning of the ring if it is the last element.

	//Stores Rx data only if the ReceiveBuffer has room.
	if (nextHead != Serial.ReceiveBufferTail)
	{
		//Pause/Resume/Clear Buffer if applicable.
		char newChar = UDR0;
		switch (newChar)
		{
			case SERIAL_HARDWARE_PAUSE_CHAR:
				System.Pause = TRUE;
				break;
			case SERIAL_HARDWARE_RESUME_CHAR:
				System.Pause = FALSE;
				break;
			case SERIAL_ABORT_CHAR:
				System.Abort = TRUE;
				break;
			default:
				//Stores USART0 Data Register into ReceiveBuffer.
				Serial.ReceiveBuffer[Serial.ReceiveBufferHead] = newChar;
				//Update head.
				Serial.ReceiveBufferHead = nextHead;
				break;
		}
	}
}

void SerialWriteString(char * transmitMessage)
{
	uint8_t head = Serial.TransmitBufferHead;

	//Do not transmit if the message is empty.
	if (transmitMessage[0] != NULL_CHAR)
	{
		//Store transmitMessage into TransmitBuffer.
		uint8_t i;
		for (i = 0; i < SERIAL_TRANSMIT_BUFFER_SIZE; i++)
		{
			//Stop populating TransmitBuffer if it is full.
			uint8_t nextHead = head + 1;
			if (nextHead == SERIAL_TRANSMIT_BUFFER_SIZE) { nextHead = 0; }  //Loops back to the beginning of the ring if it is the last element.
			
			//Wait until the transmit buffer is no longer full.
			while (nextHead == Serial.TransmitBufferTail) { }
			
			//Stores an element of outgoingMessage into the corresponding element of TransmitBuffer.
			Serial.TransmitBuffer[head] = transmitMessage[i];

			//Update head.
			head = nextHead;

			//Stop populating TransmitBuffer at the end of the message.
			if (IsTerminalOrNull(transmitMessage[i]) == TRUE) { break; }
		}

		Serial.TransmitBufferHead = head;
		
		if (Serial.TransmitBufferTail != Serial.TransmitBufferHead) //If there is data to transmit...
		{
			//Begin serial transmit stream.
			UCSR0B |= (1 << UDRIE0); //Enable USART0 Data Register Empty Interrupt (USART0_UDRE_vect).
		}
	}
}

void SerialWriteTaskCompleted()
{
	char taskCompletedMessage[2] = { SERIAL_TASK_COMPLETED_CHAR, NULL_CHAR };
	SerialWriteString(taskCompletedMessage);
}

//USART0 Data Register Empty interrupt handler.
//Triggered when the USART0 transmit buffer is cleared and ready to receive data.
ISR(USART0_UDRE_vect)
{
	uint8_t tail = Serial.TransmitBufferTail;
	
	//Send a byte from the TransmitBuffer.
	if (IsTerminalOrNull(Serial.TransmitBuffer[tail]) == FALSE)
	{
		UDR0 = Serial.TransmitBuffer[tail];
	}
	else //End the serial transmit stream if all of the transmit message has been sent.
	{
		//Finalize the message by ending with the terminal char.
		UDR0 = SERIAL_TERMINAL_CHAR;
	}

	//Update tail.
	tail++;
	if (tail == SERIAL_TRANSMIT_BUFFER_SIZE) { tail = 0; } //Loops back to the beginning of the ring if it is the last element.

	//Ends the serial transmit stream if all messages were transmitted.
	if (tail == Serial.TransmitBufferHead)
	{
		UCSR0B &= ~(1 << UDRIE0); //Disables the interrupt handled by this handler.
	}

	Serial.TransmitBufferTail = tail;
}

uint8_t SerialReadAvailible()
{
	return (Serial.ReceiveBufferHead != Serial.ReceiveBufferTail) ? TRUE : FALSE;
}

uint8_t SerialWriteAvailible()
{
	return ((UCSR0A & (1 << UDRE0)) == (1 << UDRE0)) ? TRUE : FALSE;
}

void SerialISRWriteString(char * transmitMessage)
{
	uint8_t head = Serial.ISRTransmitBufferHead;
		
	//Store transmitMessage into ISRTransmitBuffer.
	for (uint8_t i = 0; i < SERIAL_TRANSMIT_BUFFER_SIZE; i++)
	{
		//Stores an element of outgoingMessage into the corresponding element of the ISR TransmitBuffer.
		//Note that there is no check to see if the ISRTransmitBuffer is full. Since these operations occur within ISRs, there is no way to wait for buffers to empty.
		//In the case that ISRTransmitBuffer is full, there will be data loss.
		Serial.ISRTransmitBuffer[head] = transmitMessage[i];

		//Update head.
		head++;
		if (head == SERIAL_TRANSMIT_BUFFER_SIZE) { head = 0; }

		//Stop populating ISRTransmitBuffer at the end of the message.
		if (IsTerminalOrNull(transmitMessage[i]) == TRUE) { break; }
	}

	//Update head so that the message accounts for the terminal character.
	head++;
	if (head == SERIAL_TRANSMIT_BUFFER_SIZE) { head = 0; }

	//Update head.
	Serial.ISRTransmitBufferHead = head;
}

uint8_t SerialISRTransmitBufferEmpty()
{
	return (Serial.ISRTransmitBufferHead == Serial.ISRTransmitBufferTail) ? TRUE : FALSE;
}

void SerialISRWriteTaskCompleted()
{
	char taskCompletedMessage[2] = { SERIAL_TASK_COMPLETED_CHAR, NULL_CHAR };
	SerialISRWriteString(taskCompletedMessage);
}

void SerialLoadISRTransmitMessage()
{	
	char transmitMessage[SERIAL_TRANSMIT_BUFFER_SIZE] = { NULL_CHAR };
	
	//Remembers the tail and head as ISRs can overwrite these values.
	uint8_t tail = Serial.ISRTransmitBufferTail;
	uint8_t head = Serial.ISRTransmitBufferHead;

	while (tail != head)
	{
		//Load the ISR Transmit Buffer into transmitMessage.
		for (uint8_t i = 0; tail != head; i++)
		{
			transmitMessage[i] = Serial.ISRTransmitBuffer[tail];

			if (IsTerminalOrNull(Serial.ISRTransmitBuffer[tail]) == TRUE)
			{
				//Update tail.
				tail++;
				if (tail == SERIAL_TRANSMIT_BUFFER_SIZE) { tail = 0; }  //Loops back to the beginning of the ring if it is the last element.
				
				SerialWriteString(transmitMessage);
				memset(transmitMessage, NULL_CHAR, SERIAL_TRANSMIT_BUFFER_SIZE);
				break;
			}

			//Update tail.
			tail++;
			if (tail == SERIAL_TRANSMIT_BUFFER_SIZE) { tail = 0; }  //Loops back to the beginning of the ring if it is the last element.
		}
	}

	//Record tail.
	Serial.ISRTransmitBufferTail = tail;
}

uint8_t IsTerminalOrNull(char c)
{
	if ((c == SERIAL_TERMINAL_CHAR)
	|| (c == NULL_CHAR)) //Something is wrong if a serial message contains a null character.
	{ return TRUE; }
	
	return FALSE;
}