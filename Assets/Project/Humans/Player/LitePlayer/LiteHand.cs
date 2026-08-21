using UnityEngine;

public class LiteHand : MonoBehaviour {
	[SerializeField] Side side;
	DeviceInput Input => side == Side.Left ? VRPlayer.Input.LHand : VRPlayer.Input.RHand;
	private Rigidbody held;
	private Pose grabPose;

	void Update() {
		transform.SetPose(Input.position - VRPlayer.Input.head.position.FlattenY(), Input.rotation, Space.Self);
	}

	void FixedUpdate() {
		if (held) {
			held.linearVelocity = (transform.position - held.transform.TransformPoint(grabPose.position)) / Time.fixedDeltaTime;
			(transform.rotation * Quaternion.Inverse(held.rotation * grabPose.rotation)).ToAngleAxis(out float angle, out Vector3 axis);
			held.angularVelocity = angle * Mathf.Deg2Rad / Time.fixedDeltaTime * axis;
		} else if (Input.grip > 0.5f) {
			foreach (Collider col in Physics.OverlapSphere(transform.position, 0.1f)) {
				if (col.TryGetComponent(out GrabInteractable grab)) {
					print($"grab: {grab.name}");
					held = grab.rb;
					grabPose = new(grab.grabPoint.localPosition, grab.grabPoint.localRotation);
					break;
				}
				if (col.attachedRigidbody) {
					held = col.attachedRigidbody;
					Transform t = held.transform;
					grabPose = new(t.InverseTransformPoint(transform.position), transform.rotation * Quaternion.Inverse(t.rotation));
					break;
				}
			}
		}
		if (Input.grip <= 0.5f) { held = null; }
	}
}
