#ifndef INTERPRETER_H_
#define INTERPRETER_H_

#include "CInterpreter.h"
#include "GInterpreter.h"
#include "UInterpreter.h"

#define COMMAND_CHECK_CONNECTION "C00"
#define COMMAND_SET_AXIS "C10"
#define COMMAND_SET_MOTOR_PRINTHEAD "C11"
#define COMMAND_SET_VALVE_PRINTHEAD "C12"
#define COMMAND_AXES_MOVEMENT "G00"
#define COMMAND_MOTOR_PRINT_WITH_MOVEMENT "G01"
#define COMMAND_VALVE_PRINT_WITH_MOVEMENT "G02"
#define COMMAND_MOTOR_PRINT_WITHOUT_MOVEMENT "G11"
#define COMMAND_VALVE_PRINT_WITHOUT_MOVEMENT "G12"

//Parses receiveMessage, executes commands, then returns transmitMessage.
void InterpretMessage(char * receiveMessage);

//Returns the next numeric value in the message and advances the index.
uint32_t ParseNextNum(char * receiveMessage, uint8_t * receiveIndex);

//Increment index until char array is at the next char.
void NextChar(char * receiveMessage, uint8_t * receiveIndex);

//Used by the message interpreter.
//Based on the current index and the global state, should the InterpretMessage loop terminate or continue?
int8_t IsInterpretLoopTerminated(char * receiveMessage, uint8_t receiveIndex);

#endif