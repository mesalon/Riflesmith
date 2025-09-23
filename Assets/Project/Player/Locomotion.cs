using UnityEngine;

public class Locomotion : MonoBehaviour {
	[SerializeField] float speed = 5f;
	[SerializeField] float airSpeed = 1;
	[SerializeField] float flightSpeed = 15;
	[SerializeField] float jumpForce = 5f;
	[SerializeField] float doubleTap = 0.4f;
	[SerializeField] float gravity = -9.81f;
	[SerializeField] Transform lookSource;
	[SerializeField] LayerMask groundLayer;
	CharacterController controller;
	Rig rig;
	float jumpTimer;
	float lastStick;
	float verticalVelocity;
	bool jumpPressed;
	bool flyPressed;
	bool isFlying;
	
	void Start() {
		controller = GetComponent<CharacterController>();
		rig = GetComponent<Rig>();
	}

	void Update() {
		if (rig.RHand.Input.stick.y.DidFail(-0.5f, rig.RHand.LastInput.stick.y)) { jumpPressed = true; }
		if ((rig.RHand.Input.stickButton ? 1f : 0f).DidReach(1, (rig.RHand.LastInput.stickButton ? 1f : 0f))) { flyPressed = true; }
		
		bool isGrounded = controller.isGrounded;

		if (flyPressed) {
			flyPressed = false;
			if (isFlying) { 
				isFlying = false; 
			} else if (!isGrounded) {
				isFlying = true;
				verticalVelocity = 0f;
			}
		}
		
		Vector3 movement = Vector3.zero;

		if (isFlying) {
			movement += Quaternion.Euler(0, lookSource.eulerAngles.y, 0) * new Vector3(rig.LHand.Input.stick.x, 0, rig.LHand.Input.stick.y) * flightSpeed;
			verticalVelocity = rig.RHand.Input.stick.y * flightSpeed * 0.5f;
			if (isGrounded) { isFlying = false; }
		} else {
			movement += Quaternion.Euler(0, lookSource.eulerAngles.y, 0) * new Vector3(rig.LHand.Input.stick.x, 0, rig.LHand.Input.stick.y) * (isGrounded ? speed : airSpeed);
			
			if (jumpPressed && isGrounded) {
				verticalVelocity = jumpForce;
				jumpPressed = false;
			} else if (!isGrounded) {
				verticalVelocity += gravity * Time.deltaTime;
			} else if (verticalVelocity < 0) {
				verticalVelocity = -2f;
			}
		}

		GrabInteractable L = rig.LHand.held as GrabInteractable;
		GrabInteractable R = rig.RHand.held as GrabInteractable;
		L?.rb.MovePosition(L.rb.position + movement * Time.deltaTime);
		R?.rb.MovePosition(R.rb.position + movement * Time.deltaTime);
		
		movement.y = verticalVelocity;
		controller.Move(movement * Time.deltaTime);
		
		if (rig.RHand.Input.stick.x.DidFail(-0.5f, lastStick)) { transform.rotation *= Quaternion.AngleAxis(-30, Vector3.up); }
		if (rig.RHand.Input.stick.x.DidPass(0.5f, lastStick)) { transform.rotation *= Quaternion.AngleAxis(30, Vector3.up); }
		lastStick = rig.RHand.Input.stick.x;
	}
}
