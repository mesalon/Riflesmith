using UnityEngine;

public class AddForce : MonoBehaviour {
	[SerializeField] Vector3 force, torque;
	private Rigidbody rb;
	void Awake() => rb = GetComponent<Rigidbody>();
	void FixedUpdate() {
		rb.AddTorque(force);
		rb.AddTorque(torque);
	}
}
