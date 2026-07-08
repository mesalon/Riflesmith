using System;
using UnityEngine;

public class GrabInteractable : MonoBehaviour {
	public Hand Interactor { get; set; }
	public bool PreventInteraction { get; set; }
	public Action OnPickedE, OnHoldE, OnHoldFixedE, OnDroppedE;
	[SerializeField] HandPoseObject hp;
	[HideInInspector] public Rigidbody rb;
	private Quaternion initRot, targetRot, actualRot;

	void Awake() {
		rb = GetComponent<Rigidbody>(); 
	}

	public void OnPicked() {
		OnPickedE?.Invoke();
		actualRot = initRot = rb.rotation;
		targetRot = Interactor.palm.rotation;
	}
	public void OnHold() { 
		OnHoldE?.Invoke();
		ConfigurableJoint other = Interactor.other.grabJoint;
		if (other != null && other.transform.root == transform) {
			Quaternion look = Quaternion.LookRotation(Interactor.other.transform.position - Interactor.transform.position);
		}
	}
	public void OnHoldFixed() {
		OnHoldFixedE?.Invoke();
		ConfigurableJoint joint = Interactor.grabJoint;
		joint.targetPosition = Vector3.Lerp(joint.targetPosition, Vector3.zero, Time.fixedDeltaTime * Interactor.grabPosSpeed);
		actualRot = Quaternion.Slerp(actualRot, targetRot, Interactor.grabRotSpeed * Time.fixedDeltaTime);
		joint.SetTargetRotationLocal(actualRot, initRot);
		
	}
	public void OnDropped() {
		OnDroppedE?.Invoke();
	}
	public void SetDormant(bool state) {
		enabled = !state;
		if (state) { 
			if (Interactor) { Destroy(Interactor.grabJoint); }
			Destroy(rb); 
		} else {
			rb = gameObject.AddComponent<Rigidbody>(); 
		}
	}
}
//        if (Interactor.other.held is GrabInteractable s && s.transform.root == transform.root && priority > s.priority) {
//            Vector3 gripOffset = s.grabPoint.position - grabPoint.position;
//            Vector3 handsDir = s.Hand.position - Hand.position;
//            Vector3 adjustedDir = handsDir - gripOffset;
//            targetRotation = Quaternion.LookRotation(adjustedDir, Vector3.Cross(adjustedDir, Hand.right));
//       }
