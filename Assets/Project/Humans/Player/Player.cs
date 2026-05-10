using System;
using UnityEngine;

public class Player : MonoBehaviour, IVRAnchorProvider {
	public CharacterController cc;
	[SerializeField] Transform head;
	[SerializeField] Hand LHand, RHand;
	[SerializeField] Transform headForward;

	public Vector3 Anchor => head.position;

	void Start() {
		VRPlayer.anchorProvider = this;
	}

	void Update() {
		Vector3 delta = VRPlayer.Input.head.position - VRPlayer.LastInput.head.position;
		transform.position += new Vector3(delta.x, 0, delta.z);
		head.localPosition = new Vector3(0, VRPlayer.Input.head.position.y, 0);
		head.rotation = VRPlayer.Input.head.rotation * Quaternion.Inverse(headForward.localRotation);
	}

	void FixedUpdate() {
		cc.height = head.localPosition.y;
		cc.center = new(head.localPosition.x, cc.height / 2, head.localPosition.z);
	}
}

