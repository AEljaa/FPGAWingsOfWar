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

#define NUM_TAPS 28
#define Q_BITS 8

void floatArrayToFixed(alt_32 fixedArray[], float floatArray[], int arraySize) {
    for (int i = 0; i < arraySize; i++) {
        fixedArray[i] = (alt_32)(floatArray[i] * pow(2,Q_BITS));
    }
}

alt_32 filter_state[NUM_TAPS] = {0};
float filter_coefficientsf[NUM_TAPS] = {-0.000952509386971404,-0.00134958940985540,0.000719515201841284,0.00677266988673947,0.0143466291403088,0.0161182104566358,0.00439193909885626,-0.0202845378052121,-0.0431939780902334,-0.0397897552037631,0.00892811196164070,0.0993476734284162,0.200265495185259,0.266946227284711,0.266946227284711,0.200265495185259,0.0993476734284162,0.00892811196164070,-0.0397897552037631,-0.0431939780902334,-0.0202845378052121,0.00439193909885626,0.0161182104566358,0.0143466291403088,0.00677266988673947,0.000719515201841284,-0.00134958940985540,-0.000952509386971404
};
alt_32 filter_coefficients[NUM_TAPS];


//const float filter_coefficients[NUM_TAPS] = {0.0245, 0.0245, 0.9510};

alt_32 filterFIR(alt_32 acc_read) {
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


int main() {
	floatArrayToFixed(filter_coefficients,filter_coefficientsf, NUM_TAPS);
    alt_32 x_read,y_read,z_read;
    alt_up_accelerometer_spi_dev * acc_dev;
    acc_dev = alt_up_accelerometer_spi_open_dev("/dev/accelerometer_spi");
    int sw;
    if (acc_dev == NULL) {
        return 1;
    }
    //alt_printf("hey\t");
    timer_init(sys_timer_isr);
    while (1) {
    	sw = ~IORD_ALTERA_AVALON_PIO_DATA(SWITCH_BASE);
    	sw &= (0b0001111111);
    	if(sw == 1)
    	{
			usleep(500000);
			alt_up_accelerometer_spi_read_x_axis(acc_dev, & x_read);
			alt_up_accelerometer_spi_read_y_axis(acc_dev, & y_read);
			alt_up_accelerometer_spi_read_z_axis(acc_dev, & z_read);
			alt_printf("%x\t\t", x_read);
			alt_printf("%x\t\t", y_read);
			alt_printf("%x\n", z_read);
			convert_read(filterFIR(x_read), & level, & led);
    	}
    }
    return 0;
}

