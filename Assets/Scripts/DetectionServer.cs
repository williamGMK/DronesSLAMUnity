using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using TMPro;  // ✅ Add this at the top for TextMeshPro support

[Serializable]
public class DetectionData
{
    public string label;
    public float x;
    public float y;
    public float z;
}

[Serializable]
public class DetectionArray
{
    public DetectionData[] items;
}

public class DetectionServer : MonoBehaviour
{
    public GameObject detectionPrefab;   // assign DetectionMarker prefab in Inspector
    public int port = 8888;
    public float markerLifetime = 15f;   // seconds the marker stays
    TcpListener listener;
    Thread listenerThread;

    void Start()
    {
        listenerThread = new Thread(ListenForClients);
        listenerThread.IsBackground = true;
        listenerThread.Start();
        Debug.Log("DetectionServer: listener thread started on port " + port);
    }

    void ListenForClients()
    {
        try
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Debug.Log("DetectionServer: listening...");

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient(); // blocking
                Debug.Log("DetectionServer: client connected: " + client.Client.RemoteEndPoint);
                Thread clientThread = new Thread(() => HandleClientComm(client));
                clientThread.IsBackground = true;
                clientThread.Start();
            }
        }
        catch (ThreadAbortException) { }
        catch (Exception e)
        {
            Debug.LogError("DetectionServer listener exception: " + e);
        }
    }

    void HandleClientComm(TcpClient client)
    {
        var stream = client.GetStream();
        var buffer = new byte[4096];
        var sb = new StringBuilder();

        try
        {
            while (client.Connected)
            {
                if (!stream.DataAvailable)
                {
                    Thread.Sleep(10);
                    continue;
                }

                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead <= 0) break;

                string chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                sb.Append(chunk);

                string all = sb.ToString();
                int newline;
                while ((newline = all.IndexOf('\n')) != -1)
                {
                    string line = all.Substring(0, newline).Trim();
                    if (!string.IsNullOrEmpty(line))
                        ProcessJsonLine(line);
                    all = all.Substring(newline + 1);
                }

                sb.Clear();
                sb.Append(all);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Client read error: " + e);
        }
        finally
        {
            try { client.Close(); } catch { }
            Debug.Log("DetectionServer: client disconnected");
        }
    }

    void ProcessJsonLine(string json)
    {
        try
        {
            json = json.Trim();

            if (json.StartsWith("{"))
            {
                var data = JsonUtility.FromJson<DetectionData>(json);
                if (data != null)
                    MainThreadDispatcher.Enqueue(() => SpawnMarker(data));
            }
            else if (json.StartsWith("["))
            {
                string wrapped = "{\"items\":" + json + "}";
                var wrapper = JsonUtility.FromJson<DetectionArray>(wrapped);
                if (wrapper != null && wrapper.items != null)
                {
                    foreach (var d in wrapper.items)
                        MainThreadDispatcher.Enqueue(() => SpawnMarker(d));
                }
            }
            else
            {
                Debug.LogWarning("Unexpected JSON format: " + json);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("JSON parse error: " + e + " json=" + json);
        }
    }

    // 🔹 UPDATED: TextMeshPro handling
    void SpawnMarker(DetectionData data)
    {
        if (detectionPrefab == null)
        {
            Debug.LogWarning("No detectionPrefab assigned");
            return;
        }

        Vector3 pos = new Vector3(data.x, data.y, data.z);
        GameObject marker = Instantiate(detectionPrefab, pos, Quaternion.identity);

        // --- Update TMP text label ---
        var tmp = marker.GetComponentInChildren<TextMeshPro>();
        if (tmp != null)
        {
            tmp.text = data.label;
        }
        else
        {
            var tmpUGUI = marker.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpUGUI != null)
                tmpUGUI.text = data.label;
            else
                Debug.LogWarning("No TextMeshPro component found in DetectionMarker prefab!");
        }

        // --- Make label face camera ---
        var labelTf = marker.transform.Find("Label");
        if (labelTf != null && Camera.main != null)
            labelTf.LookAt(Camera.main.transform);

        Destroy(marker, markerLifetime);
    }

    void OnApplicationQuit()
    {
        try
        {
            listener?.Stop();
            listenerThread?.Abort();
        }
        catch { }
    }
}
