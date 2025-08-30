import socket
import json
import math
import time

class SLAMTCPServer:
    def __init__(self, host="127.0.0.1", port=8765):
        self.host = host
        self.port = port
        self.conn = None

    def start_tcp_server(self):
        # Create TCP server
        server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        server.bind((self.host, self.port))
        server.listen(1)
        print(f"SLAM TCP Server running on tcp://{self.host}:{self.port}")
        print("Waiting for Unity to connect...")

        # Wait until Unity connects
        self.conn, addr = server.accept()
        print(f"Connected to Unity: {addr}")

    def run(self):
        t = 0
        r = 5.0   # radius of the circle
        z = 2.0   # fixed altitude

        try:
            while True:
                # Generate dummy SLAM data (circle path)
                angle = t * 0.1
                x = r * math.cos(angle)
                y = r * math.sin(angle)
                yaw = math.degrees(angle) % 360

                data = {
                    "x": x,
                    "y": y,
                    "z": z,
                    "yaw": yaw
                }

                # Encode JSON with newline for Unity
                msg = json.dumps(data) + "\n"

                try:
                    self.conn.sendall(msg.encode("utf-8"))
                except Exception as e:
                    print("Lost connection to Unity:", e)
                    break

                time.sleep(0.2)
                t += 1

        except KeyboardInterrupt:
            print("Server stopped by user")
        finally:
            if self.conn:
                self.conn.close()

if __name__ == "__main__":
    server = SLAMTCPServer()
    server.start_tcp_server()
    server.run()
