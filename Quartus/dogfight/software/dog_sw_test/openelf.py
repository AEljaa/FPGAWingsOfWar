import subprocess
from pathlib import Path
import os 

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

def upload(process):
    with open("output.txt", "w") as f:
        out = " "
        while process.stdout.readable():
                out = str(process.stdout.readline(-1),"utf-8")
                f.write(out)
                print(out)
                if out == "0":
                    break
                    
import select

# def upload(process):
#     with open("output.txt", "w") as f:
#         while process.stdout.readable():
#             # Wait for 1 second for data to be available in stdout
#             ready = select.select([process.stdout], [], [], 1)[0]
#             if ready:
#                 out = process.stdout.readline().decode("utf-8")
#                 f.write(out)
#                 print(len(out))
#             else:
#                 # No data received in 1 second, break the loop
#                 break

                    

def loadelf(dir, elfname):
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
            # Send the "cd" command to change directory
            process.stdin.write(bytes(f"cd '{dir}'\n", "utf-8"))
            process.stdin.flush()

            # Send the "nios2-download" command to download the ELF file
            process.stdin.write(bytes(f'nios2-download -g {elfname}.elf\n', "utf-8"))
            process.stdin.flush()
            
            # process.stdin.write(bytes('nios2-terminal\n', "utf-8"))
            # process.stdin.flush()
            
            
            #time.sleep(5)
            #process.stdout.close()  
            #time.sleep(5)
            
            #writing = threading.Thread(target=upload(process))
            #writing.start()
            
            with open("output.txt", "w") as f:
                out = " "
                while process.stdout.readable() :
                    #out = process.stdout.readline(-1)
                    out = str(process.stdout.readline(-1),"utf-8")
                    f.write(out)
                    print(out)
                    
                    
                 
            
            print("session over")
            # Read the output and error, and terminate the process
            #vals, err = process.communicate()
            process.terminate()
            
        except subprocess.TimeoutExpired:
            vals = "Failed"
            process.terminate()
            
        

        #return vals

def main():
    dirpath = Path('C:/intelFPGA_lite/labs/dogfight/software/dog_sw_test')
    loadelf(dirpath,"dog_sw_test2")
    # niosTerminal = threading.Thread(target=loadelf(dirpath,"dog_sw_test"))
    # niosTerminal.start()
    
    # niosTerminal.join()
   
        
    
    
    
    #print(str(response).replace('\\n', '\n').replace('\\r', '\r'))
    
    
main()