using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DroneSpawner : MonoBehaviour
{
    public GameObject markerPrefab;
    public float spawnInterval = 1f;
    public float lifetime = 10f;           // seconds before marker auto-destroys (0 = never)
    public bool alignToGround = true;
    public LayerMask groundMask;           // set to your ground/terrain layer (or leave empty for default)
    public Transform markersParent;        // optional: keeps Hierarchy tidy

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer < spawnInterval) return;
        timer = 0f;

        // Choose spawn position
        Vector3 spawnPos = transform.position;

        if (alignToGround)
        {
            // Cast from a bit above the drone, downwards
            Ray ray = new Ray(transform.position + Vector3.up * 10f, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundMask.value == 0 ? Physics.DefaultRaycastLayers : groundMask))
            {
                spawnPos = hit.point;
            }
        }

        // Spawn
        GameObject m = Instantiate(markerPrefab, spawnPos, Quaternion.identity, markersParent);

        // Auto-destroy
        if (lifetime > 0f) Destroy(m, lifetime);
    }
}

