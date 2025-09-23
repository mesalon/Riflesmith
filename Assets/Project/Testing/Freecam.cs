using UnityEngine;

public class Freecam : MonoBehaviour {
    public float moveSpeed = 10f, lookSpeed = 2f, boostMultiplier = 2f, smoothTime = 0.1f;
    [SerializeField] private ProjectileData data;
    
    private float moveX, moveZ, vertical, xVel, zVel, vertVel;
    private bool disabled;
    void Update() {
        if (Input.GetKeyDown(KeyCode.X)) {
            disabled = !disabled; }

        if (disabled) return;
        if (Input.GetMouseButton(1)) {
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x - Input.GetAxis("Mouse Y") * lookSpeed, transform.eulerAngles.y + Input.GetAxis("Mouse X") * lookSpeed, 0);
            
            moveX = Mathf.SmoothDamp(moveX, Input.GetKey(KeyCode.D) ? 1f : Input.GetKey(KeyCode.A) ? -1f : 0f, ref xVel, smoothTime);
            moveZ = Mathf.SmoothDamp(moveZ, Input.GetKey(KeyCode.W) ? 1f : Input.GetKey(KeyCode.S) ? -1f : 0f, ref zVel, smoothTime);
            vertical = Mathf.SmoothDamp(vertical, Input.GetKey(KeyCode.E) ? 1f : Input.GetKey(KeyCode.Q) ? -1f : 0f, ref vertVel, smoothTime);

            transform.position += (transform.right * moveX + transform.forward * moveZ + Vector3.up * vertical) * moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? boostMultiplier : 1f) * Time.deltaTime;
        }
        
        if (data && Input.GetKeyDown(KeyCode.Space)) ProjectileManager.CreateProjectile(new(data, transform.position - transform.rotation * new Vector3(0, 0, -0.01f), transform.forward));
    }
}