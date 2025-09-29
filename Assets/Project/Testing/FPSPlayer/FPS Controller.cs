using UnityEngine;

public class FPSController : MonoBehaviour {
	[SerializeField] Transform head;
	[SerializeField] private float moveSpeed = 5f;
	[SerializeField] private float runSpeed = 10f;
	[SerializeField] private float jumpHeight = 2f;
	[SerializeField] private float mouseSensitivity = 2f;
	private CharacterController cc;
	private Vector3 velocity;
	private float vLook;

	void Start() {
		cc = GetComponent<CharacterController>();
		Cursor.lockState = CursorLockMode.Locked;
	}

	void Update() {
		Vector3 move = (transform.right * Input.GetAxisRaw("Horizontal") + transform.forward * Input.GetAxisRaw("Vertical")) * (Input.GetKey(KeyCode.LeftShift) ? runSpeed : moveSpeed);
		cc.Move(move * Time.deltaTime);
		cc.Move(velocity * Time.deltaTime);

		float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
		float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
		transform.Rotate(Vector3.up * mouseX);
		vLook -= mouseY;
		vLook = Mathf.Clamp(vLook, -90f, 90f);
		head.transform.localRotation = Quaternion.Euler(vLook, 0f, 0f);
	}
}
