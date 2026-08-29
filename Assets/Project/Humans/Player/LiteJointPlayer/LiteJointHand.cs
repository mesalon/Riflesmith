using UnityEngine;

public class LiteJointHand : MonoBehaviour {
	private DeviceInput Input => side == Side.Left ? VRPlayer.Input.LHand : VRPlayer.Input.RHand;
	[SerializeField] Side side;
	[SerializeField] Transform palm;
	[SerializeField] LiteJointHand other;
	[SerializeField] float palmRange;
	[SerializeField] float heldDriveSpring;
	[SerializeField] float grabTransitionSeconds;
	private float grabT;
	private GrabInteractable grabInteractable;
	private ConfigurableJoint handJoint, grabJoint;
	private Body grabBody;
	private Vector3 initGrabPos;
	private Quaternion initGrabRot;
	private Rigidbody rb;
	private DeviceInput lastInput;

	void Awake() {
		rb = GetComponent<Rigidbody>();
		handJoint = GetComponent<ConfigurableJoint>();
	}

	Color color;
	float visRange;
	void Update() { 
		VRGizmos.Ray(palm.position, palm.forward * visRange, color); 
		if (grabInteractable) {
			grabInteractable.OnHold?.Invoke();
		}
		VRGizmos.Axis(transform.position, transform.rotation, 0.05f);
	}

	void FixedUpdate() {
		if (!rb.detectCollisions) rb.detectCollisions = true;

		handJoint.targetPosition = Input.position - VRPlayer.Input.head.position.FlattenY();
		handJoint.targetRotation = Input.rotation;

		color = Color.grey;
		visRange = palmRange;
		JointDrive d;
		if (grabJoint) {
			Transform reference = grabBody.transform;
			if (grabInteractable) {
				grabInteractable.OnHoldFixed?.Invoke();
				reference = grabInteractable.grabPoint;
			}
			d = grabJoint.xDrive;
			if (other.grabJoint && other.grabBody == grabBody) { 
				d.positionSpring = d.positionDamper = 0; 
				other.grabJoint.slerpDrive = d;

				if (side == Side.Right) {
					Vector3 right = Vector3.ProjectOnPlane(transform.right, grabBody.transform.forward);
					float diff = Vector3.SignedAngle(right, reference.right, grabBody.transform.forward);
					grabBody.AddRelativeTorque(new(0, 0, -diff));
				}
			}
			grabJoint.slerpDrive = d;
		} else {
			Body foundBody = null;
			GrabInteractable foundGrab = null;
			RaycastHit[] hits = Physics.RaycastAll(palm.position, palm.forward, palmRange);
			System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
			foreach (RaycastHit hit in hits) {
				if (hit.collider.GetBody()) {
					if (!foundBody) {
						foundBody = hit.collider.GetBody();
						color = Color.green;
						visRange = hit.distance;
					}
					if (hit.collider.TryGetComponent(out GrabInteractable grab)) {
						foundGrab = grab;
						color = Color.purple;
						visRange = hit.distance;
						break;
					}
				}
			}

			if (foundBody && Input.grip.DidReach(0.5f, lastInput.grip)) {
				grabBody = foundBody;
				grabJoint = gameObject.AddComponent<ConfigurableJoint>();
				grabJoint.swapBodies = true;
				grabJoint.SetBody(foundBody);
				grabJoint.autoConfigureConnectedAnchor = false;
				grabJoint.rotationDriveMode = RotationDriveMode.Slerp;
				grabJoint.anchor = Vector3.zero;
				grabJoint.targetPosition = -transform.InverseTransformPoint(grabBody.transform.position);
				rb.detectCollisions = false; // Avoid a Unity bug where joints don't phase if created while touching by manually phasing and unphasing over a tick

				d = grabJoint.xDrive;
				d.positionSpring = heldDriveSpring;
				d.positionDamper = Mathf.Sqrt(heldDriveSpring) * 2;
				grabJoint.xDrive = grabJoint.yDrive = grabJoint.zDrive = grabJoint.slerpDrive = d;

				if (foundGrab) {
					grabInteractable = foundGrab;
					grabInteractable.OnPicked?.Invoke();
					grabT = 0;
					initGrabPos = grabJoint.targetPosition;
					initGrabRot = transform.localRotation;
				}
			} 
		}
		if (Input.grip < 0.5f) {
			if (grabJoint) { 
				Destroy(grabJoint); 
				grabBody = null;
			}
			if (grabInteractable) {
				grabInteractable.OnDropped?.Invoke();
				grabInteractable = null;
			}
		}

		if (grabInteractable && grabInteractable.grabPoint) {
			VRGizmos.Axis(grabInteractable.grabPoint.position, grabInteractable.grabPoint.rotation, 0.05f);
			float t = 1 - Mathf.Pow(1 - (grabT / grabTransitionSeconds), 3);
			//grabJoint.targetPosition = initGrabRot * grabInteractable.grabPoint.localPosition;
			//grabJoint.targetPosition = Vector3.Lerp(initGrabPos, grabInteractable.grabPoint.localPosition, t);
			grabJoint.targetRotation = Quaternion.Inverse(initGrabRot);
			//grabJoint.SetTargetRotationLocal(Quaternion.Euler(e), initGrabRot);
			//grabJoint.SetTargetRotationLocal(Quaternion.Lerp(initGrabRot, Quaternion.Inverse(grabInteractable.grabPoint.localRotation), t), initGrabRot);
		}

		grabT += Time.fixedDeltaTime;
		lastInput = Input;
	}
	[SerializeField] Vector3 e;

}
