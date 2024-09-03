import time
import intel_jtag_uart
import sys

try:
    ju = intel_jtag_uart.intel_jtag_uart()
    print("Nios connected")
except Exception as e:
    print(e)
    sys.exit(0)
try:
    while True:
        data = ju.read()
        if str(data)[-4:] == "!\\n'":
            print("Stream ended. Exiting...")
            sys.exit(0)
        if data:
            print(str(data,"utf-8"),end="")
except KeyboardInterrupt:
    print("Keyboard interrupt received. Exiting...")
    sys.exit(0)
except Exception as e:
    print("An error occurred:", e)
    sys.exit(1)

#ju.write(b'r')
#time.sleep(0.7)


