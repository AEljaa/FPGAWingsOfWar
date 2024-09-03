import time
import intel_jtag_uart
import sys

try:
    ju = intel_jtag_uart.intel_jtag_uart()
    print("Nios connected")
except Exception as e:
    print(e)
    sys.exit(0)
    
ammo = input("Give ammo count: ")
ammo = "<"+ammo+">"
try:
    ju.write(bytes(ammo, "utf-8"))
    time.sleep(3)
    print(str(ju.read(),"utf-8"))
except Exception as e:
    print("An error occurred:", e)
    sys.exit(1)

#ju.write(b'r')
#time.sleep(0.7)


