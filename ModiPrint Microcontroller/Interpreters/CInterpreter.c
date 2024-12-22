#include "ModiPrintMicro.h"

void ExecuteC00(char * receiveMessage, uint8_t * receiveIndex)
{
	//Contains the message this program will write after executing a command.
	char transmitMessage[SERIAL_TRANSMIT_BUFFER_SIZE] = { NULL_CHAR };
	
	//Skip past "C00".
	*receiveIndex = *receiveIndex + 3;

	//Store all characters after "C00" in transmitMessage.
	uint8_t transmitIndex = 0;
	transmitMessage[transmitIndex] = SERIAL_TASK_QUEUED_CHAR;
	transmitIndex++;
	for (; IsInterpretLoopTerminated(receiveMessage, *receiveIndex) == FALSE; transmitIndex++)
	{
		transmitMessage[transmitIndex] = receiveMessage[*receiveIndex];
		*receiveIndex = *receiveIndex + 1;
	}

	SerialWriteString(transmitMessage);
}

void ExecuteC10(char * receiveMessage, uint8_t * receiveIndex)
{	
	//Default values for an Axis.
	char axisName = AXIS_NAME_NULL;
	uint8_t stepPin = PIN_ID_NULL;
	uint8_t directionPin = PIN_ID_NULL;
	uint8_t stepPulseDuration = 0;
	uint8_t limitSwitchPin = PIN_ID_NULL;
	uint32_t maxSpeed = 0;
	uint32_t maxAcceleration = 0;

	//Axis to be set.
	axis_t * axis = NULL;

	NextChar(receiveMessage, receiveIndex);
	while (IsInterpretLoopTerminated(receiveMessage, *receiveIndex) == FALSE)
	{
		switch (receiveMessage[*receiveIndex])
		{
			case AXIS_NAME_X:
				axisName = AXIS_NAME_X;
				axis = &Hardware.XYZStage.XAxis;
				break;
			case AXIS_NAME_Y:
				axisName = AXIS_NAME_Y;
				axis = &Hardware.XYZStage.YAxis;
				break;
			case AXIS_NAME_Z:
				axisName = AXIS_NAME_Z;
				axis = &Hardware.XYZStage.ZAxis;
				break;
			case 'T':
				stepPin = (uint8_t)ParseNextNum(receiveMessage, receiveIndex);
				break;
			case 'D':
				directionPin = (uint8_t)ParseNextNum(receiveMessage, receiveIndex);
				break;
			case 'P':
				stepPulseDuration = (uint8_t)ParseNextNum(receiveMessage, receiveIndex);
				break;
			case 'L':
				limitSwitchPin = (uint8_t)ParseNextNum(receiveMessage, receiveIndex);
				break;
			case 'S':
				maxSpeed = ParseNextNum(receiveMessage, receiveIndex);
				break;
			case 'A':
				maxAcceleration = ParseNextNum(receiveMessage, receiveIndex);
				break;
			default:
				HandleError(ERROR_SYNTAX_INVALID);
				return;
		}
		NextChar(receiveMessage, receiveIndex);
	}

	if (axis != NULL)
	{
		OperateSetAxis(axis, axisName, stepPin, directionPin, stepPulseDuration, limitSwitchPin, maxSpeed, maxAcceleration);
	}
	else
	{
		HandleError(ERROR_SYNTAX_INVALID);
		return;
	}
}

void ExecuteC11(char * receiveMessage, uint8_t * receiveIndex)
{
	uint8_t stepPin = PIN_ID_NULL;
	uint8_t directionPin = PIN_ID_NULL;
	uint8_t stepPulseDuration = 0;
	uint8_t limitSwitchPin = PIN_ID_NULL;
	uint32_t maxSpeed = 0;
	uint32_t maxAcceleration = 0;

	NextChar(receiveMessage, receiveIndex);
	while (IsInterpretLoopTerminated(receiveMessage, *receiveIndex) == FALSE)
	{
		switch (receiveMessage[*receiveIndex])
		{
			case 'T':
				stepPin = (uint8_t)ParseNextNum(receiveMessage, receiveIndex);
				break;
			case 'D':
				directionPin = (uint8_t)ParseNextNum(receiveMessage, receiveIndex);
				break;
			case 'P':
				stepPulseDuration = (uint8_t)ParseNextNum(receiveMessage, receiveIndex);
				break;
			case 'L':
				limitSwitchPin = (uint8_t)ParseNextNum(receiveMessage, receiveIndex);
				break;
			case 'S':
				maxSpeed = ParseNextNum(receiveMessage, receiveIndex);
				break;
			case 'A':
				maxAcceleration = ParseNextNum(receiveMessage, receiveIndex);
				break;
			default:
				HandleError(ERROR_SYNTAX_INVALID);
				return;
		}
		NextChar(receiveMessage, receiveIndex);
	}

	OperateSetMotorPrinthead(stepPin, directionPin, stepPulseDuration, limitSwitchPin, maxSpeed, maxAcceleration);
}

void ExecuteC12(char * receiveMessage, uint8_t * receiveIndex)
{
	uint8_t valvePin = PIN_ID_NULL;

	NextChar(receiveMessage, receiveIndex);
	while (IsInterpretLoopTerminated(receiveMessage, *receiveIndex) == FALSE)
	{
		switch (receiveMessage[*receiveIndex])
		{
			case 'V':
				valvePin = (uint8_t)ParseNextNum(receiveMessage, receiveIndex);
				break;
			default:
				HandleError(ERROR_SYNTAX_INVALID);
				return;
		}
		NextChar(receiveMessage, receiveIndex);
	}

	OperateSetValvePrinthead(valvePin);
}