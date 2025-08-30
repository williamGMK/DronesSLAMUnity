import asyncio
import math
import websockets
import json
import numpy as np

async def slam_handler(websocket):   # only 1 arg in v12+
    try:
        # SLAM data to make drone go in a circle
        t = 0
        r = 5.0   # radius of the circle
        z = 2.0   # fixed altitude

        while True:
            # Receive (optional)
            try:
                msg = await asyncio.wait_for(websocket.recv(), timeout=0.01)
                print(f"Unity sent: {msg}")
            except asyncio.TimeoutError:
                pass

            # Generate dummy SLAM data (circular, parametric equations for a circle)
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

            # data = {
            #     "x": float(np.random.uniform(-10, 10)),
            #     "y": float(np.random.uniform(-10, 10)),
            #     "z": float(np.random.uniform(0, 5)),
            #     "yaw": float(np.random.uniform(-180, 180))
            # }

            await websocket.send(json.dumps(data))
            await asyncio.sleep(0.2)
            t += 1

    except websockets.exceptions.ConnectionClosed:
        print("Unity disconnected")

async def main():
    print("SLAM WebSocket Server running on ws://localhost:8765")
    async with websockets.serve(slam_handler, "localhost", 8765):
        await asyncio.Future()  # run forever

if __name__ == "__main__":
    asyncio.run(main())