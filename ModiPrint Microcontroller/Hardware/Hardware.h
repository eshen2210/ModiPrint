#ifndef HARDWARE_H_
#define HARDWARE_H_

//What type of Printhead is currently being used?
enum PRINTHEAD_TYPE{
	PT_Unset = 0,
	PT_Motor = 1,
	PT_Valve = 2,
	PT_Other = 3
};

//If a limit switch is pressed, this value will be set.
//If the limit switch is pressed while moving in the negative direction, LS_Min is set.
//If the limit switch is pressed while moving in the positive direction, LS_Max is set.
enum LIMIT_SWITCH_STATE{
	LS_Unset = 0,
	LS_Min = 1,
	LS_Max = 2
};

#include "Pauser.h"
#include "Axis.h"
#include "XYZStage.h"
#include "MotorPrinthead.h"
#include "ValvePrinthead.h"
#include "Pin.h"
#include "Steppers.h"

//Collection of hardware structs.
//These structs contain parameters associated with the physical components of this printer.
typedef struct{
	
	xyz_stage_t XYZStage;

	enum PRINTHEAD_TYPE PrintheadType;

	motor_printhead_t MotorPrinthead;
	valve_printhead_t ValvePrinthead;

}hardware_t;
extern hardware_t Hardware;

//Sets the defaults values of all hardware by calling their initialize functions.
void InitializeHardware();

#endif