#ifndef SYSTEM_H_
#define SYSTEM_H_

#define ERROR_SYNTAX_INVALID "Syn"
#define ERROR_HARDWARE_UNSET "Uns"
#define ERROR_INCORRECT_EXIT_SPEED "Ext"
#define ERROR_MINIMUM_CYCLES_EXCEEDED "Cyc"

//A collection of global flags.
typedef struct{

	//If true, then terminate one protocol cycle.
	uint8_t TerminateLoop;

	//If true, then the hardware is currently performing some action and no other actions should begin.
	volatile uint8_t HardwareBusy;

	//If true, then pause the protocol at the beginning of the next protocol cycle.
	uint8_t Pause;

	//If true, then clear the movement buffer.
	uint8_t Abort;

	//If true, then the stepper protocol is currently paused.
	uint8_t StepperPaused;
} system_t;
extern system_t System;

//Set global flags to default values.
 void InitializeSystem();

 //Send error message through the serial port.
 void HandleError(char * error);

 //Used in reporter functions.
 //Appends a number to a specified index in a character array then shifts the index based on the amount of digits in the number.
 void ReporterAppendNumber(float number, char * transmitMessage, uint8_t * transmitIndex);

 //Used in reporter functions.
 //Appends a comma then a whitespace and shifts the index by two.
 void ReporterNextNumber(char * transmitMessage, uint8_t * transmitIndex);

 //Wait until Hardware has finished operations.
 void WaitForHardwareFree();

 //Wait until system is no longer paused.
 void WaitForPause();

 //Execute this while the main loop is is idling.
void MainLoopIdle();

#endif