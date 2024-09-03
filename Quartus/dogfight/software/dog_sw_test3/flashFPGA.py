import subprocess
from pathlib import Path

# Path of the Quartus Programmer
# (should be the same for all computers if you installed Quartus the normal way)
progPath = Path("C:/intelFPGA_lite/18.1/quartus/bin64/quartus_pgm")

# Path of the .sof file to flash
sofPath = Path("C:/intelFPGA_lite/labs/dogfight/DE10_LITE_Golden_Top.sof")

# Port for flashing
hardware = "USB-Blaster"

# Command to program the FPGA
command = [
    progPath,
    "-m", "JTAG", # mode of command
    "-c", hardware, # cable used
    "-o", "p;"+ str(sofPath) # operation type
]

# Run the command
subprocess.run(command)

# You can find more at https://www.intel.com/programmable/technical-pdfs/654662.pdf (page 76)