using UnityEngine;

public class DebugCamera : MonoBehaviour {
    [SerializeField] ProjectileData projectile;
    [SerializeField] float sensitivity = 1, speed = 5, shiftMult = 2, smoothing = 0.15f;
    private Vector3 movement, velocity;
    Rigidbody dragBody;
    Vector3 dragOffset;
    float dragDepth;
    private bool inTouchMode;

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {
        if (!inTouchMode) {
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x - Input.GetAxis("Mouse Y") * sensitivity, transform.eulerAngles.y + Input.GetAxis("Mouse X") * sensitivity, 0);
            Vector3 moveDir = new Vector3(
                    -(Input.GetKey(KeyCode.A) ? 1 : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0),
                    -(Input.GetKey(KeyCode.Q) ? 1 : 0) + (Input.GetKey(KeyCode.E) ? 1 : 0),
                    -(Input.GetKey(KeyCode.S) ? 1 : 0) + (Input.GetKey(KeyCode.W) ? 1 : 0)).normalized;
            movement = Vector3.SmoothDamp(movement, moveDir, ref velocity, smoothing);
            float flySpeed = Input.GetKey(KeyCode.LeftShift) ? speed * shiftMult : speed;
            transform.position += transform.rotation * movement * flySpeed * Time.deltaTime;

            if (projectile && Input.GetKeyDown(KeyCode.Space)) ProjectileManager.CreateProjectile(new(projectile, transform.position - transform.rotation * new Vector3(0, 0, -0.01f), transform.forward));
        }

        if (Input.GetKeyDown(KeyCode.X)) {
            inTouchMode = !inTouchMode;
            Cursor.lockState = inTouchMode ? CursorLockMode.None : CursorLockMode.Locked;
        }

        if (Input.GetMouseButtonDown(0)) {
            var ray = GameManager.Camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit) && hit.rigidbody) {
                dragBody = hit.rigidbody;
                dragDepth = Vector3.Distance(GameManager.Camera.transform.position, hit.point);
                dragOffset = dragBody.transform.InverseTransformPoint(hit.point);
                // dragBody.useGravity = false; // Tip: Keep gravity ON for natural "dangling"
            }
        }
        else if (Input.GetMouseButtonUp(0) && dragBody) {
            // dragBody.useGravity = true; 
            dragBody = null;
        }
        if (dragBody) dragDepth = Mathf.Max(1f, dragDepth + Input.mouseScrollDelta.y);
    }

    void FixedUpdate() {
        if (dragBody) {
            var mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, dragDepth);
            var targetPos = GameManager.Camera.ScreenToWorldPoint(mousePos);
            var currentPos = dragBody.transform.TransformPoint(dragOffset);

            // Calculate velocity required to reach target
            var targetVel = (targetPos - currentPos) * 15f;
            
            // Apply the difference between target velocity and current point velocity
            // This applies Torque automatically because we are pushing a specific point
            var force = targetVel - dragBody.GetPointVelocity(currentPos);
            
            dragBody.AddForceAtPosition(force, currentPos, ForceMode.VelocityChange);
        }
    }
}
