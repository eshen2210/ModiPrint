#include "ModiPrintMicro.h"

void PrepareStepperRealTime(movement_t * movement, int32_t xSteps, int32_t ySteps, int32_t zSteps, int32_t eSteps, float movementExitSpeed, uint8_t doValvePrint)
{	
	xyz_stage_t * xyzStage = &Hardware.XYZStage;
	motor_printhead_t * motorPrinthead = &Hardware.MotorPrinthead;
	valve_printhead_t * valvePrinthead = (doValvePrint == TRUE) ? &Hardware.ValvePrinthead : NULL;
	
	//maxSpeed in steps/s.
	//acceleration in steps/s^2.
	//timeSquared in s^2.
	//Some values with "cycles" in its variable name have time in units of Timer 1 cycles.

	movement->MovementExitSpeed = movementExitSpeed;

	//Set the on-movement pin.
	if (valvePrinthead != NULL)
	{
		movement->ValvePin = (IsPinValid(valvePrinthead->ValvePin) == TRUE) ? valvePrinthead->ValvePin : PIN_ID_NULL;
	}
	else
	{
		movement->ValvePin = PIN_ID_NULL;
	}

	//Load the number of steps for each actuator stepper into the data block.
	movement->XTotalSteps = xSteps;
	movement->YTotalSteps = ySteps;
	movement->ZTotalSteps = zSteps;
	movement->ETotalSteps = eSteps;

	//Absolute values for each of the steps.
	uint32_t xAbsTotalSteps = (xSteps >= 0) ? xSteps : (-1 * xSteps);
	uint32_t yAbsTotalSteps = (ySteps >= 0) ? ySteps : (-1 * ySteps);
	uint32_t zAbsTotalSteps = (zSteps >= 0) ? zSteps : (-1 * zSteps);
	uint32_t eAbsTotalSteps = (eSteps >= 0) ? eSteps : (-1 * eSteps);

	//The Bresenham counters should begin at zero.
	movement->XCounter = movement->YCounter = movement->ZCounter = movement->ECounter = 0;

	//Initialize the number of leading steps.
	//Leading is defines the stepper that will take the most steps in this movement.
	//The stepper timer (Timer 1) will tick based on the leading stepper's movement.
	//The other stepper movements will be rasterized with the Bresenham algorithm.
	movement->LeadingStepsTaken = 0;
	movement->LeadingStepsTotal = xAbsTotalSteps;
	movement->LeadingStepsTotal = (yAbsTotalSteps > movement->LeadingStepsTotal) ? yAbsTotalSteps : movement->LeadingStepsTotal;
	movement->LeadingStepsTotal = (zAbsTotalSteps > movement->LeadingStepsTotal) ? zAbsTotalSteps : movement->LeadingStepsTotal;
	movement->LeadingStepsTotal = (eAbsTotalSteps > movement->LeadingStepsTotal) ? eAbsTotalSteps : movement->LeadingStepsTotal;

	//The "distance" of the movement, which is simply the magnitude of this movement.
	//Used to scale the speeds of the movement.
	double movementTotalSteps = 0;
	//Find the max speed and acceleration of this movement such that no acceleration values are exceeded for each Axis.
	float movementMaxSpeed = FLT_MAX;
	float movementAcceleration = FLT_MAX;
	//Set the step pulse duration of all motors (in microseconds).
	//Ideally, each stepper motor would have individualized step pulse durations. However, there isn't enough timers on the Mega 2560 to end four stepper pulses asynchronously.
	//The compromise is that all stepper motors will share the longest minimum pulse duration among all steppers.
	uint16_t stepPulseDuration = 0; //(in microseconds)

	//Calculate movementTotalSteps and stepPulseDuration in these if statements.
	if (xSteps != 0) 
	{ 
		movementTotalSteps = ((float)xSteps * (float)xSteps);
		stepPulseDuration = (stepPulseDuration > xyzStage->XAxis.StepPulseDuration) ? stepPulseDuration : xyzStage->XAxis.StepPulseDuration;
	}
	if (ySteps != 0) 
	{ 
		movementTotalSteps += ((float)ySteps * (float)ySteps);
		stepPulseDuration = (stepPulseDuration > xyzStage->YAxis.StepPulseDuration) ? stepPulseDuration : xyzStage->YAxis.StepPulseDuration;
	}
	if (zSteps != 0)
	{
		movementTotalSteps += ((float)zSteps * (float)zSteps);
		stepPulseDuration = (stepPulseDuration > xyzStage->ZAxis.StepPulseDuration) ? stepPulseDuration : xyzStage->ZAxis.StepPulseDuration;
	}
	if (eSteps != 0)
	{
		movementTotalSteps += ((float)eSteps * (float)eSteps);
		stepPulseDuration = (stepPulseDuration > motorPrinthead->StepPulseDuration) ? stepPulseDuration : motorPrinthead->StepPulseDuration;
	}
	movementTotalSteps = sqrt(movementTotalSteps);
	
	//Calculate the speeds of the leading stepper.
	//Leading speeds are in units of steps/cycles.
	//Leading acceleration is in units of steps/cycles^2.
	float leadingAcceleration = FLT_MAX;
	float leadingMaxSpeed = 0;
	float leadingEntrySpeed = 0;
	float leadingExitSpeed = 0;
	if (xSteps != 0)
	{
		float invXScaled = (float)movementTotalSteps / (float)xAbsTotalSteps;
		movementMaxSpeed = xyzStage->XAxis.MaxSpeed * invXScaled;
		movementAcceleration = xyzStage->XAxis.MaxAcceleration * invXScaled;

		//If the X actuator has the leading stepper...
		if (movement->LeadingStepsTotal == xAbsTotalSteps)
		{
			float xScaled = (float)xAbsTotalSteps / (float)movementTotalSteps;
			leadingAcceleration = movementAcceleration * xScaled;
			leadingMaxSpeed = movementMaxSpeed * xScaled;
			leadingEntrySpeed = movement->MovementEntrySpeed * xScaled;
			leadingExitSpeed = movementExitSpeed * xScaled;
		}
	}
	if (ySteps != 0)
	{
		float invYScaled = (float)movementTotalSteps / (float)yAbsTotalSteps;
		float yScaledMaxSpeed = xyzStage->YAxis.MaxSpeed * invYScaled;
		movementMaxSpeed = (yScaledMaxSpeed < movementMaxSpeed) ? yScaledMaxSpeed : movementMaxSpeed;
		float yScaledAcceleration = xyzStage->YAxis.MaxAcceleration * invYScaled;
		movementAcceleration = (yScaledAcceleration < movementAcceleration) ? yScaledAcceleration : movementAcceleration;

		//If the Y actuator has the leading stepper...
		if (movement->LeadingStepsTotal == yAbsTotalSteps)
		{
			float yScaled = (float)yAbsTotalSteps / (float)movementTotalSteps;
			leadingAcceleration = movementAcceleration * yScaled;
			leadingMaxSpeed = movementMaxSpeed * yScaled;
			leadingEntrySpeed = movement->MovementEntrySpeed * yScaled;
			leadingExitSpeed = movementExitSpeed * yScaled;
		}
	}
	if (zSteps != 0)
	{
		float invZScaled = (float)movementTotalSteps / (float)zAbsTotalSteps;
		float zScaledMaxSpeed = xyzStage->ZAxis.MaxSpeed * invZScaled;
		movementMaxSpeed = (zScaledMaxSpeed < movementMaxSpeed) ? zScaledMaxSpeed : movementMaxSpeed;
		float zScaledAcceleration = xyzStage->ZAxis.MaxAcceleration * invZScaled;
		movementAcceleration = (zScaledAcceleration < movementAcceleration) ? zScaledAcceleration : movementAcceleration;

		//If the Z actuator has the leading stepper...
		if (movement->LeadingStepsTotal == zAbsTotalSteps)
		{			
			float zScaled = (float)zAbsTotalSteps / (float)movementTotalSteps;
			leadingAcceleration = movementAcceleration * zScaled;
			leadingMaxSpeed = movementMaxSpeed * zScaled;
			leadingEntrySpeed = movement->MovementEntrySpeed * zScaled;
			leadingExitSpeed = movementExitSpeed * zScaled;
		}
	}
	if (eSteps != 0)
	{
		float invEScaled = (float)movementTotalSteps / (float)eAbsTotalSteps;
		float eScaledMaxSpeed = motorPrinthead->MaxSpeed * invEScaled;
		movementMaxSpeed = (eScaledMaxSpeed < movementMaxSpeed) ? eScaledMaxSpeed : movementMaxSpeed;
		float eScaledAcceleration = motorPrinthead->MaxAcceleration * invEScaled;
		movementAcceleration = (eScaledAcceleration < movementAcceleration) ? eScaledAcceleration : movementAcceleration;

		//If the E actuator has the leading stepper...
		if (movement->LeadingStepsTotal == eAbsTotalSteps)
		{
			float eScaled = (float)eAbsTotalSteps / (float)movementTotalSteps;
			leadingAcceleration = movementAcceleration * eScaled;
			leadingMaxSpeed = movementMaxSpeed * eScaled;
			leadingEntrySpeed = movement->MovementEntrySpeed * eScaled;
			leadingExitSpeed = movementExitSpeed * eScaled;
		}
	}

	//StepPulseDelay is in units of cycles.
	//StepPulseDuration is in units of microseconds.
	//Timer 0 with a prescaler of 8 and a CPU frequency of 16 MHz will tick once every 0.5 microseconds.
	movement->StepPulseDelay = 2 * stepPulseDuration;

	//Number of steps to go from entry speed to the maximum allowable exit speed assuming pure acceleration/deceleration throughout the entire movement.
	float leadingEntryToExitSteps = fabs((leadingExitSpeed - leadingEntrySpeed) / leadingAcceleration) * (leadingEntrySpeed + 0.5 * (leadingExitSpeed - leadingEntrySpeed));
	//Number of steps for the leading stepper to go from entry speed to maximum speed, then from maximum speed to exit speed.
	float leadingEntryToCruiseSteps = ((leadingMaxSpeed - leadingEntrySpeed) / leadingAcceleration) * (leadingEntrySpeed + 0.5 * (leadingMaxSpeed - leadingEntrySpeed));
	//Number of steps required to decelerate from max speed to exit speed.
	float leadingCruiseToExitSteps = (leadingMaxSpeed != leadingExitSpeed) ? ((leadingMaxSpeed - leadingExitSpeed) / leadingAcceleration * (leadingExitSpeed + 0.5 * (leadingMaxSpeed - leadingExitSpeed))) : 0;

	//Is there enough distance to accelerate to max speed and form a trapezoid velocity profile?
	if ((leadingEntryToCruiseSteps + leadingCruiseToExitSteps) <= movement->LeadingStepsTotal)
	{
		movement->LeadingStepsToCruise = ceil(leadingEntryToCruiseSteps);
		movement->LeadingStepsToDeceleration = ceil(movement->LeadingStepsTotal - leadingCruiseToExitSteps);
		movement->Phase = VP_Acceleration;
	}
	else
	{
		//Is there enough distance to accelerate then decelerate and form a triangle velocity profile?		
		if ((uint32_t)leadingEntryToExitSteps < movement->LeadingStepsTotal)
		{
			//Movement can be divided into two parts for this profile.
			//One half of the movement involves accelerating to an arbitrary speed, then decelerating back to entry speed. Acceleration == -1 * deceleration so acceleration and deceleration share exactly half of this section's distance.
			//The other half of the movement involves travelling from the entry speed to the exit speed across presumedEntryToExitDistnce.
			//Whichever part occurs first depends on whether or not the exit speed is greater than the movement speed.
			if ((uint32_t)leadingEntrySpeed >= (uint32_t)leadingExitSpeed)
			{
				//Acceleration and deceleration occurs before deceleration to exit speed.
				movement->LeadingStepsToCruise = movement->LeadingStepsToDeceleration = ceil((movement->LeadingStepsTotal - leadingEntryToExitSteps) / 2);
				movement->Phase = VP_Acceleration;
			}
			else
			{
				//Acceleration to exit speed occurs before further acceleration and deceleration.
				movement->LeadingStepsToCruise = movement->LeadingStepsToDeceleration = ceil(leadingEntryToExitSteps + (movement->LeadingStepsTotal - leadingEntryToExitSteps) / 2);
				movement->Phase = VP_Acceleration;
			}
		}
		//The case where the entire movement is entirely acceleration or deceleration.
		else if ((uint32_t)leadingEntryToExitSteps == movement->LeadingStepsTotal)
		{
			if ((uint32_t)leadingEntrySpeed < (uint32_t)leadingExitSpeed)
			{
				//Entire movement is acceleration.
				movement->LeadingStepsToCruise = movement->LeadingStepsToDeceleration = INT32_MAX;
				movement->Phase = VP_Acceleration;
			}
			else
			{
				//Entire movement is deceleration.
				movement->Phase = VP_Deceleration;
			}
		}
		else
		{
			//Should only reach this point if the exit speed value from serial commands is incorrect.
			//Can also reach this point if LeadingStepsTotal is zero (which should not happen).
			HandleError(ERROR_INCORRECT_EXIT_SPEED);
		}
	}

	//Initial acceleration calculation parameter of the first step of a movement starting from zero entry speed.
	float leadingAccelerationCycles = leadingAcceleration / (TIMER_1_CYCLES_FREQUENCY * TIMER_1_CYCLES_FREQUENCY);
	movement->InvLeadingAccelerationCycles = 1 / leadingAccelerationCycles;
	//Number of steps to go from zero speed to the entry speed. 
	//Plus one because the first calculated Cycles value pertains to the time in between the first and second step.
	float leadingZeroToEntryStepsPlusOne = 0.5 * leadingEntrySpeed * leadingEntrySpeed / leadingAcceleration + 1;
	//Don't ask me about the following equation man. It just works.
	movement->InitialVirtualTotalCycles = (2 * leadingZeroToEntryStepsPlusOne) / sqrt(2 * leadingZeroToEntryStepsPlusOne * leadingAccelerationCycles);

	//Since deceleration calculations involve the Cycles value first, initialize this value during deceleration-first movement.
	if (movement->Phase == VP_Deceleration)
	{
		movement->Cycles = (movement->InvLeadingAccelerationCycles / movement->InitialVirtualTotalCycles);
	}

	//At this point all member variables of this movement should be loaded.
}