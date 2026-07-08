using UnityEngine;

public class MockInputProvider : VRInputProvider {
	[SerializeField] float sensitivity, handSensitivity, speed, shiftMult, smoothing;
	private Vector3 movement, velocity;
	private Mode mode = Mode.Head;
	private VRInput _Input;
	private float pitch;

	void Update() {
		if (Input.GetKey(KeyCode.Alpha1)) { mode = Mode.Head; }
		if (Input.GetKey(KeyCode.Alpha2)) { mode = Mode.LHand; }
		if (Input.GetKey(KeyCode.Alpha3)) { mode = Mode.RHand; }
		if (Input.GetKey(KeyCode.R)) { _Input = VRInput.TPose; }

		Cursor.lockState = CursorLockMode.Locked;
		switch (mode) {
			case Mode.Head:
				pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * sensitivity, -90, 90);
				_Input.head.rotation = Quaternion.Euler(pitch, _Input.head.rotation.eulerAngles.y + Input.GetAxis("Mouse X") * sensitivity, 0);
				Vector3 moveDir = new Vector3(
						-(Input.GetKey(KeyCode.A) ? 1 : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0),
						-(Input.GetKey(KeyCode.Q) ? 1 : 0) + (Input.GetKey(KeyCode.E) ? 1 : 0),
						-(Input.GetKey(KeyCode.S) ? 1 : 0) + (Input.GetKey(KeyCode.W) ? 1 : 0)).normalized;
				movement = Vector3.SmoothDamp(movement, moveDir, ref velocity, smoothing);
				float moveSpeed = Input.GetKey(KeyCode.LeftShift) ? speed * shiftMult : speed;
				_Input.head.position += _Input.head.rotation * movement * moveSpeed * Time.deltaTime;

				break;
			case Mode.LHand:
				DoHandInput(ref _Input.LHand);
				break;
			case Mode.RHand:
				DoHandInput(ref _Input.RHand);
				break;
		}
	}

	private void DoHandInput(ref DeviceInput input) {
		Vector3 pos = new(Input.mousePositionDelta.x, Input.mousePositionDelta.y, Input.mouseScrollDelta.y);
		input.position += _Input.head.rotation * pos * handSensitivity;
		Vector3 rot = new Vector3(
				-(Input.GetKey(KeyCode.A) ? 1 : 0) + (Input.GetKey(KeyCode.D) ? 1 : 0),
				-(Input.GetKey(KeyCode.Q) ? 1 : 0) + (Input.GetKey(KeyCode.E) ? 1 : 0),
				-(Input.GetKey(KeyCode.S) ? 1 : 0) + (Input.GetKey(KeyCode.W) ? 1 : 0)).normalized;
		input.rotation *= Quaternion.Euler(rot);
		input.grip = Input.GetKey(KeyCode.G) ? 1 : 0;
		input.trigger = Input.GetKey(KeyCode.T) ? 1 : 0;
	}

	int t;
	public override void GetInput(ref VRInput LastInput, ref VRInput _Input) {
		if (t > 0) _Input = this._Input;
		else this._Input = _Input;
		t++;
	}

	public enum Mode { Head, LHand, RHand }
}
