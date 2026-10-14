using UnityEngine;

[DisallowMultipleComponent, RequireComponent(typeof(GrabInteractable))]
public class FixedPart : Part {
	private Pose AttachPose {
		get {
			Quaternion rot = Quaternion.Inverse(mountPoint.localRotation);
			Vector3 pos = rot * -mountPoint.localPosition;
			return new(pos, rot);
		}
	}
	[SerializeField] Transform mountPoint;
	[SerializeField, OnObjectChanged(nameof(OnAttachedChanged))] FixedMount mount;
	[SerializeField] FixedMount detectedMount;
	private GrabInteractable gi;

	new void Awake() {
		base.Awake();
		gi = GetComponent<GrabInteractable>();
		gi.OnHoldFixed += OnHoldFixed; gi.OnDropped += OnDropped;
		if (mount) KillRb();
	}
	void OnDestroy() { gi.OnHoldFixed -= OnHoldFixed; gi.OnDropped -= OnDropped; }

	void KillRb() => Destroy(GetComponent<Rigidbody>());

	private void OnHoldFixed() {
		detectedMount = null;
		if (!mount) {
			foreach (PointQuery p in PointQuery.overlap) {
				if (p.TryGetComponent(out FixedMount m) && CanAttach(m)) {
					float dist = (m.transform.position - mountPoint.position).sqrMagnitude;
					if (dist < GameManager.I.config.attachmentRange.Sqr()) {
						RenderGhost(m.transform.localToWorldMatrix * mountPoint.worldToLocalMatrix);
						if (dist < GameManager.I.config.attachmentMountRange.Sqr()) {
							SetGhostColor(true);
							detectedMount = m;
							// todo: Make it snap visually
						} else {
							SetGhostColor(false);
						}
						break; // todo: Is this fragile? Would sorting by distance do me good here in case of multiple compatible types in the same area? // edit: Yes it's fragile you fucking idiot. Obviously, it's layed out in a static list.
					}
				}
			}
		}
	}

	private void OnDropped() {
		if (detectedMount) { 
			Attach(detectedMount); 
		}
	}

	private bool CanAttach(FixedMount m) => !m.attached && m.mountType.Equals(mountType);

	private void OnAttachedChanged(FixedMount oldValue, FixedMount newValue) {
		if (oldValue) { Detach(oldValue); }
		if (newValue) { Attach(newValue); }
	}

	private void Attach(FixedMount m) {
		print($"Attaching"); 
		mount = m;
		mount.attached = this;
		mount.Register();
		if (mount.receiver) mount.receiver.Reassemble();
		transform.SetParent(mount.transform);
		transform.SetPose(AttachPose, Space.Self);
	}

	private void Detach(FixedMount m) {
		print($"Detaching"); 
		m.Deregister();
		if (m.receiver) m.receiver.Reassemble();
		transform.SetParent(null);
		m.attached = null;
	}
}
