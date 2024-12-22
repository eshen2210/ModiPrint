#ifndef STEPPERS_H_
#define STEPPERS_H_

//The different phases of the stepper velocity profile.
enum VELOCITY_PROFILE_PHASE{
	VP_Unset = 0,
	VP_Acceleration = 1,
	VP_Cruise = 2,
	VP_Deceleration = 3,
	VP_Limit = 4 //When a limit switch is hit.
};

#include "Prepper.h"

//Cycles / s of Timer 1 (main stepper timer) with a prescaler of 64.
#define TIMER_1_CYCLES_FREQUENCY 250000.0f

//Initial value of OCR1A.
//This gives Timer 1 enough time to run through its ISR and transition to the next movement without exceeding acceleration.
#define INITIAL_OCR1A 5000

//Contains information used by Timer 1 and the Prepper to calculate stepper timings.0
extern stepper_realtime_t StepperRealTime;

//Initialize the parameters for stepper motor operation.
//Initialize the parameters for stepper motor timer interrupts.
//Initialize the parameters required to perform stepper motor calculations.
void InitializeMovementBuffer();
void InitializeSteppers();

//Stops all stepper operations and wipes the movement buffer.
void AbortSteppers();

//Called by other functions to execute stepper motor movement.
//Initiates movement calculations and enables stepper timer interrupts.
void MoveSteppers(int32_t xSteps, int32_t ySteps, int32_t zSteps, int32_t eSteps, float exitSpeed, uint8_t doValvePrint);

//Begin stepper operations if applicable.
//Returns TRUE if stepper protocol was executed.
//Returns FALSE if not.
uint8_t ExecuteStepperProtocol();

//Resume stepper operations after pausing.
//Returns TRUE if stepper protocol was executed.
//Returns FALSE if not.
uint8_t ResumeStepperProtocol();

//Send a serial message with the numbers of steps taken before a limit switch was hit.
void ReportMovementLimit(int32_t xStepsTaken, int32_t yStepsTaken, int32_t zStepsTaken, int32_t eStepsTaken, int8_t xLimit, int8_t yLimit, int8_t zLimit, int8_t eLimit);

#endif