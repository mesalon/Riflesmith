// hey. So basically, instead of doing this UTTER FUYCKING NOSNENSE. THIS RETARDEDD FUCKING SHIT. YOU NEED TO fucking oh my god you need to simply steer the hardware so that the two hand grab agrees with itself. Thats the issue, isn't it? You just need to steer is to it agrees. Not literally flop it. <- this guy is retarded geg
using UnityEngine;

public class GrabInteractable : Interactable {
	public DeviceInput Input => hand.Input;
	private Hand Other => hand ? hand.other : null;
	private bool IsTwoHanded => Other.grabJoint && Other.grabJoint.GetComponent<Rigidbody>() == rb;
	[SerializeField] HandPoseObject pose;
	public Transform grabPoint;
	private Quaternion initRot, targetRot, currentRot;
	private Vector3 targetAnchor, targetConAnchor;

	void OnValidate() {
		if (!rb) TryGetComponent(out rb);
	}

	public override void OnPicked() {
		if (grabPoint) {
			initRot = currentRot = Quaternion.Inverse(hand.grabPoint.rotation) * rb.transform.rotation;
			targetRot = Quaternion.Inverse(rb.transform.rotation) * grabPoint.rotation;
			targetAnchor = rb.transform.InverseTransformPoint(grabPoint.position);
			targetConAnchor = hand.grabPoint.localPosition;
		}
		base.OnPicked();
	}

	public override void OnHold() {
		// Lock the SHand to the exact distance at grab time
		if (IsTwoHanded) {
			Vector3 a = rb.transform.TransformPoint(hand.grabJoint.anchor);
			Vector3 b = rb.transform.TransformPoint(Other.grabJoint.anchor);
			float r = (b - a).magnitude;
			Vector3 c = grabPoint.position;
			Vector3 dir = (Other.transform.position - c).normalized * r;
			Vector3 pos = c + dir;
			Other.transform.position = pos;
		}
		base.OnHold();
	}

	public override void OnHoldFixed() {
		if (grabPoint) {
			ConfigurableJoint joint = hand.grabJoint;
			joint.anchor = Vector3.Lerp(joint.anchor, targetAnchor, Time.fixedDeltaTime * hand.grabPosSpeed);
			joint.connectedAnchor = Vector3.Lerp(joint.connectedAnchor, targetConAnchor, Time.fixedDeltaTime * hand.grabPosSpeed);
			currentRot = Quaternion.Slerp(currentRot, targetRot, hand.grabRotSpeed * Time.fixedDeltaTime);
			joint.SetTargetRotationLocal(currentRot, initRot);
		}

		if (IsTwoHanded) {
			float dot = Vector3.Dot(hand.grabPoint.forward, Other.grabPoint.position - hand.grabPoint.position);
			if (dot > 0) {
				Flop(true);
				Vector3 right = Vector3.ProjectOnPlane(hand.transform.right, rb.transform.forward);
				float diff = Vector3.SignedAngle(right, grabPoint.right, rb.transform.forward);
				rb.AddRelativeTorque(new(0, 0, -diff));
			}
		} else {
			Flop(false);
		}
		base.OnHoldFixed();
	}

	public override void OnDropped() {
		Flop(false);
		base.OnDropped();
	}

	private void Flop(bool state) {
		hand.joint.slerpDrive = state ? default : hand.heldDrive; 
		Other.joint.slerpDrive = state ? default : hand.heldDrive; 
	}

	public void SetDormant(bool state) {
		print($"Setting {name} dormant {Application.isPlaying}, {Time.frameCount}");
		if (state) {
			rb.isKinematic = true;
			if (Application.isPlaying) { Destroy(rb); }
			else { DestroyImmediate(rb); }
			rb = null;
		} else {
			rb = gameObject.AddComponent<Rigidbody>();
		}
	}
}
