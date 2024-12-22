#include "ModiPrintMicro.h"

pin_t PinList[PIN_COUNT + 1];

void InitializePins()
{
		//ID		PortAddress	PortMask	DirAddress	DirMask		PinAddress	PinMask
		//0			PORTE		PE0			DDRE		DDE0		PINE		PINE0
		//1			PORTE		PE1			DDRE		DDE1		PINE		PINE1
		//2			PORTE		PE4			DDRE		DDE4		PINE		PINE4
		//3			PORTE		PE5			DDRE		DDE5		PINE		PINE5
		//4			PORTG		PG5			DDRG		DDG5		PING		PING5
		//5			PORTE		PE3			DDRE		DDE3		PINE		PINE3
		//6			PORTH		PH3			DDRH		DDH3		PINH		PINH3
		//7			PORTH		PH4			DDRH		DDH4		PINH		PINH4
		//8			PORTH		PH5			DDRH		DDH5		PINH		PINH5
		//9			PORTH		PH6			DDRH		DDH6		PINH		PINH6
		//10		PORTB		PB4			DDRB		DDB4		PINB		PINB4
		//11		PORTB		PB5			DDRB		DDB5		PINB		PINB5
		//12		PORTB		PB6			DDRB		DDB6		PINB		PINB6
		//13		PORTB		PB7			DDRB		DDB7		PINB		PINB7
		//14		PORTJ		PJ1			DDRJ		DDJ1		PINJ		PINJ1
		//15		PORTJ		PJ0			DDRJ		DDJ0		PINJ		PINJ0
		//16		PORTH		PH1			DDRH		DDH1		PINH		PINH1
		//17		PORTH		PH0			DDRH		DDH0		PINH		PINH0
		//18		PORTD		PD3			DDRD		DDD3		PIND		PIND3
		//19		PORTD		PD2			DDRD		DDD2		PIND		PIND2
		//20		PORTD		PD1			DDRD		DDD1		PIND		PIND1
		//21		PORTD		PD0			DDRD		DDD0		PIND		PIND0
		//22		PORTA		PA0			DDRA		DDA0		PINA		PINA0
		//23		PORTA		PA1			DDRA		DDA1		PINA		PINA1
		//24		PORTA		PA2			DDRA		DDA2		PINA		PINA2
		//25		PORTA		PA3			DDRA		DDA3		PINA		PINA3
		//26		PORTA		PA4			DDRA		DDA4		PINA		PINA4
		//27		PORTA		PA5			DDRA		DDA5		PINA		PINA5
		//28		PORTA		PA6			DDRA		DDA6		PINA		PINA6
		//29		PORTA		PA7			DDRA		DDA7		PINA		PINA7
		//30		PORTC		PC7			DDRC		DDC7		PINC		PINC7
		//31		PORTC		PC6			DDRC		DDC6		PINC		PINC6
		//32		PORTC		PC5			DDRC		DDC5		PINC		PINC5
		//33		PORTC		PC4			DDRC		DDC4		PINC		PINC4
		//34		PORTC		PC3			DDRC		DDC3		PINC		PINC3
		//35		PORTC		PC2			DDRC		DDC2		PINC		PINC2
		//36		PORTC		PC1			DDRC		DDC1		PINC		PINC1
		//37		PORTC		PC0			DDRC		DDC0		PINC		PINC0
		//38		PORTD		PD7			DDRD		DDD7		PIND		PIND7
		//39		PORTG		PG2			DDRG		DDG2		PING		PING2
		//40		PORTG		PG1			DDRG		DDG1		PING		PING1
		//41		PORTG		PG0			DDRG		DDG0		PING		PING0
		//42		PORTL		PL7			DDRL		DDL7		PINL		PINL7
		//43		PORTL		PL6			DDRL		DDL6		PINL		PINL6
		//44		PORTL		PL5			DDRL		DDL5		PINL		PINL5
		//45		PORTL		PL4			DDRL		DDL4		PINL		PINL4
		//46		PORTL		PL3			DDRL		DDL3		PINL		PINL3
		//47		PORTL		PL2			DDRL		DDL2		PINL		PINL2
		//48		PORTL		PL1			DDRL		DDL1		PINL		PINL1
		//49		PORTL		PL0			DDRL		DDL0		PINL		PINL0
		//50		PORTB		PB3			DDRB		DDB3		PINB		PINB3
		//51		PORTB		PB2			DDRB		DDB2		PINB		PINB2
		//52		PORTB		PB1			DDRB		DDB1		PINB		PINB1
		//53		PORTB		PB0			DDRB		DDB0		PINB		PINB0
		//54		PORTF		PF0			DDRF		DDF0		PINF		PINF0
		//55		PORTF		PF1			DDRF		DDF1		PINF		PINF1
		//56		PORTF		PF2			DDRF		DDF2		PINF		PINF2
		//57		PORTF		PF3			DDRF		DDF3		PINF		PINF3
		//58		PORTF		PF4			DDRF		DDF4		PINF		PINF4
		//59		PORTF		PF5			DDRF		DDF5		PINF		PINF5
		//60		PORTF		PF6			DDRF		DDF6		PINF		PINF6
		//61		PORTF		PF7			DDRF		DDF7		PINF		PINF7
		//62		PORTK		PK0			DDRK		DDK0		PINK		PINK0
		//63		PORTK		PK1			DDRK		DDK1		PINK		PINK1
		//64		PORTK		PK2			DDRK		DDK2		PINK		PINK2
		//65		PORTK		PK3			DDRK		DDK3		PINK		PINK3
		//66		PORTK		PK4			DDRK		DDK4		PINK		PINK4
		//67		PORTK		PK5			DDRK		DDK5		PINK		PINK5
		//68		PORTK		PK6			DDRK		DDK6		PINK		PINK6
		//69		PORTK		PK7			DDRK		DDK7		PINK		PINK7
		
		PinList[0 ].PortAddress = &PORTE; PinList[0 ].PortMask = (1 << PE0); PinList[0 ].DirectionAddress = &DDRE; PinList[0 ].DirectionMask = (1 << DDE0); PinList[0 ].PinAddress = &PINE; PinList[0 ].PinMask = (1 << PINE0);
		PinList[1 ].PortAddress = &PORTE; PinList[1 ].PortMask = (1 << PE1); PinList[1 ].DirectionAddress = &DDRE; PinList[1 ].DirectionMask = (1 << DDE1); PinList[1 ].PinAddress = &PINE; PinList[1 ].PinMask = (1 << PINE1);
		PinList[2 ].PortAddress = &PORTE; PinList[2 ].PortMask = (1 << PE4); PinList[2 ].DirectionAddress = &DDRE; PinList[2 ].DirectionMask = (1 << DDE4); PinList[2 ].PinAddress = &PINE; PinList[2 ].PinMask = (1 << PINE4);
		PinList[3 ].PortAddress = &PORTE; PinList[3 ].PortMask = (1 << PE5); PinList[3 ].DirectionAddress = &DDRE; PinList[3 ].DirectionMask = (1 << DDE5); PinList[3 ].PinAddress = &PINE; PinList[3 ].PinMask = (1 << PINE5);
		PinList[4 ].PortAddress = &PORTG; PinList[4 ].PortMask = (1 << PG5); PinList[4 ].DirectionAddress = &DDRG; PinList[4 ].DirectionMask = (1 << DDG5); PinList[4 ].PinAddress = &PING; PinList[4 ].PinMask = (1 << PING5);
		PinList[5 ].PortAddress = &PORTE; PinList[5 ].PortMask = (1 << PE3); PinList[5 ].DirectionAddress = &DDRE; PinList[5 ].DirectionMask = (1 << DDE3); PinList[5 ].PinAddress = &PINE; PinList[5 ].PinMask = (1 << PINE3);
		PinList[6 ].PortAddress = &PORTH; PinList[6 ].PortMask = (1 << PH3); PinList[6 ].DirectionAddress = &DDRH; PinList[6 ].DirectionMask = (1 << DDH3); PinList[6 ].PinAddress = &PINH; PinList[6 ].PinMask = (1 << PINH3);
		PinList[7 ].PortAddress = &PORTH; PinList[7 ].PortMask = (1 << PH4); PinList[7 ].DirectionAddress = &DDRH; PinList[7 ].DirectionMask = (1 << DDH4); PinList[7 ].PinAddress = &PINH; PinList[7 ].PinMask = (1 << PINH4);
		PinList[8 ].PortAddress = &PORTH; PinList[8 ].PortMask = (1 << PH5); PinList[8 ].DirectionAddress = &DDRH; PinList[8 ].DirectionMask = (1 << DDH5); PinList[8 ].PinAddress = &PINH; PinList[8 ].PinMask = (1 << PINH5);
		PinList[9 ].PortAddress = &PORTH; PinList[9 ].PortMask = (1 << PH6); PinList[9 ].DirectionAddress = &DDRH; PinList[9 ].DirectionMask = (1 << DDH6); PinList[9 ].PinAddress = &PINH; PinList[9 ].PinMask = (1 << PINH6);
		PinList[10].PortAddress = &PORTB; PinList[10].PortMask = (1 << PB4); PinList[10].DirectionAddress = &DDRB; PinList[10].DirectionMask = (1 << DDB4); PinList[10].PinAddress = &PINB; PinList[10].PinMask = (1 << PINB4);
		PinList[11].PortAddress = &PORTB; PinList[11].PortMask = (1 << PB5); PinList[11].DirectionAddress = &DDRB; PinList[11].DirectionMask = (1 << DDB5); PinList[11].PinAddress = &PINB; PinList[11].PinMask = (1 << PINB5);
		PinList[12].PortAddress = &PORTB; PinList[12].PortMask = (1 << PB6); PinList[12].DirectionAddress = &DDRB; PinList[12].DirectionMask = (1 << DDB6); PinList[12].PinAddress = &PINB; PinList[12].PinMask = (1 << PINB6);
		PinList[13].PortAddress = &PORTB; PinList[13].PortMask = (1 << PB7); PinList[13].DirectionAddress = &DDRB; PinList[13].DirectionMask = (1 << DDB7); PinList[13].PinAddress = &PINB; PinList[13].PinMask = (1 << PINB7);
		PinList[14].PortAddress = &PORTJ; PinList[14].PortMask = (1 << PJ1); PinList[14].DirectionAddress = &DDRJ; PinList[14].DirectionMask = (1 << DDJ1); PinList[14].PinAddress = &PINJ; PinList[14].PinMask = (1 << PINJ1);
		PinList[15].PortAddress = &PORTJ; PinList[15].PortMask = (1 << PJ0); PinList[15].DirectionAddress = &DDRJ; PinList[15].DirectionMask = (1 << DDJ0); PinList[15].PinAddress = &PINJ; PinList[15].PinMask = (1 << PINJ0);
		PinList[16].PortAddress = &PORTH; PinList[16].PortMask = (1 << PH1); PinList[16].DirectionAddress = &DDRH; PinList[16].DirectionMask = (1 << DDH1); PinList[16].PinAddress = &PINH; PinList[16].PinMask = (1 << PINH1);
		PinList[17].PortAddress = &PORTH; PinList[17].PortMask = (1 << PH0); PinList[17].DirectionAddress = &DDRH; PinList[17].DirectionMask = (1 << DDH0); PinList[17].PinAddress = &PINH; PinList[17].PinMask = (1 << PINH0);
		PinList[18].PortAddress = &PORTD; PinList[18].PortMask = (1 << PD3); PinList[18].DirectionAddress = &DDRD; PinList[18].DirectionMask = (1 << DDD3); PinList[18].PinAddress = &PIND; PinList[18].PinMask = (1 << PIND3);
		PinList[19].PortAddress = &PORTD; PinList[19].PortMask = (1 << PD2); PinList[19].DirectionAddress = &DDRD; PinList[19].DirectionMask = (1 << DDD2); PinList[19].PinAddress = &PIND; PinList[19].PinMask = (1 << PIND2);
		PinList[20].PortAddress = &PORTD; PinList[20].PortMask = (1 << PD1); PinList[20].DirectionAddress = &DDRD; PinList[20].DirectionMask = (1 << DDD1); PinList[20].PinAddress = &PIND; PinList[20].PinMask = (1 << PIND1);
		PinList[21].PortAddress = &PORTD; PinList[21].PortMask = (1 << PD0); PinList[21].DirectionAddress = &DDRD; PinList[21].DirectionMask = (1 << DDD0); PinList[21].PinAddress = &PIND; PinList[21].PinMask = (1 << PIND0);
		PinList[22].PortAddress = &PORTA; PinList[22].PortMask = (1 << PA0); PinList[22].DirectionAddress = &DDRA; PinList[22].DirectionMask = (1 << DDA0); PinList[22].PinAddress = &PINA; PinList[22].PinMask = (1 << PINA0);
		PinList[23].PortAddress = &PORTA; PinList[23].PortMask = (1 << PA1); PinList[23].DirectionAddress = &DDRA; PinList[23].DirectionMask = (1 << DDA1); PinList[23].PinAddress = &PINA; PinList[23].PinMask = (1 << PINA1);
		PinList[24].PortAddress = &PORTA; PinList[24].PortMask = (1 << PA2); PinList[24].DirectionAddress = &DDRA; PinList[24].DirectionMask = (1 << DDA2); PinList[24].PinAddress = &PINA; PinList[24].PinMask = (1 << PINA2);
		PinList[25].PortAddress = &PORTA; PinList[25].PortMask = (1 << PA3); PinList[25].DirectionAddress = &DDRA; PinList[25].DirectionMask = (1 << DDA3); PinList[25].PinAddress = &PINA; PinList[25].PinMask = (1 << PINA3);
		PinList[26].PortAddress = &PORTA; PinList[26].PortMask = (1 << PA4); PinList[26].DirectionAddress = &DDRA; PinList[26].DirectionMask = (1 << DDA4); PinList[26].PinAddress = &PINA; PinList[26].PinMask = (1 << PINA4);
		PinList[27].PortAddress = &PORTA; PinList[27].PortMask = (1 << PA5); PinList[27].DirectionAddress = &DDRA; PinList[27].DirectionMask = (1 << DDA5); PinList[27].PinAddress = &PINA; PinList[27].PinMask = (1 << PINA5);
		PinList[28].PortAddress = &PORTA; PinList[28].PortMask = (1 << PA6); PinList[28].DirectionAddress = &DDRA; PinList[28].DirectionMask = (1 << DDA6); PinList[28].PinAddress = &PINA; PinList[28].PinMask = (1 << PINA6);
		PinList[29].PortAddress = &PORTA; PinList[29].PortMask = (1 << PA7); PinList[29].DirectionAddress = &DDRA; PinList[29].DirectionMask = (1 << DDA7); PinList[29].PinAddress = &PINA; PinList[29].PinMask = (1 << PINA7);
		PinList[30].PortAddress = &PORTC; PinList[30].PortMask = (1 << PC7); PinList[30].DirectionAddress = &DDRC; PinList[30].DirectionMask = (1 << DDC7); PinList[30].PinAddress = &PINC; PinList[30].PinMask = (1 << PINC7);
		PinList[31].PortAddress = &PORTC; PinList[31].PortMask = (1 << PC6); PinList[31].DirectionAddress = &DDRC; PinList[31].DirectionMask = (1 << DDC6); PinList[31].PinAddress = &PINC; PinList[31].PinMask = (1 << PINC6);
		PinList[32].PortAddress = &PORTC; PinList[32].PortMask = (1 << PC5); PinList[32].DirectionAddress = &DDRC; PinList[32].DirectionMask = (1 << DDC5); PinList[32].PinAddress = &PINC; PinList[32].PinMask = (1 << PINC5);
		PinList[33].PortAddress = &PORTC; PinList[33].PortMask = (1 << PC4); PinList[33].DirectionAddress = &DDRC; PinList[33].DirectionMask = (1 << DDC4); PinList[33].PinAddress = &PINC; PinList[33].PinMask = (1 << PINC4);
		PinList[34].PortAddress = &PORTC; PinList[34].PortMask = (1 << PC3); PinList[34].DirectionAddress = &DDRC; PinList[34].DirectionMask = (1 << DDC3); PinList[34].PinAddress = &PINC; PinList[34].PinMask = (1 << PINC3); 
		PinList[35].PortAddress = &PORTC; PinList[35].PortMask = (1 << PC2); PinList[35].DirectionAddress = &DDRC; PinList[35].DirectionMask = (1 << DDC2); PinList[35].PinAddress = &PINC; PinList[35].PinMask = (1 << PINC2);
		PinList[36].PortAddress = &PORTC; PinList[36].PortMask = (1 << PC1); PinList[36].DirectionAddress = &DDRC; PinList[36].DirectionMask = (1 << DDC1); PinList[36].PinAddress = &PINC; PinList[36].PinMask = (1 << PINC1);
		PinList[37].PortAddress = &PORTC; PinList[37].PortMask = (1 << PC0); PinList[37].DirectionAddress = &DDRC; PinList[37].DirectionMask = (1 << DDC0); PinList[37].PinAddress = &PINC; PinList[37].PinMask = (1 << PINC0);
		PinList[38].PortAddress = &PORTD; PinList[38].PortMask = (1 << PD7); PinList[38].DirectionAddress = &DDRD; PinList[38].DirectionMask = (1 << DDD7); PinList[38].PinAddress = &PIND; PinList[38].PinMask = (1 << PIND7);
		PinList[39].PortAddress = &PORTG; PinList[39].PortMask = (1 << PG2); PinList[39].DirectionAddress = &DDRG; PinList[39].DirectionMask = (1 << DDG2); PinList[39].PinAddress = &PING; PinList[39].PinMask = (1 << PING2);
		PinList[40].PortAddress = &PORTG; PinList[40].PortMask = (1 << PG1); PinList[40].DirectionAddress = &DDRG; PinList[40].DirectionMask = (1 << DDG1); PinList[40].PinAddress = &PING; PinList[40].PinMask = (1 << PING1);
		PinList[41].PortAddress = &PORTG; PinList[41].PortMask = (1 << PG0); PinList[41].DirectionAddress = &DDRG; PinList[41].DirectionMask = (1 << DDG0); PinList[41].PinAddress = &PING; PinList[41].PinMask = (1 << PING0);
		PinList[42].PortAddress = &PORTL; PinList[42].PortMask = (1 << PL7); PinList[42].DirectionAddress = &DDRL; PinList[42].DirectionMask = (1 << DDL7); PinList[42].PinAddress = &PINL; PinList[42].PinMask = (1 << PINL7);
		PinList[43].PortAddress = &PORTL; PinList[43].PortMask = (1 << PL6); PinList[43].DirectionAddress = &DDRL; PinList[43].DirectionMask = (1 << DDL6); PinList[43].PinAddress = &PINL; PinList[43].PinMask = (1 << PINL6);
		PinList[44].PortAddress = &PORTL; PinList[44].PortMask = (1 << PL5); PinList[44].DirectionAddress = &DDRL; PinList[44].DirectionMask = (1 << DDL5); PinList[44].PinAddress = &PINL; PinList[44].PinMask = (1 << PINL5); 
		PinList[45].PortAddress = &PORTL; PinList[45].PortMask = (1 << PL4); PinList[45].DirectionAddress = &DDRL; PinList[45].DirectionMask = (1 << DDL4); PinList[45].PinAddress = &PINL; PinList[45].PinMask = (1 << PINL4);
		PinList[46].PortAddress = &PORTL; PinList[46].PortMask = (1 << PL3); PinList[46].DirectionAddress = &DDRL; PinList[46].DirectionMask = (1 << DDL3); PinList[46].PinAddress = &PINL; PinList[46].PinMask = (1 << PINL3);
		PinList[47].PortAddress = &PORTL; PinList[47].PortMask = (1 << PL2); PinList[47].DirectionAddress = &DDRL; PinList[47].DirectionMask = (1 << DDL2); PinList[47].PinAddress = &PINL; PinList[47].PinMask = (1 << PINL2);
		PinList[48].PortAddress = &PORTL; PinList[48].PortMask = (1 << PL1); PinList[48].DirectionAddress = &DDRL; PinList[48].DirectionMask = (1 << DDL1); PinList[48].PinAddress = &PINL; PinList[48].PinMask = (1 << PINL1);
		PinList[49].PortAddress = &PORTL; PinList[49].PortMask = (1 << PL0); PinList[49].DirectionAddress = &DDRL; PinList[49].DirectionMask = (1 << DDL0); PinList[49].PinAddress = &PINL; PinList[49].PinMask = (1 << PINL0);
		PinList[50].PortAddress = &PORTB; PinList[50].PortMask = (1 << PB3); PinList[50].DirectionAddress = &DDRB; PinList[50].DirectionMask = (1 << DDB3); PinList[50].PinAddress = &PINB; PinList[50].PinMask = (1 << PINB3);
		PinList[51].PortAddress = &PORTB; PinList[51].PortMask = (1 << PB2); PinList[51].DirectionAddress = &DDRB; PinList[51].DirectionMask = (1 << DDB2); PinList[51].PinAddress = &PINB; PinList[51].PinMask = (1 << PINB2);
		PinList[52].PortAddress = &PORTB; PinList[52].PortMask = (1 << PB1); PinList[52].DirectionAddress = &DDRB; PinList[52].DirectionMask = (1 << DDB1); PinList[52].PinAddress = &PINB; PinList[52].PinMask = (1 << PINB1);
		PinList[53].PortAddress = &PORTB; PinList[53].PortMask = (1 << PB0); PinList[53].DirectionAddress = &DDRB; PinList[53].DirectionMask = (1 << DDB0); PinList[53].PinAddress = &PINB; PinList[53].PinMask = (1 << PINB0);
		PinList[54].PortAddress = &PORTF; PinList[54].PortMask = (1 << PF0); PinList[54].DirectionAddress = &DDRF; PinList[54].DirectionMask = (1 << DDF0); PinList[54].PinAddress = &PINF; PinList[54].PinMask = (1 << PINF0);
		PinList[55].PortAddress = &PORTF; PinList[55].PortMask = (1 << PF1); PinList[55].DirectionAddress = &DDRF; PinList[55].DirectionMask = (1 << DDF1); PinList[55].PinAddress = &PINF; PinList[55].PinMask = (1 << PINF1);
		PinList[56].PortAddress = &PORTF; PinList[56].PortMask = (1 << PF2); PinList[56].DirectionAddress = &DDRF; PinList[56].DirectionMask = (1 << DDF2); PinList[56].PinAddress = &PINF; PinList[56].PinMask = (1 << PINF2);
		PinList[57].PortAddress = &PORTF; PinList[57].PortMask = (1 << PF3); PinList[57].DirectionAddress = &DDRF; PinList[57].DirectionMask = (1 << DDF3); PinList[57].PinAddress = &PINF; PinList[57].PinMask = (1 << PINF3);
		PinList[58].PortAddress = &PORTF; PinList[58].PortMask = (1 << PF4); PinList[58].DirectionAddress = &DDRF; PinList[58].DirectionMask = (1 << DDF4); PinList[58].PinAddress = &PINF; PinList[58].PinMask = (1 << PINF4);
		PinList[59].PortAddress = &PORTF; PinList[59].PortMask = (1 << PF5); PinList[59].DirectionAddress = &DDRF; PinList[59].DirectionMask = (1 << DDF5); PinList[59].PinAddress = &PINF; PinList[59].PinMask = (1 << PINF5);
		PinList[60].PortAddress = &PORTF; PinList[60].PortMask = (1 << PF6); PinList[60].DirectionAddress = &DDRF; PinList[60].DirectionMask = (1 << DDF6); PinList[60].PinAddress = &PINF; PinList[60].PinMask = (1 << PINF6);
		PinList[61].PortAddress = &PORTF; PinList[61].PortMask = (1 << PF7); PinList[61].DirectionAddress = &DDRF; PinList[61].DirectionMask = (1 << DDF7); PinList[61].PinAddress = &PINF; PinList[61].PinMask = (1 << PINF7);
		PinList[62].PortAddress = &PORTK; PinList[62].PortMask = (1 << PK0); PinList[62].DirectionAddress = &DDRK; PinList[62].DirectionMask = (1 << DDK0); PinList[62].PinAddress = &PINK; PinList[62].PinMask = (1 << PINK0);
		PinList[63].PortAddress = &PORTK; PinList[63].PortMask = (1 << PK1); PinList[63].DirectionAddress = &DDRK; PinList[63].DirectionMask = (1 << DDK1); PinList[63].PinAddress = &PINK; PinList[63].PinMask = (1 << PINK1);
		PinList[64].PortAddress = &PORTK; PinList[64].PortMask = (1 << PK2); PinList[64].DirectionAddress = &DDRK; PinList[64].DirectionMask = (1 << DDK2); PinList[64].PinAddress = &PINK; PinList[64].PinMask = (1 << PINK2);
		PinList[65].PortAddress = &PORTK; PinList[65].PortMask = (1 << PK3); PinList[65].DirectionAddress = &DDRK; PinList[65].DirectionMask = (1 << DDK3); PinList[65].PinAddress = &PINK; PinList[65].PinMask = (1 << PINK3);
		PinList[66].PortAddress = &PORTK; PinList[66].PortMask = (1 << PK4); PinList[66].DirectionAddress = &DDRK; PinList[66].DirectionMask = (1 << DDK4); PinList[66].PinAddress = &PINK; PinList[66].PinMask = (1 << PINK4);
		PinList[67].PortAddress = &PORTK; PinList[67].PortMask = (1 << PK5); PinList[67].DirectionAddress = &DDRK; PinList[67].DirectionMask = (1 << DDK5); PinList[67].PinAddress = &PINK; PinList[67].PinMask = (1 << PINK5);
		PinList[68].PortAddress = &PORTK; PinList[68].PortMask = (1 << PK6); PinList[68].DirectionAddress = &DDRK; PinList[68].DirectionMask = (1 << DDK6); PinList[68].PinAddress = &PINK; PinList[68].PinMask = (1 << PINK6);
		PinList[69].PortAddress = &PORTK; PinList[69].PortMask = (1 << PK7); PinList[69].DirectionAddress = &DDRK; PinList[69].DirectionMask = (1 << DDK7); PinList[69].PinAddress = &PINK; PinList[69].PinMask = (1 << PINK7);
		//Masks and addresses at PIN_ID_NULL should never be accessed.
		PinList[PIN_ID_NULL].PortAddress = NULL;
		PinList[PIN_ID_NULL].PortMask = 0;
		PinList[PIN_ID_NULL].DirectionAddress = NULL;
		PinList[PIN_ID_NULL].DirectionMask = 0;
		PinList[PIN_ID_NULL].PinAddress = NULL;
		PinList[PIN_ID_NULL].PinMask = 0;
}

int8_t IsPinValid(uint8_t pinID)
{
	if ((pinID >= 0)
	 && (pinID <= 69))
	 {
		return TRUE;
	 }
	 else
	 {
		return FALSE;
	 }
}