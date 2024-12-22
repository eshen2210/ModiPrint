#include "ModiPrintMicro.h"

//Used as a counter to keep track of the number of ISRs before the pause time is achieved.
uint16_t PauseCounter;

//Length of time to pause.
//In units of milliseconds.
uint16_t PauseTime;

void InitializePauser()
{
	PauseCounter = 0;
	
	//Timer 2 (8-bit timer) settings.
	//Timer 2 drives the timing of the Pauser.
	TCNT2 = 0;
	TCCR2B |= (1 << WGM22); //Set Timer 2 to Compare Match (CTC) mode.
	TCCR2A &= ~((1 << WGM21) | (1 << WGM20));
	TCCR2B &= ~((1 << CS20) | (1 << CS21) | (1 << CS22)); //Ensure Timer 2 is not ticking (prescaler set to 0).
	TCCR2A &= ~((1 << COM2A1) | (1 << COM2A0) | (1 << COM2B1) | (1 << COM2B0)); //Disable external outputs for Timer 2.
	TIMSK2 |= (1 << OCIE2A); //Enables compare-driven interrupts by OCR2A.
}

void Pause(uint16_t pauseTime)
{
	//Pause time in units of milliseconds.
	
	//Reset counters.
	PauseCounter = 0;
	TCNT2 = 0;

	PauseTime = pauseTime;

	//At a prescaler of 1024, 155 timer cycles comes just under 10 milliseconds.
	OCR2A = 155; 
		
	//Begin Timer 2.
	//Set Timer 2 prescaler to 1024. 
	//At a clock cycle of 16 MHz, every tick of Timer 2 is 64 microseconds.
	//A millisecond passes every 15.625 ticks of Timer 2.
	//The maximum time between Timer triggers is 4.194 seconds.
	TCCR2B |= ((1 << CS20) | (1 << CS21) | (1 << CS22));

	while ((TCCR2B & ((1 << CS20) | (1 << CS21) | (1 << CS22))) != 0) { } //Do nothing (pause) until Timer 2 is shut off.
}

ISR(TIMER2_COMPA_vect)
{
	PauseCounter += 10;
	
	if (PauseCounter == PauseTime)
	{
		//Shut off Timer 2 and resume normal operations.
		TCCR2B &= ~((1 << CS20) | (1 << CS21) | (1 << CS22)); //Prescaler set to 0.
	}
}
