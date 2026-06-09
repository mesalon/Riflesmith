using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GrabInteractable : MonoBehaviour, IInteractable {
	public Interactor Interactor { get; set; }
	public bool PreventInteraction { get; set; }
	private Rigidbody rb;

	private void Awake() {
		rb = GetComponent<Rigidbody>();
	}

	public virtual void OnPicked() {}
	public virtual void OnHold() { }
	public virtual void OnHoldFixed() { }
	public virtual void OnDropped() { }
}
//        if (Interactor.other.held is GrabInteractable s && s.transform.root == transform.root && priority > s.priority) {
//            Vector3 gripOffset = s.grabPoint.position - grabPoint.position;
//            Vector3 handsDir = s.Hand.position - Hand.position;
//            Vector3 adjustedDir = handsDir - gripOffset;
//            targetRotation = Quaternion.LookRotation(adjustedDir, Vector3.Cross(adjustedDir, Hand.right));
//        }
