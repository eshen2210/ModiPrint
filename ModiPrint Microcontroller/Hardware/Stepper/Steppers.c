#include "ModiPrintMicro.h"

stepper_realtime_t StepperRealTime;

//How acceleration is calculated:
//The acceleration formula estimates velocity with v_n = cycles_total_(n-1) * a.
//This velocity is estimated with a time value of the previous step to avoid square roots.
//The number of cycles in between steps is estimated with cycles_n = 1 / v_n.
//Cycles is not calculated in a more conventional way ((v_n - v_(n-1)) / a) because that yields cycle values of zero considering previous estimations.
//Therefore, cycles are estimated with 1 / v_n, which introduces negligible error past the first estimated cycle which can be easily by initializing values with alternate equations.
//Note that the error will increase with higher accelerations.
//But reasonable movement parameters will yield a few % error at most where error decreases with increasing steps.
//Because error decreases with increasing steps, the cycles total value is kept across movements.
//These janky but simple calculations create an almost linear acceleration curve with only a single float divide per OCR1A value.
//The goal is to enable this program to perform all acceleration calculations on demand without the limitation of buffers to hold these values.
//Note these values are sometimes not even close to the actual values they represent.
//Yet, these values usually calculate a OCR1A value close enough to the real thing.

//The total cycles estimate.
//This value is incremented and decremented throughout the acceleration calculations of the movement.
//This value is remembered to circumvent initial errors in the acceleration calculations of the next movement.
//DEBUGTEMP as signed to check for negative values.
float VirtualTotalCycles;

//Flagged by Timer 1 when Timer 1 is running.
//Flagged false once a step has been executed.
//Prevents Timer 1 from triggering itself.
volatile uint8_t IsStepping = FALSE;

//Note: For System.HardwareBusy
//Flagged true whenever real time movement is occurring.
//Flagged false once all movements in the buffer have been executed.
//Prevents functions from overriding Timer 1's clock when a movement is in process.

void InitializeMovementBuffer()
{
	//Default values.
	StepperRealTime.BufferTail = 0; //Data buffer is empty
	StepperRealTime.BufferHead = 0;
	VirtualTotalCycles = 0;

	//Initial data buffer values.
	for (int i = 0; i < MOVEMENT_BUFFER_SIZE; i++)
	{
		StepperRealTime.MovementBuffer[i].MovementExitSpeed = 0;
	}
}

void InitializeSteppers()
{
	InitializeMovementBuffer();

	//Timer 1 (16-bit timer) settings.
	//Timer 1 drives the timing of the stepper motors.
	TCNT1 = 0;
	TCCR1B &= ~((1 << CS10) | (1 << CS11) | (1 << CS12)); //Ensure Timer 1 is not ticking (prescaler set to 0).
	TCCR1B |= (1 << WGM12); //Set Timer 1 to Compare Match (CTC) mode.
	TCCR1A &= ~((1 << WGM11) | (1 << WGM10)); 
	TCCR1B &= ~(1 << WGM13); 
	TCCR1A &= ~((1 << COM1A1) | (1 << COM1A0) | (1 << COM1B1) | (1 << COM1B0)); //Disable external outputs for Timer 1.
	TIMSK1 |= (1 << OCIE1A); //Enables compare-driven interrupts by OCR1A.

	//Timer 0 (8-bit timer) settings.
	//Timer 0 drives the timing of the stepper motors.
	TCNT0 = 0;
	TCCR0B |= (1 << WGM02); //Set Timer 0 to Compare Match (CTC) mode.
	TCCR0A &= ~((1 << WGM01) | (1 << WGM00));
	TCCR0B &= ~((1 << CS00) | (1 << CS01) | (1 << CS02)); //Ensure Timer 0 is not ticking (prescaler set to 0).
	TCCR0A &= ~((1 << COM0A1) | (1 << COM0A0) | (1 << COM0B1) | (1 << COM0B0)); //Disable external outputs for Timer 0.
	TIMSK0 |= (1 << OCIE0A); //Enables compare-driven interrupts by OCR0A.
}

void AbortSteppers()
{
	//Stop Timer 1.
	//If applicable, Timer 0 should shut off on its own after disabling the step pins.
	TCCR1B &= ~((1 << CS10) | (1 << CS11) | (1 << CS12));
	
	//Shut off the Valve Pin.
	movement_t * movement = &StepperRealTime.MovementBuffer[StepperRealTime.BufferTail];
	*PinList[movement->ValvePin].PortAddress &= ~PinList[movement->ValvePin].PortMask;
	
	//Wipe the movement buffer.
	InitializeMovementBuffer();

	//Reset flags to default.
	System.HardwareBusy = FALSE;
	IsStepping = FALSE;
}

void MoveSteppers(int32_t xSteps, int32_t ySteps, int32_t zSteps, int32_t eSteps, float exitSpeed, uint8_t doValvePrint)
{
	//Prepare values that need to be precalculated ahead of the real time calculations performed by the main stepper timers.
	PrepareStepperRealTime(&StepperRealTime.MovementBuffer[StepperRealTime.BufferHead], xSteps, ySteps, zSteps, eSteps, exitSpeed, doValvePrint);

	//Next head.
	uint8_t nextHead = StepperRealTime.BufferHead + 1;
	if (nextHead == MOVEMENT_BUFFER_SIZE) { nextHead = 0; } //Loops back to the beginning of the ring if it is the last element.

	//Set the next movement's entry speed.
	StepperRealTime.MovementBuffer[nextHead].MovementEntrySpeed = StepperRealTime.MovementBuffer[StepperRealTime.BufferHead].MovementExitSpeed;

	//If buffer is full, then wait until movements are processed.
	while (nextHead == StepperRealTime.BufferTail) 
	{ 
		//Movements should be processed by the stepper ISRs while nothing is occurring in this loop.
		CheckSystem();

		//In case the movement buffer was reset, exit out of this movement.
		if (StepperRealTime.BufferHead == StepperRealTime.BufferTail) { return; }
	}

	//Begin stepper operations if applicable.
	ExecuteStepperProtocol();

	//Update head.
	StepperRealTime.BufferHead = nextHead;
}

uint8_t ExecuteStepperProtocol()
{
	//If no hardware is in use and there is are stepper movements to be executed, then begin another movement.
	if ((System.HardwareBusy == FALSE)
	 && (System.StepperPaused == FALSE)
	 && (System.Pause == FALSE)
	 && (StepperRealTime.BufferHead != StepperRealTime.BufferTail)
	 && (System.Abort == FALSE))
	{
		//Set system flag that notifies that hardware components are taking action.
		//Prevent this function from modifying the main stepper timer (Timer 1) until all movements in buffer is complete.
		//Prevents other functions from modifying stepper parameters until all movements in buffer is complete.
		System.HardwareBusy = TRUE;

		//Begin Timer 1.
		OCR1A = INITIAL_OCR1A; //Fire the stepper timer.
		TCCR1B |= ((1 << CS11) | (1 << CS10)); //Set prescaler to 64. At a clock cycle of 16 MHz, every tick of Timer 1 is 4 microseconds and the maximum time between Timer triggers is just over 0.25 seconds.

		return TRUE;
	}
	else
	{
		return FALSE;
	}
}

uint8_t ResumeStepperProtocol()
{
	if ((System.StepperPaused == TRUE)
	&& (System.Pause == FALSE)
	&& (StepperRealTime.BufferHead != StepperRealTime.BufferTail)
	&& (System.Abort == FALSE))
	{
		System.StepperPaused = FALSE;
		
		//Resume stepper time.
		TCCR1B |= ((1 << CS11) | (1 << CS10));

		return TRUE;
	}
	else
	{
		return FALSE;
	}
}

//Timer 1 Compare Match A interrupt handler.
//Triggered when Timer 1 OCR1A ticks to match TCNT1.
//To Do: All cycle per section values need to be retested.
ISR(TIMER1_COMPA_vect)
{			
	//Timer 1 ISR takes ~1420 clock cycles (~10 us).
	//This interrupt takes an additional ~500 cycles for the first step of each movement.
	//Empirical testing shows that the max speed that can be handled by this program is just over 10,000 steps/s.
	
	//To Improve: This entire Timer protocol can be trimmed down. 
	//There should be another bootleg thread of stepper calculations running in parallel to the main protocol and hardware operations.
	//All calculations of the next OCR1A can be offloaded to the new intermediate bootleg thread.

	//Section 1 Initial Check.
	//91 cycles.
	//Note: It takes 69 cycles to enter this ISR. Why???

	//Prevents Timer 1 from triggering itself multiple times.
	//Timer 1 may trigger its own ISR unintentionally if the maximum speed is set too high.
	if (IsStepping == TRUE) { 
	TCCR1B &= ~((1 << CS10) | (1 << CS11) | (1 << CS12)); //Ensure Timer 1 is not ticking so that the error messages can get across.
	HandleError(ERROR_MINIMUM_CYCLES_EXCEEDED);
	//So, if I resume stepper timers, it could end up in such a fast repeating loop that serial messages cannot fire.
	//However, if I shut this down, stepper operations will not resume and the entire print stops.
	//TCCR1B |= ((1 << CS11) | (1 << CS10)); //Resume stepper timer.

	return; }
	IsStepping = TRUE;

	//Section 2: First Step Settings.
	//1st Step: 351 Cycles
	//Every Step After: 15 Cycles

	movement_t * movement = &StepperRealTime.MovementBuffer[StepperRealTime.BufferTail];

	//If this movement has just begun...
	if (movement->LeadingStepsTaken == 0)
	{	
		//Fire Timer 0 after step pulse time.
		//Timer 0 turns the step pins off.
		OCR0A = movement->StepPulseDelay; 

		//E direction.
		uint8_t eDirectionPinID = Hardware.MotorPrinthead.DirectionPin;
		if (movement->ETotalSteps > 0)
		{
			*PinList[eDirectionPinID].PortAddress |= PinList[eDirectionPinID].PortMask;
		}
		else
		{
			*PinList[eDirectionPinID].PortAddress &= ~PinList[eDirectionPinID].PortMask;
		}

		//X direction.
		uint8_t xDirectionPinID = Hardware.XYZStage.XAxis.DirectionPin;
		if (movement->XTotalSteps > 0)
		{
			*PinList[xDirectionPinID].PortAddress |= PinList[xDirectionPinID].PortMask;
		}
		else
		{
			*PinList[xDirectionPinID].PortAddress &= ~PinList[xDirectionPinID].PortMask;
		}

		//Y direction.
		uint8_t yDirectionPinID = Hardware.XYZStage.YAxis.DirectionPin;
		if (movement->YTotalSteps > 0)
		{
			*PinList[yDirectionPinID].PortAddress |= PinList[yDirectionPinID].PortMask;
		}
		else
		{
			*PinList[yDirectionPinID].PortAddress &= ~PinList[yDirectionPinID].PortMask;
		}

		//Z direction.
		uint8_t zDirectionPinID = Hardware.XYZStage.ZAxis.DirectionPin;
		if (movement->ZTotalSteps > 0)
		{
			*PinList[zDirectionPinID].PortAddress |= PinList[zDirectionPinID].PortMask;
		}
		else
		{
			*PinList[zDirectionPinID].PortAddress &= ~PinList[zDirectionPinID].PortMask;
		}

		//Toggle on the Valve Pin.
		if (movement->ValvePin != PIN_ID_NULL)
		{
			*PinList[movement->ValvePin].PortAddress |= PinList[movement->ValvePin].PortMask;
		}

		//Initial acceleration calculation parameters.
		VirtualTotalCycles = movement->InitialVirtualTotalCycles;
	}

	//Section 3: Increment Bresenham Counters.
	//151-154 Cycles

	//Tick the Bresenham counters.
	movement->ECounter += (movement->ETotalSteps >= 0) ? movement->ETotalSteps : (-1 * movement->ETotalSteps);
	movement->XCounter += (movement->XTotalSteps >= 0) ? movement->XTotalSteps : (-1 * movement->XTotalSteps);
	movement->YCounter += (movement->YTotalSteps >= 0) ? movement->YTotalSteps : (-1 * movement->YTotalSteps);
	movement->ZCounter += (movement->ZTotalSteps >= 0) ? movement->ZTotalSteps : (-1 * movement->ZTotalSteps);

	//Section 4: Toggle GPIO Pins.
	//221 Cycles.

	//If the Bresenham counter says its time to step the stepper...
	//Step E stepper.
	if (movement->ECounter >= movement->LeadingStepsTotal)
	{
		motor_printhead_t * motorPrinthead = &Hardware.MotorPrinthead;

		//See if this stepper has hit a limit switch.
		//To Do: Is the PIN_ID_NULL check necessary?
		if ((motorPrinthead->LimitSwitchPin != PIN_ID_NULL)
		 && (((*PinList[motorPrinthead->LimitSwitchPin].PinAddress) & (PinList[motorPrinthead->LimitSwitchPin].PinMask)) == 0)) //Active LO
		{			
			//If this stepper is stepping away from the Limit Switch, then enable its movement.
			if (((movement->ETotalSteps > 0) && (motorPrinthead->LimitSwitchState == LS_Min))
			 || ((movement->ETotalSteps < 0) && (motorPrinthead->LimitSwitchState == LS_Max)))
			{				
				goto EStep;
			}
			else
			{
				//Otherwise, take note of which Limit Switch was hit (min or max range).
				motorPrinthead->LimitSwitchState = (movement->ETotalSteps > 0) ? LS_Max : LS_Min;
				movement->Phase = VP_Limit;
			}
		}
		else
		{
			motorPrinthead->LimitSwitchState = LS_Unset;
			
			EStep:;
			
			//Step stepper.
			uint8_t eStepPinID = motorPrinthead->StepPin;
			*PinList[eStepPinID].PortAddress |= PinList[eStepPinID].PortMask;

			//Decrement counter.
			movement->ECounter -= movement->LeadingStepsTotal;
		}
	}

	//Step X stepper.
	if (movement->XCounter >= movement->LeadingStepsTotal)
	{
		axis_t * xAxis = &Hardware.XYZStage.XAxis;

		//See if this stepper has hit a limit switch.
		if ((xAxis->LimitSwitchPin != PIN_ID_NULL) 
		 && (((*PinList[xAxis->LimitSwitchPin].PinAddress) & (PinList[xAxis->LimitSwitchPin].PinMask)) == 0)) //Active LO
		{	
			//If this stepper is stepping away from the Limit Switch, then enable its movement.
			if (((movement->XTotalSteps > 0) && (xAxis->LimitSwitchState == LS_Min))
			 || ((movement->XTotalSteps < 0) && (xAxis->LimitSwitchState == LS_Max)))
			{				
				goto XStep;
			}
			else
			{
				//Otherwise, take note of which Limit Switch was hit (min or max range).
				xAxis->LimitSwitchState = (movement->XTotalSteps > 0) ? LS_Max : LS_Min;
				movement->Phase = VP_Limit;
			}
		}
		else
		{
			xAxis->LimitSwitchState = LS_Unset;
			
			XStep:;
			
			//Step stepper.
			uint8_t xStepPinID = xAxis->StepPin;
			*PinList[xStepPinID].PortAddress |= PinList[xStepPinID].PortMask;

			//Decrement counter.
			movement->XCounter -= movement->LeadingStepsTotal;
		}
	}

	//Step Y stepper.
	if (movement->YCounter >= movement->LeadingStepsTotal)
	{
		axis_t * yAxis = &Hardware.XYZStage.YAxis;

		//See if this stepper has hit a limit switch.
		if ((yAxis->LimitSwitchPin != PIN_ID_NULL)
		 && (((*PinList[yAxis->LimitSwitchPin].PinAddress) & (PinList[yAxis->LimitSwitchPin].PinMask)) == 0)) //Active LO
		{
			//If this stepper is stepping away from the Limit Switch, then enable its movement.
			if (((movement->YTotalSteps > 0) && (yAxis->LimitSwitchState == LS_Min))
			 || ((movement->YTotalSteps < 0) && (yAxis->LimitSwitchState == LS_Max)))
			{
				goto YStep;
			}
			else
			{
				//Otherwise, take note of which Limit Switch was hit (min or max range).
				yAxis->LimitSwitchState = (movement->YTotalSteps > 0) ? LS_Max : LS_Min;
				movement->Phase = VP_Limit;
			}
		}
		else
		{
			yAxis->LimitSwitchState = LS_Unset;

			YStep:;
			
			//Step stepper.
			uint8_t yStepPinID = yAxis->StepPin;
			*PinList[yStepPinID].PortAddress |= PinList[yStepPinID].PortMask;

			//Decrement counter.
			movement->YCounter -= movement->LeadingStepsTotal;
		}
	}

	//Step Z stepper.
	if (movement->ZCounter >= movement->LeadingStepsTotal)
	{
		axis_t * zAxis = &Hardware.XYZStage.ZAxis;

		//See if this stepper has hit a limit switch.
		if ((zAxis->LimitSwitchPin != PIN_ID_NULL)
		 && (((*PinList[zAxis->LimitSwitchPin].PinAddress) & (PinList[zAxis->LimitSwitchPin].PinMask)) == 0)) //Active LO
		{
			//If this stepper is stepping away from the Limit Switch, then enable its movement.
			if (((movement->ZTotalSteps > 0) && (zAxis->LimitSwitchState == LS_Min))
			 || ((movement->ZTotalSteps < 0) && (zAxis->LimitSwitchState == LS_Max)))
			{
				goto ZStep;
			}
			else
			{
				//Otherwise, take note of which Limit Switch was hit (min or max range).
				zAxis->LimitSwitchState = (movement->ZTotalSteps > 0) ? LS_Max : LS_Min;
				movement->Phase = VP_Limit;
			}
		}
		else
		{					
			zAxis->LimitSwitchState = LS_Unset;

			ZStep:;
			
			//Step stepper.
			uint8_t zStepPinID = zAxis->StepPin;
			*PinList[zStepPinID].PortAddress |= PinList[zStepPinID].PortMask;

			//Decrement counter.
			movement->ZCounter -= movement->LeadingStepsTotal;
		}
	}

	//Section 5: Set Timer 0.
	//35 Cycles.

	//Another step has been taken.
	movement->LeadingStepsTaken++;
	
	//Begin Timer 0.
	//Timer 0 ends the high state of the step pins.
	TCNT0 = 0;
	TCCR0B |= (1 << CS01); //Set Timer 0 prescaler to 8. At a clock cycle of 16 MHz, every tick of Timer 0 is 0.5 microseconds and the maximum time between Timer triggers is 128 us.
	sei(); //Reenable global interrupts (which are disabled upon entering any ISR). This allows Timer 0 to fire.

	//Section 6: Acceleration calculations.
	//828 Cycles.

	if (movement->Phase == VP_Acceleration) //If the stepper is currently accelerating...
	{
		movement->Cycles = (movement->InvLeadingAccelerationCycles / (VirtualTotalCycles));
		VirtualTotalCycles += movement->Cycles;
	}
	/* 
	else if (realTimeData->Phase == VP_Cruise) //If the stepper is currently cruising...
	{	
		//Do nothing. Cycles and VirtualTotalCycles value does not change.
	}
	*/
	else if (movement->Phase == VP_Deceleration) //If the stepper is currently decelerating...
	{
		//Deceleration does not perfectly mimic the reverse of acceleration.
		//Cycle values are slightly higher than intended during deceleration with these simple maths.
		VirtualTotalCycles -= movement->Cycles;
		movement->Cycles = (movement->InvLeadingAccelerationCycles / (VirtualTotalCycles));
	}

	//Section 7: Check Phase.
	//1st Step Acceleration: 209 Cycles. Why is the first check different?
	//Every Acceleration Step After: 66 Cycles.

	//See if this movement has completed acceleration, cruising, or deceleration.
	switch(movement->Phase)
	{
		case VP_Acceleration:			
			//If deceleration needs to occur..
			if (movement->LeadingStepsTaken >= movement->LeadingStepsToDeceleration)
			{
				movement->Phase = VP_Deceleration;
			}
			//If cruising needs to occur...
			else if (movement->LeadingStepsTaken >= movement->LeadingStepsToCruise)
			{								
				movement->Phase = VP_Cruise;
			}
			else if (movement->LeadingStepsTaken >= movement->LeadingStepsTotal)
			{
				//Catch the fringe case where the entire movement is acceleration.
				goto MovementEnd;
			}
			break;
		case VP_Cruise:			
			//If deceleration needs to occur..
			if (movement->LeadingStepsTaken >= movement->LeadingStepsToDeceleration)
			{
				movement->Phase = VP_Deceleration;
			}
			break;
		case VP_Deceleration:						
			//If deceleration and total execution of this movement is complete...
			if (movement->LeadingStepsTaken >= movement->LeadingStepsTotal)
			{														
				MovementEnd:;

				//End of movement.
				//Adds 200 cycles when transitioning to the next movement.

				//Increment buffer tail.
				StepperRealTime.BufferTail++;
				if (StepperRealTime.BufferTail == MOVEMENT_BUFFER_SIZE) { StepperRealTime.BufferTail = 0; } //Loops back to the beginning of the ring if it is the last element.

				//Send serial message that this command has been complete.
				SerialISRWriteTaskCompleted();

				//If there are no more movements to process...
				if (StepperRealTime.BufferTail == StepperRealTime.BufferHead)
				{	
					//Disable Timer 1.
					TCCR1B &= ~((1 << CS10) | (1 << CS11) | (1 << CS12)); //Ensure Timer 1 is not ticking.
					System.HardwareBusy = FALSE;

					//Toggle off the on-movement Pin.
					*PinList[movement->ValvePin].PortAddress &= ~PinList[movement->ValvePin].PortMask;
					
					IsStepping = FALSE;

					return;
				}
				else
				{
					//If the next movement is not a valve print, then toggle off the Valve Pin.
					movement_t * nextMovement = &StepperRealTime.MovementBuffer[StepperRealTime.BufferTail];
					if (nextMovement->ValvePin == PIN_ID_NULL) { *PinList[movement->ValvePin].PortAddress &= ~PinList[movement->ValvePin].PortMask;	}

					//Should proceed to set the OCR1A value that will lead to the next movement.
				}
			}
			break;
		case VP_Limit:
			//If a limit switch was hit...

			//Disable Timer 1.
			TCCR1B &= ~((1 << CS10) | (1 << CS11) | (1 << CS12)); //Ensure Timer 1 is not ticking.

			//Toggle off the on-movement Pin.
			*PinList[movement->ValvePin].PortAddress &= ~PinList[movement->ValvePin].PortMask;

			//Exit speed and acceleration calculation parameters are set to zero because of the stopped movement.
			movement->MovementExitSpeed = 0;
			VirtualTotalCycles = 0;

			//An extra LeadingStepsTaken was logged after hitting the limit switch.
			movement->LeadingStepsTaken--;

			//Report the number of steps that were executed before the limit switch was hit.
			int32_t eStepsTaken = 0;
			int32_t xStepsTaken = 0;
			int32_t yStepsTaken = 0;
			int32_t zStepsTaken = 0;
			eStepsTaken = (int32_t)(((float)(movement->LeadingStepsTaken) * movement->ETotalSteps) / movement->LeadingStepsTotal);
			xStepsTaken = (int32_t)(((float)(movement->LeadingStepsTaken) * movement->XTotalSteps) / movement->LeadingStepsTotal);
			yStepsTaken = (int32_t)(((float)(movement->LeadingStepsTaken) * movement->YTotalSteps) / movement->LeadingStepsTotal);
			zStepsTaken = (int32_t)(((float)(movement->LeadingStepsTaken) * movement->ZTotalSteps) / movement->LeadingStepsTotal);

			int8_t eLimit = (Hardware.MotorPrinthead.LimitSwitchState != LS_Unset) ? TRUE : FALSE;
			int8_t xLimit = (Hardware.XYZStage.XAxis.LimitSwitchState != LS_Unset) ? TRUE : FALSE;
			int8_t yLimit = (Hardware.XYZStage.YAxis.LimitSwitchState != LS_Unset) ? TRUE : FALSE;
			int8_t zLimit = (Hardware.XYZStage.ZAxis.LimitSwitchState != LS_Unset) ? TRUE : FALSE;
			ReportMovementLimit(eStepsTaken, xStepsTaken, yStepsTaken, zStepsTaken, xLimit, yLimit, zLimit, eLimit);

			//Increment buffer tail.
			StepperRealTime.BufferTail++;
			if (StepperRealTime.BufferTail == MOVEMENT_BUFFER_SIZE) { StepperRealTime.BufferTail = 0; } //Loops back to the beginning of the ring if it is the last element.
			movement = &StepperRealTime.MovementBuffer[StepperRealTime.BufferTail];

			//Waits for a moment to prevent actuators from "bouncing" and triggering limit switches twice.
			sei(); //Enable Timer 2 (the pause timer) to fire.
			Pause(100);

			//If there are no more movements to process...
			if (StepperRealTime.BufferTail == StepperRealTime.BufferHead)
			{
				System.HardwareBusy = FALSE;
			}
			else //Wait for resume command from the serial stream.
			{
				System.Pause = TRUE;
				System.StepperPaused = TRUE; 
			}

			IsStepping = FALSE;

			return;
			break;
		default:
			//Should never reach this point.
			break;
	}

	//Section 8: Set OCR1A.
	//17 Cycles

	OCR1A = (uint16_t)movement->Cycles;

	//Allows Timer 1 to execute again.
	IsStepping = FALSE;
}	

//Timer 0 Compare Match A interrupt handler.
//Triggered when TCNT0 ticks until it matches OCR0A.
//Initialized and fired by Timer 1's ISR.
ISR(TIMER0_COMPA_vect)
{
	//Section 9: Execute Timer 0.
	
	//Turn off all stepper step pins.
	uint8_t xStepPinID = Hardware.XYZStage.XAxis.StepPin;
	*PinList[xStepPinID].PortAddress &= ~PinList[xStepPinID].PortMask;

	uint8_t yStepPinID = Hardware.XYZStage.YAxis.StepPin;
	*PinList[yStepPinID].PortAddress &= ~PinList[yStepPinID].PortMask;

	uint8_t zStepPinID = Hardware.XYZStage.ZAxis.StepPin;
	*PinList[zStepPinID].PortAddress &= ~PinList[zStepPinID].PortMask;

	uint8_t eStepPinID = Hardware.MotorPrinthead.StepPin;
	*PinList[eStepPinID].PortAddress &= ~PinList[eStepPinID].PortMask;

	//Disable Timer 0.
	TCCR0B &= ~((1 << CS00) | (1 << CS01) | (1 << CS02)); 
}

void ReportMovementLimit(int32_t eStepsTaken, int32_t xStepsTaken, int32_t yStepsTaken, int32_t zStepsTaken, int8_t xLimit, int8_t yLimit, int8_t zLimit, int8_t eLimit)
{
	//Contains the status message that will be written through the serial port.
	char transmitMessage[SERIAL_TRANSMIT_BUFFER_SIZE] = { NULL_CHAR };
	
	uint8_t transmitIndex = 0;
	
	//"!Lmt"
	transmitMessage[transmitIndex++] = SERIAL_STATUS_CHAR;
	transmitMessage[transmitIndex++] = 'L';
	transmitMessage[transmitIndex++] = 'm';
	transmitMessage[transmitIndex++] = 't';

	//If a certain actuator has hit the limit switch, then insert the Axis ID here.
	//For example, if the Z Axis has hit the limit switch, insert 'Z' here.
	if (eLimit == TRUE)
	{
		transmitMessage[transmitIndex++] = 'E';
	}
	
	if (xLimit == TRUE)
	{
		transmitMessage[transmitIndex++] = 'X';
	}

	if (yLimit == TRUE)
	{
		transmitMessage[transmitIndex++] = 'Y';
	}

	if (zLimit == TRUE)
	{
		transmitMessage[transmitIndex++] = 'Z';
	}

	//"("
	transmitMessage[transmitIndex++] = '(';
	
	//E Steps Taken.
	ReporterAppendNumber(eStepsTaken, transmitMessage, &transmitIndex);
	ReporterNextNumber(transmitMessage, &transmitIndex);

	//X Steps Taken.
	ReporterAppendNumber(xStepsTaken, transmitMessage, &transmitIndex);
	ReporterNextNumber(transmitMessage, &transmitIndex);

	//Y Steps Taken.
	ReporterAppendNumber(yStepsTaken, transmitMessage, &transmitIndex);
	ReporterNextNumber(transmitMessage, &transmitIndex);

	//Z Steps Taken.
	ReporterAppendNumber(zStepsTaken, transmitMessage, &transmitIndex);

	//")"
	transmitMessage[transmitIndex] = ')';

	SerialISRWriteString(transmitMessage);
}

