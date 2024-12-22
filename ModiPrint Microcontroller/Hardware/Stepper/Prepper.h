#ifndef PREPPER_H_
#define PREPPER_H_

//The number of stepper movement segments that can be stored in the ring buffer where a movement is defined as one G01/G00 command.
//Each segment is defined as a portion of the movement
//Here, one index is left empty to distinguish between completely empty and completely full ring buffers. Therefore, a buffer size of 50 holds at most 49 segments.
//A larger buffer requires more memory but better prevents combinations of fast movements and complicated calculations from inhibiting the throughput of the entire system.
#define MOVEMENT_BUFFER_SIZE 90

//Values used within Timer 1 to operate stepper motor movement.
typedef struct{

	//The phase (acceleration/cruise/deceleration/etc) that the movement is currently at.
	enum VELOCITY_PROFILE_PHASE Phase;

	//The total number of steps that will need to be taken by the stepper with the most steps.
	//This stepper will always be stepped in each cycle of Timer 1.
	//Acceleration and timing will be calculated around this stepper.
	uint32_t LeadingStepsTotal;

	//The number of steps that has already been taken by the stepper with the most steps.
	uint32_t LeadingStepsTaken;

	//The number of steps taken by the leading stepper until cruising at max speed needs to occur.
	uint32_t LeadingStepsToCruise;

	//The number of steps taken by the leading stepper until deceleration needs to occur.
	uint32_t LeadingStepsToDeceleration;

	//The speed of the movement's beginning end.
	//Steps / s.
	float MovementEntrySpeed;
	float MovementExitSpeed; 

	//The inverse acceleration of the leading stepper.
	//In cycles^2 / step.
	//This value allows the program to reword 1 / (leadingAccelerationCycles * VirtualTotalCycles) into InvLeadingAccelerationCycles / VirtualTotalCycles.
	//Mathwise the results are the same, except the latter avoids surprisingly significant float rounding errors and saves a float multiplication operation.
	float InvLeadingAccelerationCycles;

	//Initial acceleration calculation parameter of the first step of a movement.
	float InitialVirtualTotalCycles;

	//The current number of Timer 1 cycles in between each step.
	float Cycles;

	//The total number of steps that will need to be taken during this movement.
	int32_t XTotalSteps;
	int32_t YTotalSteps;
	int32_t ZTotalSteps;
	int32_t ETotalSteps;

	//The counter used by the Bresenham algorithm.
	//At each timer cycle, this value is increased by the total steps of the corresponding stepper.
	//When this counter value is equal to or greater than LeadingStepsTotal, then step the corresponding stepper.
	//The Bresenham algorithm uses this method to rasterize lines without using floating point calculations.
	uint32_t XCounter;
	uint32_t YCounter;
	uint32_t ZCounter;
	uint32_t ECounter; 

	//The step pulse duration of all motors. In units of timer cycles (i.e. # counts of TCNTx where # == timer cycles).
	//Ideally, each stepper motor would have individualized step pulse durations. However, there isn't enough timers on the Mega 2560 to end four stepper pulses asynchronously.
	//The compromise is that all stepper motors will share the longest minimum pulse duration among all steppers.
	uint16_t StepPulseDelay;

	//This Pin is turned on immediately before this movement and turned off immediately after movement end.
	//Used for features such as valve printing or UV lights.
	uint8_t ValvePin;

}movement_t;

typedef struct{
	
	//Ring buffer for StepperRealTime. Each index corresponds to another G01/G00 movement command.
	//When head == tail, the buffer is empty.
	//When head == (tail + 1), the buffer is full.
	movement_t MovementBuffer[MOVEMENT_BUFFER_SIZE];
	volatile uint8_t BufferTail; //Indexes the earliest unprocessed element.
	uint8_t BufferHead; //Indexes the next empty element.

}stepper_realtime_t;

//Performs calculations that are required before the execution of the stepper timer interrupts and loads all the member variables of a stepperRealTimeData.
//This function essentially performs all necessary calculations that must occur before unleashing the main stepper timer.
void PrepareStepperRealTime(movement_t * movement, int32_t xSteps, int32_t ySteps, int32_t zSteps, int32_t eSteps, float movementExitSpeed, uint8_t doValvePrint);

#endif