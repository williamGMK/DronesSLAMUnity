using UnityEngine;
using NativeWebSocket; // Make sure the NativeWebSocket folder is in Assets/Plugins
using System.Collections.Generic;

[System.Serializable]
public class SLAMData
{
    public float x;
    public float y;
    public float z;
    public float yaw;
}

public class SLAMWebSocketClient : MonoBehaviour
{
    [Tooltip("ws://host:port")]
    public string serverUrl = "ws://localhost:8765";

    public Transform droneTransform;       // Assign your Drone GameObject in Inspector
    public float positionLerp = 10f;       // Higher = faster position update
    public float rotationLerp = 10f;       // Higher = faster rotation update

    private NativeWebSocket.WebSocket websocket;   // Fully qualified name to avoid ambiguity
    private readonly Queue<SLAMData> recvQueue = new Queue<SLAMData>();
    private readonly object queueLock = new object();

    async void Start()
    {
        websocket = new NativeWebSocket.WebSocket(serverUrl);

        websocket.OnOpen += () =>
        {
            Debug.Log("[WS] Connected to " + serverUrl);
        };

        websocket.OnError += (e) =>
        {
            Debug.LogWarning("[WS] Error: " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("[WS] Closed: " + e);
        };

        websocket.OnMessage += (bytes) =>
        {
            string msg = System.Text.Encoding.UTF8.GetString(bytes);
            try
            {
                SLAMData data = JsonUtility.FromJson<SLAMData>(msg);
                if (data != null)
                {
                    lock (queueLock)
                    {
                        recvQueue.Enqueue(data);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[WS] JSON parse error: " + ex.Message + " msg=" + msg);
            }
        };

        await websocket.Connect();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif

        SLAMData latestData = null;
        lock (queueLock)
        {
            while (recvQueue.Count > 0)
            {
                latestData = recvQueue.Dequeue();
            }
        }

        if (latestData != null && droneTransform != null)
        {
            // Map SLAM coordinates to Unity coordinates
            Vector3 targetPos = new Vector3(latestData.x, latestData.z, latestData.y);

            // Smooth position
            droneTransform.position = Vector3.Lerp(droneTransform.position, targetPos, Time.deltaTime * positionLerp);

            // Smooth rotation (yaw only)
            Quaternion targetRot = Quaternion.Euler(0f, latestData.yaw, 0f);
            droneTransform.rotation = Quaternion.Slerp(droneTransform.rotation, targetRot, Time.deltaTime * rotationLerp);
        }
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null)
        {
            await websocket.Close();
        }
    }
}
