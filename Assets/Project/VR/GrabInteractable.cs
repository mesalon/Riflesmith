using UnityEngine;
using FMODUnity;
using System;

public class GrabInteractable : MonoBehaviour, IInteractable {
	public Interactor Interactor { get; set; }
	public bool PreventInteraction { get; set; }
	public Action<GrabInteractable> onDropped;
	public Action<GrabInteractable> onPicked;
	protected Transform Hand => Interactor.holdPoint;
	protected virtual Pose TargetPose => new(Hand.position, Hand.rotation);
	[HideInInspector] public Rigidbody rb;

	public Transform root;
	public Transform grabPoint;
	[SerializeField] bool doSnapping;
	[SerializeField] EventReference grabSound;

	protected void Awake() {
		if (grabSound.Guid.IsNull) { grabSound = EventReference.Find("event:/Foley"); }
		if(!rb) { rb = root ? root.GetComponent<Rigidbody>() : GetComponent<Rigidbody>(); }

		if (!grabPoint) {
			grabPoint = new GameObject("Auto Grab Point").transform; 
			grabPoint.SetParent(transform, false);
		}
		if (!root) { root = transform; }
	}

	public virtual void OnPicked() {
		RuntimeManager.PlayOneShot(grabSound, grabPoint.position);
	}

	public virtual void OnHold() { }

	public virtual void OnHoldFixed() {
		rb.centerOfMass = root.InverseTransformPoint(grabPoint.position);
		Vector3 targetPosition = TargetPose.position;
		Quaternion targetRotation = TargetPose.rotation;

		if (!doSnapping) {
			targetPosition += Hand.rotation * grabPoint.localPosition;
			targetRotation *= Quaternion.Inverse(root.rotation) * grabPoint.rotation;
		}

		rb.linearVelocity = (targetPosition - grabPoint.position) / Time.fixedDeltaTime;
		Quaternion diff = targetRotation * Quaternion.Inverse(transform.rotation);
		diff.ToAngleAxis(out float angle, out Vector3 axis);
		rb.angularVelocity = axis * (angle.NormalizeAngle() * Mathf.Deg2Rad) / Time.fixedDeltaTime;
	}

	public virtual void OnDropped() { }

	public void AttachTo(Transform t) {
		root.SetParent(t);
		rb.isKinematic = t != null;
	}
}
		//        if (Interactor.other.held is GrabInteractable s && s.transform.root == transform.root && priority > s.priority) {
		//            Vector3 gripOffset = s.grabPoint.position - grabPoint.position;
		//            Vector3 handsDir = s.Hand.position - Hand.position;
		//            Vector3 adjustedDir = handsDir - gripOffset;
		//            targetRotation = Quaternion.LookRotation(adjustedDir, Vector3.Cross(adjustedDir, Hand.right));
		//        }
