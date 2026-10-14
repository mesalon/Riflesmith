using UnityEngine;

public class LiteJointHand : MonoBehaviour {
	private DeviceInput Input => side == Side.Left ? VRPlayer.Input.LHand : VRPlayer.Input.RHand;
	private bool IsTwoHand => other.grabJoint && other.grabbedBody == grabbedBody;
	[SerializeField] Side side;
	[SerializeField] Transform palm;
	[SerializeField] LiteJointHand other;
	[SerializeField] LiteJointPlayer player;
	[SerializeField] float palmRange;
	[SerializeField] float heldDriveSpring;
	[SerializeField] float grabTransitionSeconds;
	[SerializeField] float rollSpring, rollDampRatio = 1;
	private float grabT;
	private GrabInteractable grabInteractable;
	private ConfigurableJoint handJoint, grabJoint;
	private Body grabbedBody;
	private Vector3 initGrabPos;
	private Vector3 targetGrabPos;
	private Quaternion targetGrabRot;
	private Rigidbody rb;
	private DeviceInput lastInput;
	private Quaternion itemGrabRotation;
	private Quaternion handGrabRotation;

	void Awake() {
		rb = GetComponent<Rigidbody>();
		handJoint = GetComponent<ConfigurableJoint>();
		itemGrabRotation = Quaternion.identity;
	}

	Color color;
	float visRange;
	void Update() { 
		VRGizmos.Ray(palm.position, palm.forward * visRange, color); 
		if (grabInteractable) {
			grabInteractable.input = Input;
			grabInteractable.OnHold?.Invoke();
		}
		VRGizmos.Axis(transform.position, transform.rotation, 0.05f);
	}

	void FixedUpdate() {
		if (!rb.detectCollisions) rb.detectCollisions = true;
		color = Color.grey;
		visRange = palmRange;

		handJoint.targetPosition = player.playerRotation * (Input.position - VRPlayer.Input.head.position.FlattenY());
		handJoint.targetRotation = player.playerRotation * Input.rotation;

		if (grabInteractable) { 
			grabInteractable.OnHoldFixed?.Invoke(); 
		}

		if (grabJoint) {
			float t = 1 - Mathf.Pow(1 - (grabT / grabTransitionSeconds), 3);
			grabJoint.targetPosition = Vector3.Lerp(initGrabPos, targetGrabPos, t);
			grabJoint.targetRotation = Quaternion.Lerp(Quaternion.identity, targetGrabRot, t);

			if (IsTwoHand) {
				// I don't understand any of this shit. Thank god for gippity.
				// todo: smooth return, degenerate falloff, grabinteractable implementation, parallel axis theorem
				Vector3 forward = itemGrabRotation * Vector3.forward;
				Vector3 forceAxis = grabbedBody.rotation * forward;

				Vector3 up = itemGrabRotation * Vector3.up;
				Vector3 projected = Vector3.ProjectOnPlane(rb.rotation * handGrabRotation * Vector3.up, grabbedBody.TransformDirection(forward));
				float angle = Vector3.SignedAngle(up, grabbedBody.InverseTransformDirection(projected), forward) * Mathf.Deg2Rad;
				Vector3 spring = rollSpring * angle * forceAxis;

				Vector3 tensor = grabbedBody.inertiaTensor;
				Vector3 tensorAxis = Quaternion.Inverse(grabbedBody.inertiaTensorRotation) * forward;
				float moment = tensorAxis.x.Sqr() * tensor.x + tensorAxis.y.Sqr() * tensor.y + tensorAxis.z.Sqr() * tensor.z;
				float relativeVelocity = Vector3.Dot(grabbedBody.angularVelocity, forceAxis) - Vector3.Dot(rb.angularVelocity, forceAxis);
				//Vector3 damper = Ext.CritDamp(rollSpring * moment, rollDampRatio) * relativeVelocity * forceAxis;
				Vector3 damper = Ext.CritDamp(rollSpring, rollDampRatio) * relativeVelocity * forceAxis;
				grabbedBody.AddTorque(spring - damper, ForceMode.Acceleration);

				Quaternion rot = grabbedBody.rotation;
				VRGizmos.Ray(grabbedBody.position, rot * itemGrabRotation * Vector3.right, Color.red);
				VRGizmos.Ray(grabbedBody.position, rot * up, Color.green);
				VRGizmos.Ray(grabbedBody.position, rot * forward, Color.blue);
				VRGizmos.Ray(grabbedBody.position, rb.rotation * Vector3.up, Color.yellow);
				VRGizmos.Ray(grabbedBody.position, projected, Color.white);
				VRGizmos.Label(rb.position, $"{spring:f2} - {damper:f2}\n{spring-damper:f2}", Time.fixedDeltaTime);
			}
		} else {
			Body foundBody = null;
			GrabInteractable foundInteractable = null;
			RaycastHit[] hits = Physics.RaycastAll(palm.position, palm.forward, palmRange);
			System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
			Vector3 hitPos = default;
			foreach (RaycastHit hit in hits) {
				hitPos = hit.point;
				Body hitBody = hit.collider.GetBody();
				if (hitBody) {
					visRange = hit.distance;
					if (!foundBody) {
						foundBody = hitBody;
						color = Color.green;
					}
					if (hit.collider.TryGetComponent(out GrabInteractable grab)) {
						foundInteractable = grab;
						foundBody = hitBody;
						color = Color.purple;
						break;
					}
				}
			}

			if (foundBody && Input.grip.DidReach(0.5f, lastInput.grip)) {
				grabT = 0;
				grabbedBody = foundBody;
				grabJoint = gameObject.AddComponent<ConfigurableJoint>();
				grabJoint.swapBodies = true;
				grabJoint.SetBody(foundBody);
				grabJoint.autoConfigureConnectedAnchor = false;
				grabJoint.rotationDriveMode = RotationDriveMode.Slerp;
				grabJoint.anchor = Vector3.zero;
				grabJoint.targetPosition = -rb.InverseTransformPoint(grabbedBody.position);
				// targetRotation identity already means what it was when it started, so no need to set it here like you do with position.
				rb.detectCollisions = false; // Avoid a Unity bug where joints don't phase if created while touching by manually phasing and unphasing over a tick


				grabJoint.xDrive = grabJoint.yDrive = grabJoint.zDrive = grabJoint.slerpDrive = Drive(heldDriveSpring, Ext.CritDamp(heldDriveSpring));
				if (IsTwoHand) { GrabTwoHand(); other.GrabTwoHand(); }

				initGrabPos = grabJoint.targetPosition;
				// initGrabRot isn't needed because that's always identity
				targetGrabPos = rb.InverseTransformVector(hitPos - grabbedBody.position);
				targetGrabRot = Quaternion.identity;

				if (foundInteractable) {
					grabInteractable = foundInteractable;
					grabInteractable.OnPicked?.Invoke();
					Quaternion rot = Quaternion.Inverse(rb.rotation) * foundBody.rotation;
					targetGrabPos = rot * grabInteractable.grabPoint.localPosition;
					targetGrabRot = rot * grabInteractable.grabPoint.localRotation;
				}
			} 
		}
		if (Input.grip < 0.5f) {
			if (grabJoint) { 
				if (IsTwoHand) { other.grabJoint.slerpDrive = other.grabJoint.xDrive; } 
				Destroy(grabJoint); 
				grabbedBody = null;
			}
			if (grabInteractable) {
				grabInteractable.OnDropped?.Invoke();
				grabInteractable = null;
			}
		}

		grabT += Time.fixedDeltaTime;
		lastInput = Input;
	}

	public void GrabTwoHand() {
		grabJoint.slerpDrive = Drive(0, 0);
		itemGrabRotation = grabbedBody.InverseTransformRotation(Quaternion.LookRotation(other.rb.position - rb.position, rb.rotation * Vector3.up));
		handGrabRotation = transform.InverseTransformRotation(Quaternion.LookRotation(other.transform.position - transform.position, transform.up));
		VRGizmos.Axis(transform.position, grabbedBody.rotation * itemGrabRotation, 1f);
		// grabinteractable has a defined up, here up is just what it is when u grabbed
	}

	private JointDrive Drive(float spring, float damper) => new() { positionSpring = spring, positionDamper = damper, maximumForce = Mathf.Infinity };
}
