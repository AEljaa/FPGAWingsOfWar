import intel_jtag_uart
import sys
import threading
import time
import subprocess
from pathlib import Path
import os 
import time
import socket

NIOS_CMD_SHELL_BAT = "C:/intelFPGA_lite/18.1/nios2eds/Nios II Command Shell.bat"

def DownldElf(dir, elfname):
        assert len(elfname) >= 1, "Please make the elf name file at least a single character"
        assert os.path.exists(dir), "Directory doesn't exist"
        # Create a subprocess which will run the nios2-terminal
        process = subprocess.Popen(
            NIOS_CMD_SHELL_BAT,
            bufsize=0,
            stdin=subprocess.PIPE,
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE
        )
        # Send the download commmand to the nios2-terminal and terminate the process
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

def SendNios2Unity(nios,unityWriteCon):
    try:
        while True:
            data = nios.read()
            if str(data)[-4:] == "!\\n'": # Detect end of transmission character
                print("Stream ended")
                nios.close()
                unityWriteCon.close()
                sys.exit(0)
            if data:
                #print(data)
                #print(str(data, "utf-8")) 
                unityWriteCon.send(data) # Send data via socket to unity
    except Exception as e:
        print("An error occurred while reading:", e)

def SendUnity2Nios(nios, ammo):
    try:
        nios.write(bytes(ammo, "utf-8"))
    except Exception as e:
        print("An error occurred while writing:", e)
        sys.exit(1)
        
        
def main():
    dirpath = Path('C:/intelFPGA_lite/labs/dogfight/software/dog_sw_test4')
    elfname = "dog_sw_test4"
    DownldElf(dirpath,elfname)  

    try:
        nios = intel_jtag_uart.intel_jtag_uart()
        print("Nios connected")
        unityWriteAddr = ('localhost', 49152)  # Change port as needed
        unityWriteCon = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        unityWriteCon.connect(unityWriteAddr)  
        print("Socket connected")
        
        unityReadAddr = ('localhost', 49153)  # Change port as needed
        unityReadCon = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        unityReadCon.bind(unityReadAddr)
        unityReadCon.listen(1)
        unityReadClient, unityReadClientAddr = unityReadCon.accept()
    except Exception as e:
        print(e)
        sys.exit(1)
    
    # Start the thread for reading and printing
    noisReadThread = threading.Thread(target=SendNios2Unity, args=(nios,unityWriteCon))
    noisReadThread.daemon = True  # Setting as daemon thread to stop when main thread exits
    noisReadThread.start()

    # Main loop to handle writing
    while True:
        try:
            ammo = unityReadClient.recv(1024).decode("utf-8")
            if not ammo:  # If received data is empty, indicating connection closed
                print("Connection closed by Unity")
                break
        except Exception as e:
            print("An error occurred while receiving data from Unity:", e)
            continue
        if ammo.lower() == 'exit':
            unityWriteCon.close()
            break
        if not IsInteger(ammo):
            continue
        ammo = "<" + ammo + ">" # Angle brackets are beggiing and end for Nios detection
        noisWriteThread = threading.Thread(target=SendUnity2Nios, args=(nios, ammo))
        noisWriteThread.start()
        noisWriteThread.join()  # Wait for the write thread to finish before proceeding

    print("Exiting program")
    
    
    
if __name__ == "__main__":
    main()
