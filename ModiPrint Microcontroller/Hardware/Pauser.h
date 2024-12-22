#ifndef PAUSER_H_
#define PAUSER_H_

//Initialize the parameters for pauser timer interrupts.
void InitializePauser();

//Suspend operations until the specified amount of time has passed.
//Pause time in units of milliseconds.
void Pause(uint16_t pauseTime);

#endif