using UnityEngine;

[System.Serializable]
public class SLAMData
{
    public float x;
    public float y;
    public float z;
    public float yaw;
}

public class DroneController : MonoBehaviour
{
    [Header("Propellers (assign the actual propeller GameObjects)")]
    public PropellerController propeller_lf;  // left-front
    public PropellerController propeller_lr;  // left-rear
    // Add right side propellers if you have them
    public PropellerController propeller_rf;  // right-front
    public PropellerController propeller_rr;  // right-rear

    [Header("Movement smoothing")]
    public float positionLerp = 10f;
    public float rotationLerp = 10f;

    [Header("Behavior")]
    public bool useKinematic = true;
    public float basePropSpeed = 500f;
    public float extraPropSpeed = 1000f;

    Vector3 targetPosition;
    Quaternion targetRotation;
    Rigidbody rb;

    void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

        if (useKinematic)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Debug propeller assignments
        Debug.Log($"LF assigned: {propeller_lf != null}");
        Debug.Log($"LR assigned: {propeller_lr != null}");
        Debug.Log($"RF assigned: {propeller_rf != null}");
        Debug.Log($"RR assigned: {propeller_rr != null}");
    }

    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionLerp);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationLerp);

        // Manual test controls
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetAllPropellerSpeed(1000f);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            SetAllPropellerSpeed(0f);
        }
    }

    public void ApplySLAM(SLAMData d)
    {
        Vector3 mapped = new Vector3(d.x, d.z, d.y);
        targetPosition = mapped;
        targetRotation = Quaternion.Euler(0f, d.yaw, 0f);

        float thrustNormalized = Mathf.Clamp01(d.z / 10f);
        float propSpeed = basePropSpeed + (thrustNormalized * extraPropSpeed);

        SetAllPropellerSpeed(propSpeed);
    }

    public void SetAllPropellerSpeed(float rpm)
    {
        if (propeller_lf != null) propeller_lf.SetSpeed(rpm);
        if (propeller_lr != null) propeller_lr.SetSpeed(rpm);
        if (propeller_rf != null) propeller_rf.SetSpeed(rpm);
        if (propeller_rr != null) propeller_rr.SetSpeed(rpm);
    }

    public void SetPropellersActive(bool on)
    {
        if (propeller_lf != null) propeller_lf.SetActive(on);
        if (propeller_lr != null) propeller_lr.SetActive(on);
        if (propeller_rf != null) propeller_rf.SetActive(on);
        if (propeller_rr != null) propeller_rr.SetActive(on);
    }
}