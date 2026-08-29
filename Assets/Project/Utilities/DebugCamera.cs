using UnityEngine;

public class DebugCamera : MonoBehaviour {
	[SerializeField] float sensitivity = 1, speed = 5, shiftMult = 2, smoothing = 0.15f, strength = 100;
	[SerializeField] bool inTouchMode;
	private Vector3 movement, velocity;
	private Body dragBody;
	private Vector3 dragOffset;
	private float dragDepth;
	private float pitch;

	void Start() {
		SetCursor();
	}

	void Update() {
		if (!inTouchMode) {
			pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * sensitivity, -90, 90);
			transform.rotation = Quaternion.Euler(pitch, transform.eulerAngles.y + Input.GetAxis("Mouse X") * sensitivity, 0);
			Vector3 moveDir = new Vector3(
					-(Input.GetKey(KeyCode.A) ? 1 : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0),
					-(Input.GetKey(KeyCode.Q) ? 1 : 0) + (Input.GetKey(KeyCode.E) ? 1 : 0),
					-(Input.GetKey(KeyCode.S) ? 1 : 0) + (Input.GetKey(KeyCode.W) ? 1 : 0)).normalized;
			movement = Vector3.SmoothDamp(movement, moveDir, ref velocity, smoothing);
			float flySpeed = Input.GetKey(KeyCode.LeftShift) ? speed * shiftMult : speed;
			transform.position += transform.rotation * movement * flySpeed * Time.deltaTime;

			if (Input.GetKeyDown(KeyCode.Space)) 
				ProjectileManager.CreateGenericProjectile(transform.position - transform.rotation * new Vector3(0, 0, -0.01f), transform.forward);
		}

		if (Input.GetKeyDown(KeyCode.X)) {
			inTouchMode = !inTouchMode;
			SetCursor();
		}

		if (Input.GetMouseButtonDown(0)) {
			var ray = GameManager.Camera.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out var hit) && hit.GetBody()) {
				dragBody = hit.GetBody();
				dragDepth = Vector3.Distance(GameManager.Camera.transform.position, hit.point);
				dragOffset = dragBody.transform.InverseTransformPoint(hit.point);
			}
		}
		else if (Input.GetMouseButtonUp(0) && dragBody) { dragBody = null; }
		if (dragBody) dragDepth = Mathf.Max(1f, dragDepth + Input.mouseScrollDelta.y);
	}

	void FixedUpdate() {
		if (dragBody) {
			Vector3 targetPos = GameManager.Camera.ScreenToWorldPoint(new(Input.mousePosition.x, Input.mousePosition.y, dragDepth));
			Vector3 currentPos = dragBody.transform.TransformPoint(dragOffset);
			Vector3 targetVel = (targetPos - currentPos) * strength;
			Vector3 force = targetVel - dragBody.GetPointVelocity(currentPos);
			dragBody.AddForceAtPosition(force, currentPos);
		}
	}

	private void SetCursor() => Cursor.lockState = inTouchMode ? CursorLockMode.None : CursorLockMode.Locked;
}
