# send_detections.py
import socket, json, time

HOST = "127.0.0.1"
PORT = 8888

sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
print("Connecting to Unity...")
sock.connect((HOST, PORT))
print("Connected. Sending detections...")

detections = [
    {"label": "person", "x": 2, "y": 0, "z": 5},
    {"label": "car", "x": -3, "y": 0, "z": 10},
    {"label": "tree", "x": 0, "y": 0, "z": 8}
]

try:
    while True:
        for d in detections:
            msg = json.dumps(d) + "\n"
            sock.sendall(msg.encode("utf-8"))
            print("Sent:", msg.strip())
            time.sleep(1.0)
except KeyboardInterrupt:
    print("Stopping.")
finally:
    sock.close()
