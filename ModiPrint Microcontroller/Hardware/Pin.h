#ifndef PIN_H_
#define PIN_H_

//Number of GPIO Pins on this microcontroller.
#define PIN_COUNT 70

//Pin identifier for null or invalid pins.
#define PIN_ID_NULL PIN_COUNT

//Contains the addresses and bit values for a GPIO Pin's port, mask, and direction.
typedef struct{
	
	//Each GPIO Pin has a three bits individually dedicated to its Direction, Input, and Port.
	//The address is the location for either the Port, Direction, or Input 8-bit Data Registers that each drives 8 GPIO Pins (one GPIO Pin for each bit).
	//The mask is the bit on the port that corresponds to this specific pin.
	
	//Address and mask for PORTxn, the Data Register for the Pin's output.
	uint8_t volatile * PortAddress;
	uint8_t PortMask;

	//Address and mask for DDRx, the Data Direction Register for whether the pin is output or input.
	uint8_t volatile * DirectionAddress;
	uint8_t DirectionMask;

	//Address and mask for PINxn, the Input Pins Address Pin's input.
	uint8_t volatile * PinAddress;
	uint8_t PinMask;

}pin_t;

//Contains port, mask, and direction addresses and bit values for all Pins.
extern pin_t PinList[PIN_COUNT + 1];

//Populates PinList;
void InitializePins();

//Returns true if microcontroller has a pin that corresponds with the pin identifier argument.
int8_t IsPinValid(uint8_t pinID);

#endif