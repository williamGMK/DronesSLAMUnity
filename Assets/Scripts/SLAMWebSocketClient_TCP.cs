using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class DroneTCPClient : MonoBehaviour
{
    [Header("Connection Settings")]
    public string serverHost = "127.0.0.1";
    public int serverPort = 8765;

    [Header("Drone Settings")]
    public Transform droneTransform;
    public float positionLerp = 2f;
    public float rotationLerp = 2f;

    [Serializable]
    public class DroneData
    {
        public float x;
        public float y;
        public float z;
        public float yaw;
    }

    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;

    private Vector3 targetPosition = Vector3.zero;
    private Quaternion targetRotation = Quaternion.identity;

    void Start()
    {
        if (droneTransform == null)
        {
            droneTransform = transform; // default to this object
        }
        ConnectToServer(serverHost, serverPort);
    }

    void Update()
    {
        // move the drone to the new coordinates
        if (droneTransform != null)
        {
            droneTransform.position = Vector3.Lerp(droneTransform.position, targetPosition, Time.deltaTime * positionLerp);
            droneTransform.rotation = Quaternion.Slerp(droneTransform.rotation, targetRotation, Time.deltaTime * rotationLerp);
        }
    }

    void ConnectToServer(string host, int port)
    {
        try
        {
            client = new TcpClient(host, port);
            stream = client.GetStream();

            receiveThread = new Thread(new ThreadStart(ReceiveData));
            receiveThread.IsBackground = true;
            receiveThread.Start();

            Debug.Log("Connected to SLAM TCP Server");
        }
        catch (Exception e)
        {
            Debug.LogError("Connection error: " + e.Message);
        }
    }

    void ReceiveData()
    {
        byte[] buffer = new byte[1024];
        StringBuilder sb = new StringBuilder();

        try
        {
            while (true)
            {
                // number of bytes read, if 0 then skip rest of loop
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead <= 0) continue;

                sb.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                // Process newline-delimited JSON
                string content = sb.ToString();
                int newlineIndex;
                // while there still exists newline chars in the string
                while ((newlineIndex = content.IndexOf('\n')) >= 0)
                {
                    string line = content.Substring(0, newlineIndex).Trim();
                    content = content.Substring(newlineIndex + 1);

                    if (!string.IsNullOrEmpty(line))
                    {
                        try
                        {
                            // deserialise json data
                            DroneData data = JsonUtility.FromJson<DroneData>(line);

                            targetPosition = new Vector3(data.x, data.z, data.y);
                            targetRotation = Quaternion.Euler(0, -data.yaw, 0);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("JSON parse error: " + e.Message);
                        }
                    }
                }

                sb.Clear();
                sb.Append(content);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Connection closed: " + e.Message);
        }
    }

    void OnApplicationQuit()
    {
        if (receiveThread != null) receiveThread.Abort();
        if (stream != null) stream.Close();
        if (client != null) client.Close();
    }
}
