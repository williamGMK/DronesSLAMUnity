using System;
using System.Collections.Concurrent;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    static readonly ConcurrentQueue<Action> _queue = new ConcurrentQueue<Action>();
    public static void Enqueue(Action a) => _queue.Enqueue(a);

    void Update()
    {
        while (_queue.TryDequeue(out var action))
        {
            try { action?.Invoke(); }
            catch (Exception e) { Debug.LogError("Dispatcher action error: " + e); }
        }
    }
}

