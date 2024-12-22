#include "ModiPrintMicro.h"

void InitializeAxis(axis_t * axis)
{
	axis->AxisName = AXIS_NAME_NULL;
	axis->StepPin = PIN_ID_NULL;
	axis->DirectionPin = PIN_ID_NULL;
	axis->StepPulseDuration = 0;
	axis->LimitSwitchPin = PIN_ID_NULL;
	axis->MaxSpeed = 0;
	axis->MaxAcceleration = 0;
	axis->LimitSwitchState = LS_Unset;
}

void OperateSetAxis(axis_t * axis, char axisName, uint8_t stepPin, uint8_t directionPin, uint16_t stepPulseDuration, uint8_t limitSwitchPin, float maxSpeed, float maxAcceleration)
{
	//Input arguments must be valid.
	//Else error is handled.
	if ((IsPinValid(stepPin) == TRUE)
	 && (IsPinValid(directionPin) == TRUE)
	 && (stepPulseDuration > 0)
	 && (maxSpeed > 0)
	 && (maxAcceleration > 0))
	{
		//Send serial message that this command has been queued.
		ReportSetAxis(axisName, stepPin, directionPin, stepPulseDuration, limitSwitchPin, maxSpeed, maxAcceleration);
	
		WaitForHardwareFree(); //Wait until all hardware components are no longer taking action.
		System.HardwareBusy = TRUE; //Set system flag that notifies that hardware components are taking action.

		//Set all parameters to this Axis.
		axis->AxisName = axisName;
		axis->StepPin = stepPin;
		axis->DirectionPin = directionPin;
		axis->StepPulseDuration = stepPulseDuration;
		axis->LimitSwitchPin = limitSwitchPin;
		axis->MaxSpeed = maxSpeed;
		axis->MaxAcceleration = maxAcceleration;

		//Set Step and Direction Pins to output.
		*PinList[stepPin].DirectionAddress |= PinList[stepPin].DirectionMask;
		*PinList[directionPin].DirectionAddress |= PinList[directionPin].DirectionMask;

		//Set Limit Switch Pin to input.
		*PinList[limitSwitchPin].DirectionAddress &= ~PinList[limitSwitchPin].DirectionMask;

		//Send serial message that this command has been complete.
		SerialWriteTaskCompleted();

		//Task completed.
		System.HardwareBusy = FALSE;
	}
	else
	{
		HandleError(ERROR_SYNTAX_INVALID);
		return;
	}
}

void ReportSetAxis(char axisName, uint8_t stepPin, uint8_t directionPin, uint8_t stepPulseDuration, uint8_t limitSwitchPin, float maxSpeed, float maxAcceleration)
{
	//Contains the message this program will write after executing a command.
	char transmitMessage[SERIAL_TRANSMIT_BUFFER_SIZE] = { NULL_CHAR };
	
	uint8_t transmitIndex = 0;
	
	//"@Set"
	transmitMessage[transmitIndex++] = SERIAL_TASK_QUEUED_CHAR;
	transmitMessage[transmitIndex++] = 'S';
	transmitMessage[transmitIndex++] = 'e';
	transmitMessage[transmitIndex++] = 't';

	//Axis Name.
	transmitMessage[transmitIndex++] = axisName;

	//"Axs("
	transmitMessage[transmitIndex++] = 'A';
	transmitMessage[transmitIndex++] = 'x';
	transmitMessage[transmitIndex++] = 's';
	transmitMessage[transmitIndex++] = '(';
	
	//Step Pin.
	ReporterAppendNumber(stepPin, transmitMessage, &transmitIndex);
	ReporterNextNumber(transmitMessage, &transmitIndex);

	//Direction Pin.
	ReporterAppendNumber(directionPin, transmitMessage, &transmitIndex);
	ReporterNextNumber(transmitMessage, &transmitIndex);

	//StepPulseDuration.
	ReporterAppendNumber(stepPulseDuration, transmitMessage, &transmitIndex);
	ReporterNextNumber(transmitMessage, &transmitIndex);

	//Limit Switch Pin.
	ReporterAppendNumber(limitSwitchPin, transmitMessage, &transmitIndex);
	ReporterNextNumber(transmitMessage, &transmitIndex);

	//Max Speed.
	ReporterAppendNumber(maxSpeed, transmitMessage, &transmitIndex);
	ReporterNextNumber(transmitMessage, &transmitIndex);

	//Max Acceleration.
	ReporterAppendNumber(maxAcceleration, transmitMessage, &transmitIndex);

	//")"
	transmitMessage[transmitIndex] = ')';

	SerialWriteString(transmitMessage);
}
