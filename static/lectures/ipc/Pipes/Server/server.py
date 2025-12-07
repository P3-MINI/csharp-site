import socket
import os

SOCKET_FILE = '/tmp/p3_pipe'

def run_server():
    if os.path.exists(SOCKET_FILE):
        os.remove(SOCKET_FILE)

    server_socket = socket.socket(socket.AF_UNIX, socket.SOCK_STREAM)

    try:
        server_socket.bind(SOCKET_FILE)
        server_socket.listen(1)
        
        connection, client_address = server_socket.accept()

        with connection:
            reader = connection.makefile('r')
            writer = connection.makefile('w')

            while True:
                line = reader.readline()
                if not line:
                    break
                
                processed_line = line.strip().upper() + '\n'
                writer.write(processed_line)
                writer.flush()
                
    except Exception as e:
        print(f"An error occurred: {e}")
    finally:
        print("Shutting down.")
        server_socket.close()
        if os.path.exists(SOCKET_FILE):
            os.remove(SOCKET_FILE)

if __name__ == '__main__':
    run_server()