//External tool arguments: -C"M:\M Programs\Arduino\hardware\tools\avr\etc\avrdude.conf" -patmega2560 -cwiring -P\\.\COM4 -b115200 -D -Uflash:w:"$(ProjectDir)Debug\$(TargetName).hex":i

#ifndef ModiPrintMICRO_H_
#define ModiPrintMICRO_H_

//Standard libraries.
#include <avr/io.h>
#include <avr/interrupt.h> //Required for ISRs.
#include <stdlib.h> //Standard library of common C functions.
#include <math.h> //Standard library for math related functions.
#include <float.h> //Also required for mathz.
#include <string.h>
#include <ctype.h>

//Header files.
#include "Globals.h"
#include "System.h"
#include "Serial.h"
#include "Hardware.h"
#include "Interpreter.h"
#include "Steppers.h"

//Set up before executing the program's main protocol.
void Initialize();

//A single cycle of the program's main protocol.
//Looped in the main method.
void ProtocolLoop();

//Resets certain variables for the next execution of the protocol loop.
//Executed after each protocol loop.
void PrepareNextLoop();

#endif