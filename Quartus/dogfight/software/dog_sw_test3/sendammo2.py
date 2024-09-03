import intel_jtag_uart
import sys
import threading
import time
import subprocess
from pathlib import Path
import os 
import time
NIOS_CMD_SHELL_BAT = "C:/intelFPGA_lite/18.1/nios2eds/Nios II Command Shell.bat"

def con(dir, elfname):
        assert len(elfname) >= 1, "Please make the elf name file at least a single character"
        assert os.path.exists(dir), "Directory doesn't exist"
        # create a subprocess which will run the nios2-terminal
        process = subprocess.Popen(
            NIOS_CMD_SHELL_BAT,
            bufsize=0,
            stdin=subprocess.PIPE,
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE
        )
        # send the cmd string to the nios2-terminal, read the output and terminate the process
        try:
            process.stdin.write(bytes(f"cd '{dir}'\n", "utf-8"))
            process.stdin.flush()

            process.stdin.write(bytes(f'nios2-download -g {elfname}.elf\n', "utf-8"))
            process.stdin.flush()
            time.sleep(1)    
            process.stdout.close()
            process.stdin.close()
            process.stderr.close()
            process.terminate()
            print(f"Downloaded {elfname}.elf")               
        except subprocess.TimeoutExpired:
            print("Connection failed")
            process.terminate()
            sys.exit(0)

def IsInteger(s):
    try:
        int(s)
        return True
    except ValueError:
        return False

def ReadAndPrint(nios):
    try:
        while True:
            data = nios.read()
            if str(data)[-4:] == "!\\n'":
                print("Stream ended")
                nios.close()
                sys.exit(0)
            if data:
                print(str(data, "utf-8"),end="")
    except Exception as e:
        print("An error occurred while reading:", e)

def WriteAmmo(nios, ammo):
    try:
        nios.write(bytes(ammo, "utf-8"))
    except Exception as e:
        print("An error occurred while writing:", e)
        sys.exit(1)
        
        
def main():
    dirpath = Path('C:/intelFPGA_lite/labs/dogfight/software/dog_sw_test2')
    elfname = "dog_sw_test2"
    con(dirpath,"dog_sw_test2")  

    try:
        nios = intel_jtag_uart.intel_jtag_uart()
        print("Nios connected")
    except Exception as e:
        print(e)
        sys.exit(1)
    
    # try:
    #     nios.write(bytes(f"cd '{dirpath}'\n", "utf-8"))
    #     time.sleep(1)
    #     nios.write(bytes(f'nios2-download -g {elfname}.elf\n', "utf-8"))
    #     time.sleep(1)
    # except:
    #     print(e)
    #     sys.exit(1)
    
    # print("done")
    
    # Start the thread for reading and printing
    read_thread = threading.Thread(target=ReadAndPrint, args=(nios,))
    read_thread.daemon = True  # Setting as daemon thread to stop when main thread exits
    read_thread.start()

    # Main loop to handle writing
    while True:
        ammo = input()
        if ammo.lower() == 'exit':
            break
        if not IsInteger(ammo):
            continue
        ammo = "<" + ammo + ">" # Angle brackets are beggiing and end for Nios detection
        write_thread = threading.Thread(target=WriteAmmo, args=(nios, ammo))
        write_thread.start()
        write_thread.join()  # Wait for the write thread to finish before proceeding

    print("Exiting program")
    
    
    
main()
