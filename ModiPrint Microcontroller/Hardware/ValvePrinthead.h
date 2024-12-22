#ifndef VALVEPRINTHEAD_H_
#define VALVEPRINTHEAD_H_

typedef struct{
	
	//GPIO Pin that signals the opening and closing of the valve.
	uint8_t ValvePin;

}valve_printhead_t;

void InitializeValvePrinthead();

void OperateSetValvePrinthead(uint8_t valvePin);

void OperateValvePrintWithoutMovement(uint32_t openTime);

void OperateValvePrintWithMovement(int32_t xSteps, int32_t ySteps, int32_t zSteps, float exitSpeed);

void OperateStopValvePrint();

void ReportSetValvePrinthead(uint8_t valvePin);

void ReportValvePrintWithoutMovement(uint32_t openTime);

void ReportValvePrintWithMovement(int32_t xSteps, int32_t ySteps, int32_t zSteps);

void ReportStopValvePrint();

#endif