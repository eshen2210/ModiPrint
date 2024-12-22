#include "ModiPrintMicro.h"

hardware_t Hardware;

void InitializeHardware()
{
	InitializeAxis(&Hardware.XYZStage.XAxis);
	InitializeAxis(&Hardware.XYZStage.YAxis);
	InitializeAxis(&Hardware.XYZStage.ZAxis);

	InitializeMotorPrinthead();
	InitializeValvePrinthead();

	InitializeSteppers();

	InitializePauser();
}
