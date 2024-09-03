#include "sys/alt_stdio.h"
#include "system.h"
#include "altera_up_avalon_accelerometer_spi.h"
#include "altera_avalon_timer_regs.h"
#include "altera_avalon_timer.h"
#include "altera_avalon_pio_regs.h"
#include "sys/alt_irq.h"
#include "altera_avalon_jtag_uart_regs.h"
#include "altera_avalon_jtag_uart.h"
#include <stdlib.h>
#include "alt_types.h"
#include "sys/times.h"
#include <stdio.h>
#include <unistd.h>
#include <math.h>

#define OFFSET -32
#define PWM_PERIOD 16

int HEX_BASE[6] = {	HEX0_BASE,	HEX1_BASE,
					HEX2_BASE,	HEX3_BASE,
					HEX4_BASE,	HEX5_BASE };

alt_8 pwm = 0;
alt_u8 led;
int level;

void led_write(alt_u8 led_pattern) {
    IOWR(LED_BASE, 0, led_pattern);
}

void convert_read(alt_32 acc_read, int * level, alt_u8 * led) {
    acc_read += OFFSET;
    alt_u8 val = (acc_read >> 6) & 0x07;
    * led = (8 >> val) | (8 << (8 - val));
    * level = (acc_read >> 1) & 0x1f;
}

void sys_timer_isr() {
    IOWR_ALTERA_AVALON_TIMER_STATUS(TIMER_BASE, 0);

    if (pwm < abs(level)) {

        if (level < 0) {
            led_write(led << 1);
        } else {
            led_write(led >> 1);
        }

    } else {
        led_write(led);
    }

    if (pwm > PWM_PERIOD) {
        pwm = 0;
    } else {
        pwm++;
    }

}
void timer_init(void * isr) {

    IOWR_ALTERA_AVALON_TIMER_CONTROL(TIMER_BASE, 0x0003);
    IOWR_ALTERA_AVALON_TIMER_STATUS(TIMER_BASE, 0);
    IOWR_ALTERA_AVALON_TIMER_PERIODL(TIMER_BASE, 0x0900);
    IOWR_ALTERA_AVALON_TIMER_PERIODH(TIMER_BASE, 0x0000);
    alt_irq_register(TIMER_IRQ, 0, isr);
    IOWR_ALTERA_AVALON_TIMER_CONTROL(TIMER_BASE, 0x0007);

}

#define NUM_TAPS 16
#define Q_BITS 8

float filter_coefficientsf[NUM_TAPS] =
	{	0.012325,0.021251,0.035880,0.053198,
			0.071375,0.088065,0.100870,0.107831,
			0.107831,0.100870,0.088065,0.071375,
			0.053198,0.035880,0.021251,0.012325
	};
alt_32 filter_coefficients[NUM_TAPS];

void floatArrayToFixed() {
    for (int i = 0; i < NUM_TAPS; i++) {
    	filter_coefficients[i] = (alt_32)(filter_coefficientsf[i] * pow(2,Q_BITS));
    }
}

alt_32 filter_state_x[NUM_TAPS] = {0};
alt_32 filter_state_y[NUM_TAPS] = {0};
alt_32 filter_state_z[NUM_TAPS] = {0};
/*float filter_coefficientsf[NUM_TAPS] = {-0.000952509386971404,-0.00134958940985540,0.000719515201841284,0.00677266988673947,0.0143466291403088,0.0161182104566358,0.00439193909885626,-0.0202845378052121,-0.0431939780902334,-0.0397897552037631,0.00892811196164070,0.0993476734284162,0.200265495185259,0.266946227284711,0.266946227284711,0.200265495185259,0.0993476734284162,0.00892811196164070,-0.0397897552037631,-0.0431939780902334,-0.0202845378052121,0.00439193909885626,0.0161182104566358,0.0143466291403088,0.00677266988673947,0.000719515201841284,-0.00134958940985540,-0.000952509386971404
};*/



//const float filter_coefficients[NUM_TAPS] = {0.0245, 0.0245, 0.9510};

alt_32 filterFIR(alt_32 acc_read, alt_32 * filter_state) {
    for (int i = NUM_TAPS - 1; i > 0; i--)
    {
        filter_state[i] = filter_state[i - 1];
    }
    filter_state[0] = acc_read;
    alt_32 filtered_value = 0.0;
    for (int i = 0; i < NUM_TAPS; i++) {
        filtered_value += (filter_state[i] * filter_coefficients[i]) >> Q_BITS;
    }
    return filtered_value;
}

void HexOutChar(char c, int base) {
    switch (c) {
        case '0':
        	IOWR(HEX_BASE[base], 0 , 0b1000000); // Corresponds to displaying 0
            break;
        case '1':
        	IOWR(HEX_BASE[base], 0 , 0b1111001); // Corresponds to displaying 1
            break;
        case '2':
        	IOWR(HEX_BASE[base], 0 , 0b0100100); // Corresponds to displaying 2
            break;
        case '3':
        	IOWR(HEX_BASE[base], 0 , 0b0110000); // Corresponds to displaying 3
            break;
        case '4':
        	IOWR(HEX_BASE[base], 0 , 0b0011001); // Corresponds to displaying 4
            break;
        case '5':
        	IOWR(HEX_BASE[base], 0 , 0b0010010); // Corresponds to displaying 5
            break;
        case '6':
        	IOWR(HEX_BASE[base], 0 , 0b0000010); // Corresponds to displaying 6
            break;
        case '7':
        	IOWR(HEX_BASE[base], 0 , 0b1111000); // Corresponds to displaying 7
            break;
        case '8':
        	IOWR(HEX_BASE[base], 0 , 0b0000000); // Corresponds to displaying 8
            break;
        case '9':
        	IOWR(HEX_BASE[base], 0 , 0b0010000); // Corresponds to displaying 9
            break;
        default:
        	IOWR(HEX_BASE[base], 0 , 0b1111111); // Corresponds to displaying blank
            break;
    }
}

void HexOutStr(char *num)
{
	int len = strlen(num);
	for(int i = 0; i<6;i++)
	{
		IOWR(HEX_BASE[i], 0 , 0b1111111);
	}
	if (len < 7)
	{
		for(int i = 0; i < len; i++)
		{
			HexOutChar(num[i],len-i-1);
		}
	}
}

void UpdateCoefs(char* num)
{
	int len = strlen(num);
	if (len < 2)
	{
		float new_coef[NUM_TAPS];
		switch(num[0]) {
			case '1': {
				memcpy(new_coef, (float[])
				{
					0.012325,0.021251,0.035880,0.053198,
					0.071375,0.088065,0.100870,0.107831,
					0.107831,0.100870,0.088065,0.071375,
					0.053198,0.035880,0.021251,0.012325
				}, NUM_TAPS * sizeof(float));
				break;
			}
			case '2': {
				memcpy(new_coef, (float[])
				{
					-0.010897,-0.016158,-0.013893,0.005398,
					0.045566,0.099991,0.153522,0.187614,
					0.189572,0.158555,0.106364,0.051056,
					0.008957,-0.012889,-0.016222,-0.012072
				}, NUM_TAPS * sizeof(float));
				break;
			}
			case '3': {
				memcpy(new_coef, (float[])
				{
					0.007131,0.001340,-0.019200,-0.040403,
					-0.026892,0.047350,0.162069,0.253529,
					0.261253,0.180001,0.063997,-0.019269,
					-0.041703,-0.023061,-0.001239,0.007620
				}, NUM_TAPS * sizeof(float));
				break;
			}
			case '4': {
				memcpy(new_coef, (float[])
				{
					0.014569,0.024259,0.003989,-0.041651,
					-0.057818,0.022196,0.185458,0.325389,
					0.330065,0.195709,0.030130,-0.056110,
					-0.044252,0.001793,0.023965,0.015674
				}, NUM_TAPS * sizeof(float));
				break;
			}
			case '5': {
				memcpy(new_coef, (float[])
				{
					-0.011962,-0.005740,0.029887,0.028352,
					-0.048231,-0.064489,0.125470,0.379960,
					0.397802,0.156252,-0.054043,-0.056956,
					0.022761,0.033166,-0.002552,-0.013066
				}, NUM_TAPS * sizeof(float));
				break;
			}
			case '6': {
				memcpy(new_coef, (float[])
				{
					0.002723,-0.021614,-0.017213,0.041124,
					0.006241,-0.098523,0.059592,0.426097,
					0.454752,0.098554,-0.099387,-0.006773,
					0.044832,-0.011805,-0.024432,0.001480
				}, NUM_TAPS * sizeof(float));
				break;
			}
			case '7': {
				memcpy(new_coef, (float[])
				{
					0.019780,0.006845,-0.034093,0.025464,
					0.042134,-0.104564,0.026260,0.486260,
					0.506736,0.049778,-0.110016,0.036192,
					0.030685,-0.033887,0.003934,0.020860
				}, NUM_TAPS * sizeof(float));
				break;
			}
			case '8': {
				memcpy(new_coef, (float[])
				{
					-0.018126,0.004422,0.015893,-0.043308,
					0.056088,-0.024712,-0.088929,0.490347,
					0.550867,-0.050591,-0.051654,0.065649,
					-0.040620,0.008825,0.010264,-0.019497
				}, NUM_TAPS * sizeof(float));
				break;
			}
			case '9': {
				memcpy(new_coef, (float[])
				{
					-0.027345,0.010873,0.013138,-0.049239,
					0.072652,-0.047365,-0.076428,0.585281,
					0.585281,-0.076428,-0.047365,0.072652,
					-0.049239,0.013138,0.010873,-0.027345
				}, NUM_TAPS * sizeof(float));
				break;
			}
			case 'a': {
				memcpy(new_coef, (float[])
				{
					-0.001203,-0.034137,0.036726,-0.039171,
					0.022216,0.027807,-0.143694,0.612191,
					0.612191,-0.143694,0.027807,0.022216,
					-0.039171,0.036726,-0.034137,-0.001203
				}, NUM_TAPS * sizeof(float));
				break;
			}
			default: {
				memcpy(new_coef, (float[])
				{
					-0.011962,-0.005740,0.029887,0.028352,
					-0.048231,-0.064489,0.125470,0.379960,
					0.397802,0.156252,-0.054043,-0.056956,
					0.022761,0.033166,-0.002552,-0.013066
				}, NUM_TAPS * sizeof(float));
				break;
			}
		}
		memcpy(filter_coefficientsf, new_coef, NUM_TAPS * sizeof(float));
		floatArrayToFixed();

	}

}

int main() {
	floatArrayToFixed();
    alt_32 x_read,y_read,z_read;
    alt_up_accelerometer_spi_dev * acc_dev;
    acc_dev = alt_up_accelerometer_spi_open_dev("/dev/accelerometer_spi");
    int sw,but;
    IOWR(LED_BASE, 0, 0);
    for(int i = 0; i<6;i++)
	{
		IOWR(HEX_BASE[i], 0 , 0b1111111);
	}
    if (acc_dev == NULL) {
        return 1;
    }
    int halt = 1;
    timer_init(sys_timer_isr);
    while (1) {
    	sw = ~IORD(SWITCH_BASE,0);
    	sw &= (0b1111111111);
    	but = ~IORD(BUTTON_BASE,0);
    	/*if(sw == 1)
    	{
    		halt = 1;*/
			usleep(1000);
			alt_up_accelerometer_spi_read_x_axis(acc_dev, & x_read);
			alt_up_accelerometer_spi_read_y_axis(acc_dev, & y_read);
			alt_up_accelerometer_spi_read_z_axis(acc_dev, & z_read);
			alt_printf("x:%x:%x:%x:%x:%x:%x\n", filterFIR(x_read, filter_state_x),filterFIR(y_read, filter_state_y),filterFIR(z_read, filter_state_z),but&(0b01),(but&(0b10))>> 1, (sw));

			//alt_printf("x:%x:%x:%x:%x:%x:%x\n", x_read,y_read,z_read,but&(0b01),(but&(0b10))>> 1, (sw));
			//convert_read(filterFIR(x_read), & level, & led);
    	/*}
    	else if(halt)
    	{
    		alt_printf("!\n");
    		halt = 0;
    	}*/
    	if ((char)IORD(JTAG_UART_BASE, ALTERA_AVALON_JTAG_UART_DATA_REG) == '<')
    	{
    		char *word = NULL;
			int index = 0;
			char in;

			do
			{
				in = (char)IORD(JTAG_UART_BASE, ALTERA_AVALON_JTAG_UART_DATA_REG);
				if (in != '>')
				{
					char *temp = realloc(word, (index + 2) * sizeof(char));
					if (temp == NULL) {
						printf("Memory reallocation failed.\n");
						if (word != NULL){free(word);}
						return 1;
					}
					word = temp;
					word[index] = in;
					word[index + 1] = '\0'; // Null-terminate the string
					index++;
				}
			} while (in != '>');
			HexOutStr(word);
			free(word);
		}
    	if ((char)IORD(JTAG_UART_BASE, ALTERA_AVALON_JTAG_UART_DATA_REG) == '[')
    	    	{
    	    		char *word = NULL;
    				int index = 0;
    				char in;

    				do
    				{
    					in = (char)IORD(JTAG_UART_BASE, ALTERA_AVALON_JTAG_UART_DATA_REG);
    					if (in != '>')
    					{
    						char *temp = realloc(word, (index + 2) * sizeof(char));
    						if (temp == NULL) {
    							printf("Memory reallocation failed.\n");
    							if (word != NULL){free(word);}
    							return 1;
    						}
    						word = temp;
    						word[index] = in;
    						word[index + 1] = '\0'; // Null-terminate the string
    						index++;
    					}
    				} while (in != ']');
    				UpdateCoefs(word);
    				free(word);
    			}
    }
    return 0;
}

