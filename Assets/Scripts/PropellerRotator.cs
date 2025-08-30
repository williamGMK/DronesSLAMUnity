using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropellerRotator : MonoBehaviour
{

    public float rotationSpeed = 1000f;   // degrees per second
    public Vector3 rotationAxis = Vector3.up; // default spin axis
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Rotate around the Y axis (up)
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }
}
