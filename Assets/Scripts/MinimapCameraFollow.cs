using UnityEngine;

public class MinimapCameraFollow : MonoBehaviour
{
    public Transform target;      // assign your Drone here
    public float height = 50f;    // camera Y position
    public bool rotateWithTarget = false; // if true, map "up" to drone heading

    void LateUpdate()
    {
        if (target == null) return;

        // Keep camera above target (x,z) at fixed height
        transform.position = new Vector3(target.position.x, target.position.y + height, target.position.z);

        if (rotateWithTarget)
        {
            // camera looks down and rotates around Y so the minimap rotates with drone
            transform.rotation = Quaternion.Euler(90f, target.eulerAngles.y, 0f);
        }
        else
        {
            // fixed north-up view
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }
}
