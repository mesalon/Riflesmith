using System;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

public class Attachment : GrabInteractable {
	public FirearmReceiver Receiver => mount ? mount.Receiver : null;
	
	[Header("Attachment Settings")]
	public List<AttachmentMount> mounts;
	[SerializeField] Transform mountPoint;
	[SerializeField] BoxCollider detector;
	[SerializeField] EventReference attach;
	[SerializeField] EventReference unattach;
	[SerializeField] string mountType;
	[SerializeField] bool isHandDetachable;
	[SerializeField] bool isGrip;
	[SerializeField] bool attachOnRelease = true;
	private AttachmentMount mount;
	private AttachmentMount detectedMount;
	bool allowAttachment;

	protected override Pose TargetPose {
		get {
			if (detectedMount && detectedMount.type == AttachmentType.Rail) { return GetPointOnRail(); }
			return new(Hand.position, Hand.rotation);
		}
	}

	void Awake() {
		base.Awake();
		if (attach.Guid.IsNull) { attach = EventReference.Find("event:/Mag In"); }
		if (unattach.Guid.IsNull) { unattach = EventReference.Find("event:/Mag Out"); }
	}
	public override void OnPicked() {
		if (Receiver && isHandDetachable) {
			Detach();
			Interactor.Pick(this);
		}
		base.OnPicked();
	}

	protected void Update() {
		if (Interactor) {
			// Only do posing as an attachment when other hand wouldn't conflict with it 
			doPosing = Interactor.other.held is not GrabInteractable grab || grab.transform.root != transform.root; // todo: figure out wtf this does | Update: It makes it so the magazine's pos only changes if its grabbed when *not* loaded into a gun
			if (!mount) {
				Collider[] overlap = Physics.OverlapBox(detector.transform.position, Vector3.Scale(detector.size, detector.transform.lossyScale) / 2, detector.transform.rotation);
				bool wasMountFound = false; // To make it so after detaching, it doesn't immediately attach back (like a magazine)
				foreach (Collider col in overlap) {
					if (col.TryGetComponent(out AttachmentMount detected) && mountType.Equals(detected.mountType)) {
						detectedMount = detected;
						wasMountFound = true;
						break;
					}
				}
				if (!wasMountFound) {
					allowAttachment = true;
					detectedMount = null;
				}
			}
		}

	}

	public override void OnDropped() {
		if (allowAttachment && detectedMount) {
			Quaternion rot = Quaternion.Inverse(mountPoint.transform.localRotation);
			Pose railPoint = GetPointOnRail();
			Pose final = detectedMount.type == AttachmentType.Rail ? 
				new(detectedMount.transform.InverseTransformPoint(railPoint.position), Quaternion.Inverse(detectedMount.transform.rotation) * railPoint.rotation) : 
				new(rot * -mountPoint.transform.localPosition, rot);
			Attach(detectedMount, final);
		}
	}

	private Pose GetPointOnRail() {
		Vector3 start = detectedMount.transform.TransformPoint(new(0, 0, detectedMount.railMin));
		float distance = Vector3.Dot(Hand.position - start, detectedMount.transform.forward);
		return new(start + detectedMount.transform.forward * distance, detectedMount.transform.rotation);
	}
	
	public void Attach(AttachmentMount mountTo, Pose pose = default) {
		RuntimeManager.PlayOneShot(attach, transform.position);
		if(!isHandDetachable && !isGrip) { PreventInteraction = true; }
		mount = mountTo;
		mount.attachment = this;
		transform.SetParent(mount.transform);
		transform.SetPose(pose, true);
		Destroy(rb);
		RefreshRB();
		mount.Receiver?.RefreshAttachments();
	}
	
	public void Detach() {
		rb = gameObject.AddRigidBody();
		transform.SetParent(null);
		PreventInteraction = false;
		mount.attachment = null;
		if(mount.Receiver) mount.Receiver.RefreshAttachments();
		mount = null;
		RuntimeManager.PlayOneShot(unattach, transform.position);
		allowAttachment = false;
	}
	
	void RefreshRB() { // Todo: I hate how this works
		rb = mount.Rb;
		foreach (AttachmentMount mount in mounts) {
			if(mount.attachment) mount.attachment.RefreshRB();
		}
	}

	public virtual void OnAttach(FirearmReceiver f = null) { }
}
