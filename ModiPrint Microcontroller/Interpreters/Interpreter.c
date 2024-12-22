#include "ModiPrintMicro.h"

void InterpretMessage(char * receiveMessage)
{	
	for (uint8_t receiveIndex = 0; IsInterpretLoopTerminated(receiveMessage, receiveIndex) == FALSE; NextChar(receiveMessage, &receiveIndex))
	{
		switch (receiveMessage[receiveIndex])
		{
			case 'G': //Movement and printing
			{				
				switch ((uint8_t)ParseNextNum(receiveMessage, &receiveIndex))
				{
					case 00:
						ExecuteG00(receiveMessage, &receiveIndex);
						break;
					case 01:
						ExecuteG01(receiveMessage, &receiveIndex);
						break;
					case 02:
						ExecuteG02(receiveMessage, &receiveIndex);
						break;
					case 11:
						ExecuteG11(receiveMessage, &receiveIndex);
						break;
					case 12:
						ExecuteG12(receiveMessage, &receiveIndex);
						break;
					default:
						HandleError(ERROR_SYNTAX_INVALID);
						return;
				}
				break;
			}
			case 'C': //Calibration and setup
			{
				switch ((uint8_t)ParseNextNum(receiveMessage, &receiveIndex))
				{
					case 00:
						ExecuteC00(receiveMessage, &receiveIndex);
						break;
					case 10:
						ExecuteC10(receiveMessage, &receiveIndex);
						break;
					case 11:
						ExecuteC11(receiveMessage, &receiveIndex);
						break;
					case 12:
						ExecuteC12(receiveMessage, &receiveIndex);
						break;
					default:
						HandleError(ERROR_SYNTAX_INVALID);
						return;
				}
				break;
			}
			case SERIAL_TERMINAL_CHAR:
			{
				HandleError(ERROR_SYNTAX_INVALID);
				return;
			}
			case ' ':
			{
				NextChar(receiveMessage, &receiveIndex);
				break;
			}
			case NULL_CHAR:
			{
				NextChar(receiveMessage, &receiveIndex); 
				break;
			}
			case '?':
			{
				//To Do: This comes up every time a serial connection occurs but I can't seem to freakin catch it.
				NextChar(receiveMessage, &receiveIndex); 
				break;
			}
			default:
			{
				HandleError(ERROR_SYNTAX_INVALID);
				return;
			}
		}
	}
}

uint32_t ParseNextNum(char * receiveMessage, uint8_t * receiveIndex)
{
	while ((IsInterpretLoopTerminated(receiveMessage, *receiveIndex) == FALSE) //Should this function stop.
		&& (isdigit(receiveMessage[*receiveIndex]) == FALSE) //If this char is not numeric.
		&& (receiveMessage[*receiveIndex] != '-'))
	{
		*receiveIndex = *receiveIndex + 1;
	}

	return atol(&receiveMessage[*receiveIndex]);
}

void NextChar(char * receiveMessage, uint8_t * receiveIndex)
{
	*receiveIndex = *receiveIndex + 1;

	while ((IsInterpretLoopTerminated(receiveMessage, *receiveIndex) == FALSE)
		&& (isalpha(receiveMessage[*receiveIndex]) == FALSE))
	{
		*receiveIndex = *receiveIndex + 1;
	}
}

int8_t IsInterpretLoopTerminated(char * receiveMessage, uint8_t receiveIndex)
{
	if ((receiveIndex >= SERIAL_RECEIVE_BUFFER_SIZE) //If out of bounds.
	 || (receiveMessage[receiveIndex] == SERIAL_TERMINAL_CHAR) //If at the end of the message.
	 || (System.TerminateLoop == TRUE)) //If process should be aborted. 
	{
		return TRUE;
	}
	else
	{
		return FALSE;
	}
}