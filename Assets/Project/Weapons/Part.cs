using UnityEngine;

public abstract class Part : GrabInteractable {
	public Mount mount;
	public Transform mountPoint;
	public string mountType;
	public int channel;
	private Mount detectedMount;

	public abstract void Reset();
	public abstract void OnAssemble(Receiver receiver);

	public override void OnHoldFixed() {
		base.OnHoldFixed();
		detectedMount = null;
		if (!mount) {
			Collider[] overlap = Physics.OverlapSphere(mountPoint.position, 0.02f);
			foreach (Collider col in overlap) {
				if (col.TryGetComponent(out Mount detected) && mountType.Equals(detected.mountType)) {
					// todo: Make it snap visually
					detectedMount = detected;
					break;
				}
			}
		}
	}

	public override void OnDropped() {
		if (detectedMount) { detectedMount.Attach(this); }

		/* Detach code
		rb = gameObject.AddComponent<Rigidbody>();
		rb.interpolation = RigidbodyInterpolation.Interpolate;
		rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
		transform.SetParent(null);
		PreventInteraction = false;
		mount.attachment = null;
		if(mount.Receiver) mount.Receiver.RefreshAttachments();
		mount = null;
		RuntimeManager.PlayOneShot(unattach, transform.position);
		allowAttachment = false;
		*/
	}
}
