import subprocess
from pathlib import Path
import os 
import time

NIOS_CMD_SHELL_BAT = "C:/intelFPGA_lite/18.1/nios2eds/Nios II Command Shell.bat"

def send_on_jtag(cmd):
    # check if atleast one character is being sent down
    assert (len(cmd) >= 1), "Please make the cmd a single character"
   
    # create a subprocess which will run the nios2-terminal
    process = subprocess.Popen(
        NIOS_CMD_SHELL_BAT,
        bufsize=0,
        stdin=subprocess.PIPE,
        stdout=subprocess.PIPE,
    )
    # send the cmd string to the nios2-terminal, read the output and terminate the process
    try:
        vals, err = process.communicate(
            bytes(cmd, "utf-8")
        )
        process.terminate()
        
    except subprocess.TimeoutExpired:
        vals = "Failed"
        process.terminate()
    return vals

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
            
            time.sleep(1.3)
            
            process.stdin.write(bytes('nios2-terminal\n', "utf-8"))
            process.stdin.flush()
            
            time.sleep(2)
            
            with open("output.txt", "w") as f:
                while process.stdout.readable() :
                    out = process.stdout.readline(-1)
                    outdecode = str(out,"utf-8")
                    
                    f.write(outdecode)
                    print(outdecode)
                    try:
                        if int(outdecode) == 0:
                            break
                    except:
                        pass
                    
            process.stdout.close()
            process.stdin.close()
            process.stderr.close()
            process.terminate()               
            print("Conroller disonnected (SW1 flipped down)")
            
            
        except subprocess.TimeoutExpired:
            print("Connection failed")
            process.terminate()
            
def recon(dir):
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
        process.stdin.write(bytes('nios2-terminal\n', "utf-8"))
        process.stdin.flush()
        
        with open("output.txt", "w") as f:
            
            while process.stdout.readable() :
                out = process.stdout.readline(-1)
                outdecode = str(out,"utf-8")
                
                f.write(outdecode)
                print(outdecode)
                try:
                    if int(outdecode) == 0:
                        break
                except:
                    pass
                
        process.stdout.close()
        process.terminate()               
        print("Conroller manually disonnected")
        
        
    except subprocess.TimeoutExpired:
        print("Connection failed")
        process.terminate()

def main():
    dirpath = Path('C:/intelFPGA_lite/labs/dogfight/software/dog_sw_test2')
    con(dirpath,"dog_sw_test2")  
    
    if input("Reconnect Y/N ?") == "Y" :
        recon(dirpath)
        
    
main()