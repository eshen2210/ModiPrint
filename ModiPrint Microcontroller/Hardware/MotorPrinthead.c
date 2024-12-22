#include "ModiPrintMicro.h"

void InitializeMotorPrinthead()
{
	motor_printhead_t * motorPrinthead = &Hardware.MotorPrinthead;
	
	motorPrinthead->StepPin = PIN_ID_NULL;
	motorPrinthead->DirectionPin = PIN_ID_NULL;
	motorPrinthead->StepPulseDuration = 0;
	motorPrinthead->LimitSwitchPin = PIN_ID_NULL;
	motorPrinthead->MaxSpeed = 0;
	motorPrinthead->MaxAcceleration = 0;
	motorPrinthead->LimitSwitchState = LS_Unset;
}

void OperateSetMotorPrinthead(uint8_t stepPin, uint8_t directionPin, uint16_t stepPulseDuration, uint8_t limitSwitchPin, float maxSpeed, float maxAcceleration)
{
	motor_printhead_t * motorPrinthead = &Hardware.MotorPrinthead;
	
	//Input arguments must be valid.
	//Else error is handled.
	if ((IsPinValid(stepPin) == TRUE)
	 && (IsPinValid(directionPin) == TRUE))
	{
		//Send serial message that this command has been queued.
		ReportSetMotorPrinthead(stepPin, directionPin, stepPulseDuration, limitSwitchPin, maxSpeed, maxAcceleration);

		WaitForHardwareFree(); //Wait until all hardware components are no longer taking action.
		System.HardwareBusy = TRUE; //Set system flag that notifies that hardware components are taking action.

		//Set all parameters in this motor-driven Printhead.
		motorPrinthead->StepPin = stepPin;
		motorPrinthead->DirectionPin = directionPin;
		motorPrinthead->StepPulseDuration = stepPulseDuration;
		motorPrinthead->LimitSwitchPin = limitSwitchPin;
		motorPrinthead->MaxSpeed = maxSpeed;
		motorPrinthead->MaxAcceleration = maxAcceleration;

		//Set Step and Direction Pins to output.
		*PinList[stepPin].DirectionAddress |= PinList[stepPin].DirectionMask;
		*PinList[directionPin].DirectionAddress |= PinList[directionPin].DirectionMask;

		//Set Limit Switch Pin to input.
		*PinList[limitSwitchPin].DirectionAddress &= ~PinList[limitSwitchPin].DirectionMask;

		Hardware.PrintheadType = PT_Motor;
		
		//Send serial message that this command has been completed.
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

void OperateMotorPrintWithoutMovement(int32_t printSteps)
{
	motor_printhead_t * motorPrinthead = &Hardware.MotorPrinthead;
	
	//Input arguments must be valid.
	//Else error is handled.
	if ((motorPrinthead != NULL)
	 && (printSteps != 0))
	{
		//Send serial message that this command has been queued.
		ReportMotorPrintWithoutMovement(printSteps);
		
		//Synchronizing and waiting for other Hardware is handled by Stepper functions.

		//Queue command to the stepper operations buffer. 
		//The stepper buffer will send the serial message signifying task complete.
		MoveSteppers(0, 0, 0, printSteps, 0, FALSE);
	}
	else
	{
		HandleError(ERROR_SYNTAX_INVALID);
		return;
	}
}

void OperateMotorPrintWithMovement(int32_t printSteps, int32_t xSteps, int32_t ySteps, int32_t zSteps, float exitSpeed)
{
	motor_printhead_t * motorPrinthead = &Hardware.MotorPrinthead;
	
	//Input arguments must be valid.
	//Else error is handled.
	if ((motorPrinthead != NULL)
	 && (IsXYZSet(xSteps, ySteps, zSteps) == TRUE))
	{
		//Send serial message that this command has been queued.
		ReportMotorPrintWithMovement(printSteps, xSteps, ySteps, zSteps);

		//Synchronizing and waiting for other Hardware is handled by Stepper functions.

		//Queue command to the stepper operations buffer. 
		//The stepper buffer will send the serial message signifying task complete.
		MoveSteppers(xSteps, ySteps, zSteps, printSteps, exitSpeed, FALSE);
	}
	else
	{
		HandleError(ERROR_SYNTAX_INVALID);
		return;
	}
}

void ReportSetMotorPrinthead(uint8_t stepPin, uint8_t directionPin, uint16_t stepPulseDuration, uint8_t limitSwitchPin, float maxSpeed, float maxAcceleration)
{
	//Contains the message this program will write after executing a command.
	char transmitMessage[SERIAL_TRANSMIT_BUFFER_SIZE] = { NULL_CHAR };
	
	uint8_t transmitIndex = 0;
	
	//"@SetMtrPh("
	transmitMessage[transmitIndex++] = SERIAL_TASK_QUEUED_CHAR;
	transmitMessage[transmitIndex++] = 'S';
	transmitMessage[transmitIndex++] = 'e';
	transmitMessage[transmitIndex++] = 't';
	transmitMessage[transmitIndex++] = 'M';
	transmitMessage[transmitIndex++] = 't';
	transmitMessage[transmitIndex++] = 'r';
	transmitMessage[transmitIndex++] = 'P';
	transmitMessage[transmitIndex++] = 'h';
	transmitMessage[transmitIndex++] = '(';
	
	//Step Pin.
	ReporterAppendNumber(stepPin, transmitMessage, &transmitIndex);
	ReporterNextNumber(transmitMessage, &transmitIndex);

	//Direction Pin.
	ReporterAppendNumber(directionPin, transmitMessage, &transmitIndex);
	ReporterNextNumber(transmitMessage, &transmitIndex);

	//Step Pulse Duration.
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

void ReportMotorPrintWithoutMovement(int32_t printSteps)
{
	//Contains the message this program will write after executing a command.
	char transmitMessage[SERIAL_TRANSMIT_BUFFER_SIZE] = { NULL_CHAR };
	
	uint8_t transmitIndex = 0;
	
	//"@PrnMtr("
	transmitMessage[transmitIndex++] = SERIAL_TASK_QUEUED_CHAR;
	transmitMessage[transmitIndex++] = 'P';
	transmitMessage[transmitIndex++] = 'r';
	transmitMessage[transmitIndex++] = 'n';
	transmitMessage[transmitIndex++] = 'M';
	transmitMessage[transmitIndex++] = 't';
	transmitMessage[transmitIndex++] = 'r';
	transmitMessage[transmitIndex++] = '(';
	
	//Print steps.
	ReporterAppendNumber(printSteps, transmitMessage, &transmitIndex);

	//")"
	transmitMessage[transmitIndex] = ')';

	SerialWriteString(transmitMessage);
}

void ReportMotorPrintWithMovement(int32_t printSteps, int32_t xSteps, int32_t ySteps, int32_t zSteps)
{
	//Contains the message this program will write after executing a command.
	char transmitMessage[SERIAL_TRANSMIT_BUFFER_SIZE] = { NULL_CHAR };
	
	uint8_t transmitIndex = 0;
		
	//"@MvPrnMtr("
	transmitMessage[transmitIndex++] = SERIAL_TASK_QUEUED_CHAR;
	transmitMessage[transmitIndex++] = 'M';
	transmitMessage[transmitIndex++] = 'v';
	transmitMessage[transmitIndex++] = 'P';
	transmitMessage[transmitIndex++] = 'r';
	transmitMessage[transmitIndex++] = 'n';
	transmitMessage[transmitIndex++] = 'M';
	transmitMessage[transmitIndex++] = 't';
	transmitMessage[transmitIndex++] = 'r';
	transmitMessage[transmitIndex++] = '(';
		
	//Print steps.
	ReporterAppendNumber(printSteps, transmitMessage, &transmitIndex);
	ReporterNextNumber(transmitMessage, &transmitIndex);

	//X steps.
	ReporterAppendNumber(xSteps, transmitMessage, &transmitIndex);
	ReporterNextNumber(transmitMessage, &transmitIndex);

	//Y steps
	ReporterAppendNumber(ySteps, transmitMessage, &transmitIndex);
	ReporterNextNumber(transmitMessage, &transmitIndex);

	//Z steps.
	ReporterAppendNumber(zSteps, transmitMessage, &transmitIndex);

	//")"
	transmitMessage[transmitIndex] = ')';

	SerialWriteString(transmitMessage);
}