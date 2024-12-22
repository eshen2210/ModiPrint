#include "ModiPrintMicro.h"

int main(void)
{
    Initialize();
	
	while (1) 
    {
		ProtocolLoop();
		PrepareNextLoop();
    }
}

//Not sure where to put this.
//Higher priority timers will have ISRs that override Timers in order of priority. From highest priority to lowest:
//1. Timer 2 Pauser: Halts all operations for some time.
//2. Timer 1 Stepper: Manages stepper motor timings.
//3. Timer 0 Stepper: Called by Timer 1 to turn off stepper motor pins.
//4. USART0 Serial: Serial communications, receiving and sending.
//5. Timer 3 Valve: Turns off the Valve pin.
//6. Main loop: Not an ISR. Loops through a series of flags to determine if something needs to be done. 


