#ifndef MOTORPRINTHEAD_H_
#define MOTORPRINTHEAD_H_

typedef struct{
	
	//GPIO Pin that drives the Printhead motor's step.
	uint8_t StepPin;

	//GPIO Pin that drives the Printhead motor's direction.
	uint8_t DirectionPin;

	//Duration of the signal that drives stepper motor steps (microseconds).
	//This value will be placed into an 8-bit timer.
	uint8_t StepPulseDuration;

	//GPIO Pin that receives input on the Printhead's limit switch.
	uint8_t LimitSwitchPin;

	//Maximum speed of the Printhead's stepper motor (steps/s).
	float MaxSpeed;

	//Maximum acceleration of the Printhead's stepper motor (steps/s^2).
	float MaxAcceleration;

	//States whether the limit switch is not pressed, the actuator is at its maximum range, or the actuator is at its minimum range.
	enum LIMIT_SWITCH_STATE LimitSwitchState;

}motor_printhead_t;

//Set the default values of a MotorPrinthead.
void InitializeMotorPrinthead();

//Set parameters for a specified Motor Printhead.
//All hardware functions using that Motor Printhead will now use these parameters until otherwise specified.
//Returns false if arguments are invalid.
void OperateSetMotorPrinthead(uint8_t stepPin, uint8_t directionPin, uint16_t stepPulseDuration, uint8_t limitSwitchPin, float maxSpeed, float maxAcceleration);

//Print with a motor-driven Printhead.
void OperateMotorPrintWithoutMovement(int32_t printSteps);

//Print with a motor-driven Printhead while moving.
void OperateMotorPrintWithMovement(int32_t printSteps, int32_t xSteps, int32_t ySteps, int32_t zSteps, float exitSpeed);

//Send a serial message detailing the parameters of a Motor Printhead.
void ReportSetMotorPrinthead(uint8_t stepPin, uint8_t directionPin, uint16_t stepPulseDuration, uint8_t limitSwitchPin, float maxSpeed, float maxAcceleration);

//Send a serial message detailing the parameters motor-driven print.
void ReportMotorPrintWithoutMovement(int32_t printSteps);

//Send a serial message detailing the parameters motor-driven print with movement.
void ReportMotorPrintWithMovement(int32_t printSteps, int32_t xSteps, int32_t ySteps, int32_t zSteps);

#endif