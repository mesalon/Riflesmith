using UnityEngine;

public class LiteJointHand : MonoBehaviour {
	private DeviceInput Input => side == Side.Left ? VRPlayer.Input.LHand : VRPlayer.Input.RHand;
	[SerializeField] Side side;
	[SerializeField] JointDriveData heldDrive;
	private ConfigurableJoint grabJoint;
	private ConfigurableJoint joint;
	private Rigidbody rb;

	void Awake() {
		rb = GetComponent<Rigidbody>();
		joint = GetComponent<ConfigurableJoint>();
	}

	void FixedUpdate() {
		joint.targetPosition = Input.position - VRPlayer.Input.head.position.FlattenY();
		joint.targetRotation = Input.rotation;
		VRGizmos.Axis(transform.position, Input.rotation);

		if (!grabJoint && Input.grip > 0.5f) {
			foreach (Collider col in Physics.OverlapSphere(transform.position, 0.1f)) {
				print("Found rb");
				Rigidbody objectRb = col.attachedRigidbody;
				if (objectRb && objectRb != rb) {
					grabJoint = objectRb.gameObject.AddComponent<ConfigurableJoint>();
					grabJoint.connectedBody = rb;
					grabJoint.autoConfigureConnectedAnchor = false;
					grabJoint.anchor = objectRb.transform.InverseTransformPoint(rb.position);
					grabJoint.xDrive = grabJoint.yDrive = grabJoint.zDrive = heldDrive;
					grabJoint.rotationDriveMode = RotationDriveMode.Slerp;
					grabJoint.slerpDrive = heldDrive;
					if (col.TryGetComponent(out GrabInteractable grab)) { }
					break;
				}
			}
		}
		if (grabJoint && Input.grip <= 0.5f) { Destroy(grabJoint); }
	}
}
