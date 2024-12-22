#ifndef AXIS_H_
#define AXIS_H_

//Value for AxisName in Axis.
#define AXIS_NAME_NULL '\0' //For null or invalid Axis.
#define AXIS_NAME_X 'X' //For X Axis.
#define AXIS_NAME_Y 'Y' //For Y Axis.
#define AXIS_NAME_Z 'Z' //For Z Axis.

typedef struct{

	//GCode identifier for this Axis.
	char AxisName;
	
	//Identifier for the GPIO pin that drives each step.
	uint8_t StepPin;

	//Identifier for the GPIO pin that drives the direction.
	uint8_t DirectionPin;

	//Duration of the signal that drives stepper motor steps (microseconds).
	//This value will be placed into an 8-bit timer.
	uint8_t StepPulseDuration;

	//Identifier for the GPIO pin that signals the limit of movement.
	uint8_t LimitSwitchPin;

	//Maximum speed of the actuator (steps/s).
	float MaxSpeed;

	//Maximum acceleration of the actuator (steps/s^2).
	float MaxAcceleration;

	//States whether the limit switch is not pressed, the actuator is at its maximum range, or the actuator is at its minimum range.
	enum LIMIT_SWITCH_STATE LimitSwitchState;

}axis_t;

//Set the default values of an Axis.
void InitializeAxis(axis_t * axis);

//Set parameters for a specified Axis.
//All hardware functions using that Axis will now use these parameters until otherwise specified.
//Returns false if arguments are invalid.
void OperateSetAxis(axis_t * axis, char axisName, uint8_t stepPin, uint8_t directionPin, uint16_t stepPulseDuration, uint8_t limitSwitchPin, float maxSpeed, float maxAcceleration);

//Fill a char array with a message detailing the parameters of an Axis.
void ReportSetAxis(char axisName, uint8_t stepPin, uint8_t directionPin, uint8_t stepPulseDuration, uint8_t limitSwitchPin, float maxSpeed, float maxAcceleration);

#endif