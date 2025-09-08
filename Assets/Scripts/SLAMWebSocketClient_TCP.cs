using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class DroneTCPClient : MonoBehaviour
{
    public string serverHost = "127.0.0.1";
    public int serverPort = 8765;
    public DroneController drone; // assign in Inspector

    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;

    [Serializable]
    public class DroneData { public float x; public float y; public float z; public float yaw; }

    void Start()
    {
        if (drone == null) drone = GetComponent<DroneController>();
        ConnectToServer();
    }

    void ConnectToServer()
    {
        try
        {
            client = new TcpClient(serverHost, serverPort);
            stream = client.GetStream();

            receiveThread = new Thread(ReceiveLoop) { IsBackground = true };
            receiveThread.Start();

            Debug.Log("[TCP] Connected to SLAM server");

            // Enable propellers because we are connected
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                if (drone != null) drone.SetPropellersActive(true);
            });
        }
        catch (Exception e)
        {
            Debug.LogError("[TCP] Connection failed: " + e.Message);
        }
    }

    void ReceiveLoop()
    {
        byte[] buffer = new byte[2048];
        StringBuilder sb = new StringBuilder();
        try
        {
            while (client != null && client.Connected)
            {
                int bytes = stream.Read(buffer, 0, buffer.Length);
                if (bytes <= 0) continue;
                sb.Append(Encoding.UTF8.GetString(buffer, 0, bytes));

                string content = sb.ToString();
                int nl;
                while ((nl = content.IndexOf('\n')) >= 0)
                {
                    string line = content.Substring(0, nl).Trim();
                    content = content.Substring(nl + 1);

                    if (!string.IsNullOrEmpty(line))
                    {
                        try
                        {
                            var data = JsonUtility.FromJson<DroneData>(line);
                            // apply on main thread
                            UnityMainThreadDispatcher.Instance().Enqueue(() =>
                            {
                                if (drone != null)
                                    drone.ApplySLAM(new SLAMData { x = data.x, y = data.y, z = data.z, yaw = data.yaw });
                            });
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning("[TCP] JSON parse error: " + ex.Message + " line: " + line);
                        }
                    }
                }

                sb.Clear();
                sb.Append(content);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[TCP] Receive loop ended: " + ex.Message);
        }
        finally
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (drone != null) drone.SetPropellersActive(false);
            });
        }
    }

    void OnApplicationQuit()
    {
        try { receiveThread?.Abort(); } catch { }
        try { stream?.Close(); } catch { }
        try { client?.Close(); } catch { }
    }
}
