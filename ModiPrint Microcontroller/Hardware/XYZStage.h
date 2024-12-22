#ifndef XYZSTAGE_H_
#define XYZSTAGE_H_

//Collection of axis_t structs for the XYZ stage.
typedef struct{

	axis_t XAxis;
	axis_t YAxis;
	axis_t ZAxis;

}xyz_stage_t;

//Returns true if parameters have been set for all Axes that are in use.
int8_t IsXYZSet(uint32_t xSteps, uint32_t ySteps, uint32_t zSteps);

//Moves the XYZStage Axes motors.
//Takes relative coordinates in units of steps.
//If transmitMessage is NULL, then do not fill any message.
void OperateMoveXYZ(int32_t xSteps, int32_t ySteps, int32_t zSteps, float exitSpeed);

//Fill a char array with a message detailing the parameters of XYZ Stage movement.
void ReportMoveXYZ(int32_t xSteps, int32_t ySteps, int32_t zSteps);

#endif