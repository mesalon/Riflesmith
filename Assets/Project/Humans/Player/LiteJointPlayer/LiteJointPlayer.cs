using UnityEngine;

public class LiteJointPlayer : MonoBehaviour, IVRAnchorProvider  {
	private Rigidbody rb;
	DeviceInput Input => VRPlayer.Input.head;
	[SerializeField] Transform head;
	[SerializeField] float speed, turnSpeed, legResistance;

	public Pose Anchor => new(head.position, transform.rotation);

	void Awake() {
		rb = GetComponent<Rigidbody>();
		VRPlayer.anchorProvider = this;
	}

	void FixedUpdate() {
		Vector3 headDelta = transform.rotation * (Input.position - VRPlayer.LastInput.head.position);
		rb.MovePosition(rb.position + headDelta.FlattenY());
		rb.MoveRotation(Quaternion.Euler(0, VRPlayer.Input.RHand.stick.x.Deadzone(0.1f) * turnSpeed, 0) * rb.rotation);
		head.localPosition = new(0, Input.position.y, 0);

		Quaternion forward = Quaternion.LookRotation(Vector3.ProjectOnPlane(Input.rotation * transform.rotation * Vector3.forward, Vector3.up));
		Vector3 input = forward * Vector3.ClampMagnitude(new Vector3(VRPlayer.Input.LHand.stick.x, 0, VRPlayer.Input.LHand.stick.y), 1);
		rb.AddForce(input * speed, ForceMode.Acceleration);
		rb.AddForce(-rb.linearVelocity.FlattenY() * legResistance, ForceMode.Acceleration);
	}
}
