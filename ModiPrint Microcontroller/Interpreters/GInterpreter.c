#include "ModiPrintMicro.h"

void ExecuteG00(char * receiveMessage, uint8_t * receiveIndex)
{
	//Motor movements in relative coordinates.
	int32_t xSteps = 0;
	int32_t ySteps = 0;
	int32_t zSteps = 0;
	float exitSpeed = 0;

	NextChar(receiveMessage, receiveIndex);
	while (IsInterpretLoopTerminated(receiveMessage, *receiveIndex) == FALSE)
	{		
		switch (receiveMessage[*receiveIndex])
		{			
			case 'X':
				xSteps = (int32_t)ParseNextNum(receiveMessage, receiveIndex);
				break;
			case 'Y':
				ySteps = (int32_t)ParseNextNum(receiveMessage, receiveIndex);
				break;
			case 'Z':
				zSteps = (int32_t)ParseNextNum(receiveMessage, receiveIndex);
				break;
			case 'T':
				exitSpeed = ParseNextNum(receiveMessage, receiveIndex);
				break;
			default:
				HandleError(ERROR_SYNTAX_INVALID);
				return;
		}
		NextChar(receiveMessage, receiveIndex);
	}

	OperateMoveXYZ(xSteps, ySteps, zSteps, exitSpeed);
}

void ExecuteG01(char * receiveMessage, uint8_t * receiveIndex)
{
	//Only run if this printer is currently using a motor-driven Printhead.
	if (Hardware.PrintheadType == PT_Motor)
	{
		int32_t printSteps = 0;
		int32_t xSteps = 0;
		int32_t ySteps = 0;
		int32_t zSteps = 0;
		float exitSpeed = 0;

		NextChar(receiveMessage, receiveIndex);
		while (IsInterpretLoopTerminated(receiveMessage, *receiveIndex) == FALSE)
		{
			switch (receiveMessage[*receiveIndex])
			{
				case 'E':
					printSteps = (int32_t)ParseNextNum(receiveMessage, receiveIndex);
					break;
				case 'X':
					xSteps = (int32_t)ParseNextNum(receiveMessage, receiveIndex);
					break;
				case 'Y':
					ySteps = (int32_t)ParseNextNum(receiveMessage, receiveIndex);
					break;
				case 'Z':
					zSteps = (int32_t)ParseNextNum(receiveMessage, receiveIndex);
					break;
				case 'T':
					exitSpeed = ParseNextNum(receiveMessage, receiveIndex);
					break;
				default:
					HandleError(ERROR_SYNTAX_INVALID);
					return;
			}
			NextChar(receiveMessage, receiveIndex);
		}

		OperateMotorPrintWithMovement(printSteps, xSteps, ySteps, zSteps, exitSpeed);
	}
	else
	{
		HandleError(ERROR_HARDWARE_UNSET);
		return;
	}
}

void ExecuteG02(char * receiveMessage, uint8_t * receiveIndex)
{
	if (Hardware.PrintheadType == PT_Valve)
	{
		int32_t xSteps = 0;
		int32_t ySteps = 0;
		int32_t zSteps = 0;
		float exitSpeed = 0;

		NextChar(receiveMessage, receiveIndex);
		while (IsInterpretLoopTerminated(receiveMessage, *receiveIndex) == FALSE)
		{
			switch (receiveMessage[*receiveIndex])
			{
				case 'X':
					xSteps = (int32_t)ParseNextNum(receiveMessage, receiveIndex);
					break;
				case 'Y':
					ySteps = (int32_t)ParseNextNum(receiveMessage, receiveIndex);
					break;
				case 'Z':
					zSteps = (int32_t)ParseNextNum(receiveMessage, receiveIndex);
					break;
				case 'T':
					exitSpeed = ParseNextNum(receiveMessage, receiveIndex);
					break;
				default:
					HandleError(ERROR_SYNTAX_INVALID);
					return;
			}
			NextChar(receiveMessage, receiveIndex);
		}

		OperateValvePrintWithMovement(xSteps, ySteps, zSteps, exitSpeed);
	}
	else
	{
		HandleError(ERROR_HARDWARE_UNSET);
		return;
	}
}

void ExecuteG11(char * receiveMessage, uint8_t * receiveIndex)
{
	if (Hardware.PrintheadType == PT_Motor)
	{
		int32_t printSteps = 0;

		NextChar(receiveMessage, receiveIndex);
		while (IsInterpretLoopTerminated(receiveMessage, *receiveIndex) == FALSE)
		{
			switch (receiveMessage[*receiveIndex])
			{
				case 'E':
					printSteps = (int32_t)ParseNextNum(receiveMessage, receiveIndex);
					break;
				default:
					HandleError(ERROR_SYNTAX_INVALID);
					return;
			}
			NextChar(receiveMessage, receiveIndex);
		}

		OperateMotorPrintWithoutMovement(printSteps);
	}
	else
	{
		HandleError(ERROR_HARDWARE_UNSET);
		return;
	}
}

void ExecuteG12(char * receiveMessage, uint8_t * receiveIndex)
{
	if (Hardware.PrintheadType == PT_Valve)
	{
		NextChar(receiveMessage, receiveIndex);
		while (IsInterpretLoopTerminated(receiveMessage, *receiveIndex) == FALSE)
		{
			switch (receiveMessage[*receiveIndex])
			{
				case 'O':
				{
					uint32_t openTime = (uint32_t)ParseNextNum(receiveMessage, receiveIndex);
					OperateValvePrintWithoutMovement(openTime);
					break;
				}
				case 'C':
				{
					OperateStopValvePrint(&Hardware.ValvePrinthead);
					break;
				}
				default:
				{
					HandleError(ERROR_SYNTAX_INVALID);
					return;
				}
			}
			NextChar(receiveMessage, receiveIndex);
		}
	}
	else
	{
		HandleError(ERROR_HARDWARE_UNSET);
		return;
	}
}