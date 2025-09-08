using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    static UnityMainThreadDispatcher instance;
    static readonly Queue<Action> queue = new Queue<Action>();

    public static UnityMainThreadDispatcher Instance()
    {
        if (instance == null)
        {
            var go = new GameObject("UnityMainThreadDispatcher");
            instance = go.AddComponent<UnityMainThreadDispatcher>();
            DontDestroyOnLoad(go);
        }
        return instance;
    }

    public void Enqueue(Action action)
    {
        lock (queue) queue.Enqueue(action);
    }

    void Update()
    {
        while (true)
        {
            Action a = null;
            lock (queue)
            {
                if (queue.Count > 0) a = queue.Dequeue();
            }
            if (a == null) break;
            a.Invoke();
        }
    }
}
