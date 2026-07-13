using UnityEditor;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GrabInteractable : MonoBehaviour {
	[SerializeField] List<GrabPoint> grabPoints;
	private List<Interaction> interactions = new();
	private Rigidbody rb;

	private void Awake() {
		rb = GetComponent<Rigidbody>(); 
	}

	public void OnPicked(Hand hand) {
		foreach (GrabPoint grab in grabPoints) {
			if (hand.palm.position.IsInside(grab.test)) {
				print($"Passed test and grabbed {grab.test.transform.parent.name}");
				interactions.Add(new() {
					hand = hand,
					point = grab,
					initRot = rb.rotation,
					actualRot = rb.rotation, 
					targetRot = hand.palm.rotation,
					targetPos = grab.pose.localPosition,
					});
				ConfigurableJoint joint = hand.grabJoint;
				joint.SetTargetRotationLocal(hand.palm.rotation, rb.rotation);
				joint.targetPosition = grab.pose.localPosition;
				return;
			}
		}
		foreach (Interaction i in interactions) {
		}
	}

	public void OnHold() { 
		if (TryGetTwoHand(out Interaction primary, out Interaction secondary)) {
			// Lock the SHand to the exact distance at grab time
			VRGizmos.Sphere(secondary.hand.transform.position, 0.02f, Color.yellow);
			Vector3 a = transform.TransformPoint(primary.hand.grabJoint.targetPosition);
			Vector3 b = transform.TransformPoint(secondary.hand.grabJoint.targetPosition);
			float r = (b - a).magnitude;
			Vector3 c = primary.hand.transform.position;
			Vector3 dir = (secondary.hand.transform.position - c).normalized * r;
			Vector3 pos = c + dir;
			VRGizmos.Sphere(c, 0.02f, Color.blue);
			VRGizmos.Ray(c, dir, Color.blue);
			VRGizmos.Sphere(pos, 0.02f, Color.green);
			secondary.hand.transform.position = pos;
		}
	}

	public bool TryGetTwoHand(out Interaction a, out Interaction b) {
		a = null;
		b = null;
		foreach (Interaction i in interactions) {
			if (a == null && i.point.isTwoHandPrimary) {
				a = i; 
			} else if (b == null) {
				b = i;
			}
		}
		return a != null && b != null;
	}

	public void OnHoldFixed() {
		foreach (Interaction i in interactions) {
			print($"Interaction: {i.point.test.transform.parent.name}, {i.hand.name}");
			ConfigurableJoint joint = i.hand.grabJoint;
			i.actualRot = Quaternion.Slerp(i.actualRot, i.targetRot, i.hand.grabRotSpeed * Time.fixedDeltaTime);
			//joint.SetTargetRotationLocal(i.actualRot, i.initRot);
			//joint.targetPosition = Vector3.Lerp(joint.targetPosition, i.targetPos, Time.fixedDeltaTime * i.hand.grabPosSpeed);
		}
		if (TryGetTwoHand(out Interaction primary, out Interaction secondary)) {
			Flop(true);
			Vector3 right = Vector3.ProjectOnPlane(primary.hand.transform.right, transform.forward);
			float diff = Vector3.SignedAngle(right, transform.right, transform.forward);
			rb.AddRelativeTorque(new(0, 0, -diff));
			print(diff);
		} else {
			Flop(false);
		}

	}

	private void Flop(bool state) {
		foreach (Interaction i in interactions) { i.hand.grabJoint.slerpDrive = state ? default : i.hand.heldDrive; }
	}
	public void OnDropped(Hand hand) { 
		Flop(false);
		interactions.RemoveAll(x => x.hand == hand);
	}
}

public class Interaction {
	public GrabPoint point;
	public Hand hand;
	public Quaternion initRot, targetRot, actualRot;
	public Vector3 targetPos;
}

[System.Serializable] public class GrabPoint {
	public HandPoseObject handPose;
	public Transform pose;
	public Collider test;
	public bool isTwoHandPrimary;
}
