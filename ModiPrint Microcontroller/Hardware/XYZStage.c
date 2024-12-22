#include "ModiPrintMicro.h"

int8_t IsXYZSet(uint32_t xSteps, uint32_t ySteps, uint32_t zSteps)
{
	xyz_stage_t * xyzStage = &Hardware.XYZStage;
	
	if ((xSteps != 0) && (xyzStage->XAxis.AxisName == AXIS_NAME_NULL)) //If X Axis needs to move but the X Axis is not set...
	{ return FALSE; }

	if ((ySteps != 0) && (xyzStage->YAxis.AxisName == AXIS_NAME_NULL)) //If Y Axis...
	{ return FALSE; }

	if ((zSteps != 0) && (xyzStage->ZAxis.AxisName == AXIS_NAME_NULL)) //If Z Axis...
	{ return FALSE; }

	if ((xSteps == 0) //If there is no movement.
	 && (ySteps == 0)
	 && (zSteps == 0))
	{ return FALSE; }

	return TRUE;
}

void OperateMoveXYZ(int32_t xSteps, int32_t ySteps, int32_t zSteps, float exitSpeed)
{		
	//If all Axes are set.
	if (IsXYZSet(xSteps, ySteps, zSteps) == TRUE)
	{
		//Send serial message that this command has been queued.
		ReportMoveXYZ(xSteps, ySteps, zSteps);

		//Synchronizing and waiting for other Hardware is handled by Stepper functions.

		//Queue command to the stepper operations buffer.
		//The stepper buffer will send the serial message signifying task complete.
		MoveSteppers(xSteps, ySteps, zSteps, 0, exitSpeed, FALSE);
	}
	else
	{
		HandleError(ERROR_SYNTAX_INVALID);
		return;
	}
}

void ReportMoveXYZ(int32_t xSteps, int32_t ySteps, int32_t zSteps)
{
	//Contains the message this program will write after executing a command.
	char transmitMessage[SERIAL_TRANSMIT_BUFFER_SIZE] = { NULL_CHAR };
	
	uint8_t transmitIndex = 0;
	
	//"@Mv("
	transmitMessage[transmitIndex++] = SERIAL_TASK_QUEUED_CHAR;
	transmitMessage[transmitIndex++] = 'M';
	transmitMessage[transmitIndex++] = 'v';
	transmitMessage[transmitIndex++] = '(';
	
	//xSteps.
	ReporterAppendNumber(xSteps, transmitMessage, &transmitIndex);
	ReporterNextNumber(transmitMessage, &transmitIndex);

	//ySteps.
	ReporterAppendNumber(ySteps, transmitMessage, &transmitIndex);
	ReporterNextNumber(transmitMessage, &transmitIndex);

	//zSteps.
	ReporterAppendNumber(zSteps, transmitMessage, &transmitIndex);

	//")"
	transmitMessage[transmitIndex] = ')';

	SerialWriteString(transmitMessage);
}