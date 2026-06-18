using System;
using UnityEngine;

public class CustomJoint : MonoBehaviour {
	public Rigidbody connectedBody;
	[HideInInspector] public Vector3 targetPosition;
	[HideInInspector] public Quaternion targetRotation;
	private Rigidbody rb;
	[SerializeField] SpringSettings linear, angular;

	void Awake() {
		rb = GetComponent<Rigidbody>();
	}

	void FixedUpdate() {
		Vector3 linError = rb.position - transform.parent.TransformPoint(targetPosition);
		float linDamper = Mathf.Sqrt(linear.spring) * 2 * linear.dampingRatio;
		Vector3 linForce = -linear.spring * linError - linDamper * rb.linearVelocity;
		rb.AddForce(linForce);

		Quaternion deltaRot = rb.rotation * Quaternion.Inverse(transform.parent.rotation * targetRotation);
		deltaRot.ToAngleAxis(out float angle, out Vector3 axis);
		if (angle != 0) {
			Vector3 angError = axis * (angle.NormalizeAngle() * Mathf.Deg2Rad);
			float angDamper = Mathf.Sqrt(angular.spring) * 2 * angular.dampingRatio;
			rb.AddTorque(-angular.spring * angError - angDamper * rb.angularVelocity, ForceMode.Acceleration);
		}

	}
}

[Serializable] public struct SpringSettings {
	public float spring, dampingRatio;
}
