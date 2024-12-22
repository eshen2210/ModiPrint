#include "ModiPrintMicro.h"

system_t System;

 void InitializeSystem()
 {
	System.TerminateLoop = FALSE;
	System.HardwareBusy = FALSE;
	System.Pause = FALSE;
	System.Abort = FALSE;
	System.StepperPaused = FALSE;
 }
 
 void HandleError(char * error)
 {
	//Contains the message this program will write to report the error.
	char transmitMessage[SERIAL_TRANSMIT_BUFFER_SIZE] = { NULL_CHAR };
	 
	System.TerminateLoop = TRUE;
	 
	//"^"
	transmitMessage[0] = SERIAL_ERROR_CHAR;

	//Append error.
	int errorIndex = 0;
	for (; ((error != NULL_CHAR) && (errorIndex < (SERIAL_TRANSMIT_BUFFER_SIZE - 3))); errorIndex++)
	{
		transmitMessage[errorIndex + 1] = error[errorIndex];
	}

	SerialWriteString(transmitMessage);
 }

 void ReporterAppendNumber(float number, char * transmitMessage, uint8_t * transmitIndex)
 {
	 ltoa(number, transmitMessage + *transmitIndex, 10);
	 *transmitIndex += 1;
	 for (uint16_t i = 0; i < SERIAL_TRANSMIT_BUFFER_SIZE; i++)
	 {
		if (transmitMessage[*transmitIndex] != NULL_CHAR) //Move index past all of the digits in the number.
		{ *transmitIndex += 1; }
	 }
 }

 void ReporterNextNumber(char * transmitMessage, uint8_t * transmitIndex)
 {
	 transmitMessage[*transmitIndex] = ',';
	 transmitMessage[*transmitIndex + 1] = ' ';
	 *transmitIndex += 2;
 }

void CheckAbort()
{
	//If applicable, abort operations.
	if (System.Abort == TRUE)
	{	
		AbortSteppers(); //Abort equipment operations.
		InitializeSystem(); //Reset flags.
		InitializeHardware(); //Reset hardware.
	}
}

void CheckTransmitSerialISRBuffer()
{
	//If applicable, transmit stepper messages.
	if (SerialISRTransmitBufferEmpty() == FALSE)
	{
		SerialLoadISRTransmitMessage();
	}
}

void CheckSystem()
{	
	//If applicable, abort operations.
	CheckAbort();
	
	//If applicable, resume stepper operations.
	ResumeStepperProtocol();

	//If applicable, transmit stepper messages.
	CheckTransmitSerialISRBuffer();
}

 void WaitForHardwareFree()
 {
	WaitForHardwareFreeStart: ;
	
	while (System.HardwareBusy == TRUE) 
	{		
		CheckSystem();
	}

	//If applicable, transmit stepper messages.
	//This action is taken at this step to ensure no task completed or status messages are sent out of sync.
	//Specifically, the task completed message may interfere with a Limit Switch hit status message.
	if (SerialISRTransmitBufferEmpty() == FALSE)
	{
		SerialLoadISRTransmitMessage();
	}

	//If stepper movements were queued to be executed first, then execute those movements. 
	if (ExecuteStepperProtocol() == TRUE)
	{
		//If stepper movements were executed, then return to the beginning of this function and wait for the stepper protocol to complete.
		goto WaitForHardwareFreeStart;
	}
 }

 void WaitForPause()
 {	
	while (System.Pause == TRUE)
	{
		CheckSystem();
	}
 }

void MainLoopIdle()
{
	CheckSystem();
	ExecuteStepperProtocol();
}