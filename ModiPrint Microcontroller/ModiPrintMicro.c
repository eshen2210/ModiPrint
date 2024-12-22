 #include "ModiPrintMicro.h"

 void Initialize()
 {	
	InitializeSystem(); //Set global flags to default values.
	InitializeSerial(); //Enable serial communication.
	InitializeHardware(); //Set default values for all hardware parameters.
	InitializePins(); //Populate Pin lists with ports, masks, and direction.

	sei(); //Enable global interrupts.
 }

 uint8_t MainLoop = FALSE;

 void ProtocolLoop()
 {	
	//Contains the command this program will interpret and execute.
	char receiveMessage[SERIAL_RECEIVE_BUFFER_SIZE] = { NULL_CHAR };

	//Wait until there is data in the serial read buffer.
	//If there is no data, then take the time to perform idle tasks.
	while (SerialReadAvailible() == FALSE) 
	{
		MainLoopIdle();
	};

	//Retrieve command from the serial port.
	SerialReadString(receiveMessage);

	//Execute command.
	InterpretMessage(receiveMessage);
 }

 void PrepareNextLoop()
 {
	CheckSystem();

	//New loop, new cycle-specific system flags.
	System.TerminateLoop = FALSE;
 }