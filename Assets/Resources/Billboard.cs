using UnityEngine;

public class ConstantSizeBillboard : MonoBehaviour {
    public Camera targetCamera;
    public float targetScreenSize = 100f;
    public bool lockX = true;
    public bool lockY = true;
    public bool lockZ = false;

    private Vector3 initialScale;
    private float initialDistance;

    void Start() {
        // If no camera is assigned, use the main camera
        if (targetCamera == null) {
            targetCamera = Camera.main;
        }

        if (!targetCamera) return;

        // Store initial scale and distance for reference
        initialScale = transform.localScale;
        initialDistance = Vector3.Distance(transform.position, targetCamera.transform.position);
    }

    void LateUpdate() {
        if (targetCamera == null)
            return;

        // Calculate current distance from camera
        float currentDistance = Vector3.Distance(transform.position, targetCamera.transform.position);

        // Calculate the scale factor based on distance
        float scaleFactor = (currentDistance / initialDistance) * (targetScreenSize / 100f);

        // Get the parent's scale (if any)
        Vector3 parentScale = Vector3.one;
        if (transform.parent != null) {
            parentScale = transform.parent.lossyScale;
        }

        // Calculate new scale, compensating for parent scale and maintaining constant screen size
        Vector3 newScale = initialScale;

        if (lockX)
            newScale.x = initialScale.x * scaleFactor / parentScale.x;
        if (lockY)
            newScale.y = initialScale.y * scaleFactor / parentScale.y;
        if (lockZ)
            newScale.z = initialScale.z * scaleFactor / parentScale.z;

        // Apply the new scale
        transform.localScale = newScale;

        // Optional: Make the billboard face the camera
        transform.rotation = Quaternion.LookRotation(transform.position - targetCamera.transform.position);
    }

    // Optional: Visualize in editor
    void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, targetScreenSize * 0.01f);
    }
}