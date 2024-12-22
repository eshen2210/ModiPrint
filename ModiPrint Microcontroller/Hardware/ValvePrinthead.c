#include "ModiPrintMicro.h"

//Since Timer 3 is limited to 16-bit storage, the range of Timer 3 can be artificially increased with what is essentially another prescaler.
//The ValveOpenTime parameter will be divided by the prescaler and then by the maximum value of a 16-bit integer.
//The result is the number of maximum length Timer 3 cycles required to complete the value open operation. This result is out ValveTimerCycles value.
//The remainder of the division is our ValveTimerRemainder value.
uint16_t ValveTimerCycles;
uint16_t ValveTimerRemainder;

void InitializeValvePrinthead()
{
	valve_printhead_t * valvePrinthead = &Hardware.ValvePrinthead;
	
	valvePrinthead->ValvePin = PIN_ID_NULL;

	//Timer 3 (16-bit timer) settings.
	//Timer 3 drives the timing of the valves.
	TCCR3B |= (1 << WGM32); //Set Timer 3 to Compare Match (CTC) mode.
	TCCR3A &= ~((1 << WGM31) | (1 << WGM30));
	TCCR3B &= ~(1 << WGM33);
	TCCR3B &= ~((1 << CS30) | (1 << CS31) | (1 << CS32)); //Ensure Timer 3 is not ticking.
	TCCR3A &= ~((1 << COM3A1) | (1 << COM3A0) | (1 << COM3B1) | (1 << COM3B0)); //Disable external outputs for Timer 3.
	TIMSK3 |= (1 << OCIE3A); //Enables compare-driven interrupts by OCR3A.
}

void OperateSetValvePrinthead(uint8_t valvePin)
{
	valve_printhead_t * valvePrinthead = &Hardware.ValvePrinthead;
	
	//Input arguments must be valid.
	//Else error is handled.
	if (IsPinValid(valvePin) == TRUE)
	{
		//Send serial message that this command has been queued.
		ReportSetValvePrinthead(valvePin);
		
		WaitForHardwareFree(); //Wait until all hardware components are no longer taking action.
		System.HardwareBusy = TRUE; //Set system flag that notifies that hardware components are taking action.

		//Set all parameters in this Valve Printhead.
		valvePrinthead->ValvePin = valvePin;
		
		//Set valve Pin to output.
		*PinList[valvePin].DirectionAddress |= PinList[valvePin].DirectionMask;
		
		Hardware.PrintheadType = PT_Valve;

		//Send serial message that this command has been complete
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

void OperateValvePrintWithoutMovement(uint32_t openTime)
{
	valve_printhead_t * valvePrinthead = &Hardware.ValvePrinthead;
	
	if ((valvePrinthead != NULL) && (openTime >= 0))
	{
		if (openTime > 0)
		{
			//Send serial message that this command has been queued.
			ReportValvePrintWithoutMovement(openTime);

			WaitForHardwareFree(); //Wait until all hardware components are no longer taking action.
			WaitForPause(); //Delay operations until system is not paused.
			System.HardwareBusy = TRUE; //Set system flag that notifies that hardware components are taking action.
			
			//Prime Timer 3 with the valve open time.
			//The openTime value is in a value of microseconds.
			//Therefore, the number of clock cycles per microseconds is twice the openTime value considering a prescaler of 8.
			//OCR3A contains a 16-bit integer and holds a maximum value of 65,535.
			float floatCycles = ((float)openTime * 2) / (UINT16_MAX);
			float floatRemainder = fmod(floatCycles, 1);
			ValveTimerCycles = (uint16_t)floor(floatCycles);
			ValveTimerRemainder = (uint16_t)(floatRemainder * UINT16_MAX);
			OCR3A = (ValveTimerCycles < 1) ? ValveTimerRemainder : UINT16_MAX; 

			//Open valve.
			//Timer 3 ISR will close the valve.
			*PinList[valvePrinthead->ValvePin].PortAddress |= PinList[valvePrinthead->ValvePin].PortMask;

			//Begin Timer 3.
			TCCR3B |= (1 << CS31); //Set prescaler to 8. At a clock cycle of 16 MHz, every tick of Timer 3 is 0.5 microseconds and the maximum time between Timer triggers is over 32000 microsecond.
			
			//Timer 3 will set HardwareBusy global flag to false once the task is complete.
		}
		else if (openTime == 0)
		{
			//Send serial message that this command has been queued.
			ReportValvePrintWithoutMovement(openTime);

			WaitForHardwareFree(); //Wait until all hardware components are no longer taking action.
			System.HardwareBusy = TRUE; //Set system flag that notifies that hardware components are taking action.
			
			//Open valve. Valve will be closed when a command specifies.
			*PinList[valvePrinthead->ValvePin].PortAddress |= PinList[valvePrinthead->ValvePin].PortMask;

			//Send serial message that this command has been complete.
			SerialWriteTaskCompleted();

			//Hardware task completed.
			System.HardwareBusy = FALSE;
		}
		else if (openTime < 0)
		{
			//Should not reach this point.
			HandleError(ERROR_SYNTAX_INVALID);
			return;
		}
	}
	else
	{
		HandleError(ERROR_SYNTAX_INVALID);
		return;
	}
}

//Timer 3 Compare Match A interrupt handler.
//Triggered when TCNT3 ticks until it matches OCR3A.
//Initialized and fired by OperateValvePrint function.
ISR(TIMER3_COMPA_vect)
{
	if (ValveTimerCycles < 1)
	{
		//Stop Timer 3.
		TCCR3B &= ~((1 << CS30) | (1 << CS31) | (1 << CS32));

		//Close valve.
		*PinList[Hardware.ValvePrinthead.ValvePin].PortAddress &= ~PinList[Hardware.ValvePrinthead.ValvePin].PortMask;

		//Send serial message that this command has been complete
		SerialISRWriteTaskCompleted();

		//Hardware task completed.
		System.HardwareBusy = FALSE;
	}
	else
	{
		ValveTimerCycles--;
		OCR3A = (ValveTimerCycles < 1) ? ValveTimerRemainder : UINT16_MAX;
	}
}

void OperateValvePrintWithMovement(int32_t xSteps, int32_t ySteps, int32_t zSteps, float exitSpeed)
{
	valve_printhead_t * valvePrinthead = &Hardware.ValvePrinthead;
	
	//Input arguments must be valid.
	//Else error is handled.
	if ((valvePrinthead != NULL)
	 && (IsXYZSet(xSteps, ySteps, zSteps) == TRUE))
	{
		//Send serial message that this command has been queued.
		ReportValvePrintWithMovement(xSteps, ySteps, zSteps);
		
		//Synchronizing and waiting for other Hardware is handled by Stepper functions.

		//Queue command to the stepper operations buffer.
		//The stepper buffer will send the serial message signifying task complete.
		MoveSteppers(xSteps, ySteps, zSteps, 0, exitSpeed, TRUE);
	}
	else
	{
		HandleError(ERROR_SYNTAX_INVALID);
		return;
	}
}

void OperateStopValvePrint()
{
	valve_printhead_t * valvePrinthead = &Hardware.ValvePrinthead;
	
	//Send serial message that this command has been queued.
	ReportStopValvePrint();

	WaitForHardwareFree(); //Wait until all hardware components are no longer taking action.
	System.HardwareBusy = TRUE; //Set system flag that notifies that hardware components are taking action.
	
	//Close valve.
	*PinList[valvePrinthead->ValvePin].PortAddress &= ~PinList[valvePrinthead->ValvePin].PortMask;

	//Send serial message that this command has been complete
	SerialWriteTaskCompleted();

	//Hardware task completed.
	System.HardwareBusy = FALSE;
}

void ReportSetValvePrinthead(uint8_t valvePin)
{
	//Contains the message this program will write after executing a command.
	char transmitMessage[SERIAL_TRANSMIT_BUFFER_SIZE] = { NULL_CHAR };
	
	uint8_t transmitIndex = 0;
	
	//"@SetVlvPh("
	transmitMessage[transmitIndex++] = SERIAL_TASK_QUEUED_CHAR;
	transmitMessage[transmitIndex++] = 'S';
	transmitMessage[transmitIndex++] = 'e';
	transmitMessage[transmitIndex++] = 't';
	transmitMessage[transmitIndex++] = 'V';
	transmitMessage[transmitIndex++] = 'l';
	transmitMessage[transmitIndex++] = 'v';
	transmitMessage[transmitIndex++] = 'P';
	transmitMessage[transmitIndex++] = 'h';
	transmitMessage[transmitIndex++] = '(';

	//Valve Pin.
	ReporterAppendNumber(valvePin, transmitMessage, &transmitIndex);

	//")"
	transmitMessage[transmitIndex] = ')';

	SerialWriteString(transmitMessage);
}

void ReportValvePrintWithoutMovement(uint32_t openTime)
{
	//Contains the message this program will write after executing a command.
	char transmitMessage[SERIAL_TRANSMIT_BUFFER_SIZE] = { NULL_CHAR };
	
	uint8_t transmitIndex = 0;
		
	//"@PrnVlv("
	transmitMessage[transmitIndex++] = SERIAL_TASK_QUEUED_CHAR;
	transmitMessage[transmitIndex++] = 'P';
	transmitMessage[transmitIndex++] = 'r';
	transmitMessage[transmitIndex++] = 'n';
	transmitMessage[transmitIndex++] = 'V';
	transmitMessage[transmitIndex++] = 'l';
	transmitMessage[transmitIndex++] = 'v';
	
	if (openTime > 0)
	{
		//"("
		transmitMessage[transmitIndex++] = '(';

		ReporterAppendNumber(openTime, transmitMessage, &transmitIndex);
		
		//")"
		transmitMessage[transmitIndex] = ')';
	}

	SerialWriteString(transmitMessage);
}

void ReportValvePrintWithMovement(int32_t xSteps, int32_t ySteps, int32_t zSteps)
{
	//Contains the message this program will write after executing a command.
	char transmitMessage[SERIAL_TRANSMIT_BUFFER_SIZE] = { NULL_CHAR };
	
	uint8_t transmitIndex = 0;
		
	//"@MvPrnVlv("
	transmitMessage[transmitIndex++] = SERIAL_TASK_QUEUED_CHAR;
	transmitMessage[transmitIndex++] = 'M';
	transmitMessage[transmitIndex++] = 'v';
	transmitMessage[transmitIndex++] = 'P';
	transmitMessage[transmitIndex++] = 'r';
	transmitMessage[transmitIndex++] = 'n';
	transmitMessage[transmitIndex++] = 'V';
	transmitMessage[transmitIndex++] = 'l';
	transmitMessage[transmitIndex++] = 'v';
	transmitMessage[transmitIndex++] = '(';
	
	//xSteps.
	ReporterAppendNumber(xSteps, transmitMessage, &transmitIndex);
	ReporterNextNumber(transmitMessage, &transmitIndex);

	//ySteps.
	ReporterAppendNumber(ySteps, transmitMessage, &transmitIndex);
	ReporterNextNumber(transmitMessage, &transmitIndex);

	//zSteps.
	ReporterAppendNumber(zSteps, transmitMessage, &transmitIndex);

	//")"
	transmitMessage[transmitIndex] = ')';

	SerialWriteString(transmitMessage);
}

void ReportStopValvePrint()
{
	//Contains the message this program will write after executing a command.
	char transmitMessage[SERIAL_TRANSMIT_BUFFER_SIZE] = { NULL_CHAR };
	
	uint8_t transmitIndex = 0;

	//"@StpVlv"
	transmitMessage[transmitIndex++] = SERIAL_TASK_QUEUED_CHAR;
	transmitMessage[transmitIndex++] = 'S';
	transmitMessage[transmitIndex++] = 't';
	transmitMessage[transmitIndex++] = 'p';
	transmitMessage[transmitIndex++] = 'V';
	transmitMessage[transmitIndex++] = 'l';
	transmitMessage[transmitIndex] = 'v';

	SerialWriteString(transmitMessage);
}