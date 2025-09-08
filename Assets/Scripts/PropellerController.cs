using UnityEngine;

public class PropellerController : MonoBehaviour
{
    private float rotationSpeed = 0f;
    public bool clockwise = true;

    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;

    void Start()
    {
        // Store original position relative to parent (hub)
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;

        Debug.Log($"{gameObject.name} initialized at local position: {originalLocalPosition}");
    }

    void Update()
    {
        if (Mathf.Abs(rotationSpeed) > 0.01f)
        {
            float dir = clockwise ? 1f : -1f;

            // Rotate around own axis
            //transform.Rotate(Vector3.up * dir * rotationSpeed * Time.deltaTime, Space.Self);
            transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime, Space.Self);

            // Maintain original position relative to hub
            transform.localPosition = originalLocalPosition;
        }
    }

    public void SetSpeed(float speed)
    {
        rotationSpeed = speed;
        Debug.Log($"{gameObject.name} speed set to: {speed}");
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}