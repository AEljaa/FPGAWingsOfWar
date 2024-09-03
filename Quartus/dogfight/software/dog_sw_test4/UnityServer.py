import socket
import threading

def Send2Nios(niosWriteCon):
    try:
        while True:
            message = input("Enter message to send to Nios: ")
            if message.lower() == 'exit':
                break
            niosWriteCon.sendall(message.encode())
    except Exception as e:
        print("An error occurred while sending to Nios:", e)
    finally:
        niosWriteCon.close()

def ReceiveFrmNios(niosReadClient):
    try:
        while True:
            data = niosReadClient.recv(1024)
            if not data:
                break
            print(data.decode("utf-8"))
    except Exception as e:
        print("An error occurred while receiving from Nios:", e)
    finally:
        niosReadClient.close()

def main():
    # Create a socket object for receiving from Nios
    niosReadAddr = ('localhost', 49152)
    niosReadCon = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    niosReadCon.bind(niosReadAddr)
    niosReadCon.listen(1)

    print("Waiting for a connection from Nios...")
    niosReadClient, niosReadClient_addr = niosReadCon.accept()
    print("Connection established with Nios:", niosReadClient_addr)

    # Create a socket object for sending to Nios
    niosWriteAddr = ('localhost', 49153)
    niosWriteCon = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    niosWriteCon.connect(niosWriteAddr)
    print("Connected to Nios")

    # Start a thread for sending to Nios
    sendThread = threading.Thread(target=Send2Nios, args=(niosWriteCon,))
    sendThread.start()

    # Start a thread for receiving from Nios
    receiveThread = threading.Thread(target=ReceiveFrmNios, args=(niosReadClient,))
    receiveThread.start()

    sendThread.join()
    receiveThread.join()

if __name__ == "__main__":
    main()
