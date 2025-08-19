import asyncio
import websockets
import json
import numpy as np

async def slam_handler(websocket):   # only 1 arg in v12+
    try:
        while True:
            # Receive (optional)
            try:
                msg = await asyncio.wait_for(websocket.recv(), timeout=0.01)
                print(f"Unity sent: {msg}")
            except asyncio.TimeoutError:
                pass

            # Generate dummy SLAM data
            data = {
                "x": float(np.random.uniform(-10, 10)),
                "y": float(np.random.uniform(-10, 10)),
                "z": float(np.random.uniform(0, 5)),
                "yaw": float(np.random.uniform(-180, 180))
            }

            await websocket.send(json.dumps(data))
            await asyncio.sleep(0.2)

    except websockets.exceptions.ConnectionClosed:
        print("Unity disconnected")

async def main():
    print("SLAM WebSocket Server running on ws://localhost:8765")
    async with websockets.serve(slam_handler, "localhost", 8765):
        await asyncio.Future()  # run forever

if __name__ == "__main__":
    asyncio.run(main())