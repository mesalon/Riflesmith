using UnityEngine;

public class Hand : Interactor {
	private HandInput Input => side == Side.Left ? VRPlayer.Input.LHand : VRPlayer.Input.RHand;
	[SerializeField] Side side;
	[SerializeField] HandPose idlePose, grippingPose;
	[SerializeField] Transform[] bones;
	[SerializeField] Transform forward;
	[SerializeField] Transform controller;

	public override void Update() {
		base.Update();
		HandPose.Lerp(idlePose, grippingPose, Input.grip, bones);
		Vector3 pos = Input.position - VRPlayer.Input.head.position.FlattenY();
		Quaternion rot = Input.rotation * Quaternion.Inverse(forward.localRotation);
		transform.SetPose(pos - rot * forward.localPosition, rot, Space.Self);
		controller.SetPose(pos, Input.rotation, Space.Self);
	}

	public void Grip() {
		Collider[] overlap = Physics.OverlapSphere(holdPoint.position, 0.01f);
		foreach (Collider col in overlap) {
			if (held == null && col.TryGetComponent(out IInteractable interactable)) { Pick(interactable); }
		}
	}
}

public enum Side { Left, Right }
