using System.Collections.Generic;
using UnityEngine;

public abstract class Part : MonoBehaviour {
	protected Receiver Receiver => mount.receiver;
	public string mountType = "";
	[SerializeField] Transform meshSource;
	protected GrabInteractable grab;
	protected Mesh mesh;
	private Mount detectedMount;
	protected abstract Vector3 Center { get; }

	public virtual void OnReset() { }
	public virtual void OnAssemble(Receiver receiver) { }

	void OnHoldFixed() {
		detectedMount = null;
		if (!mount) {
			foreach (PointQuery p in PointQuery.overlap) {
				if (p.TryGetComponent(out Mount m) && CanAttach(m)) {
					float dist = (m.GetAttachPoint(Center) - Center).sqrMagnitude;
					if (dist < GameManager.I.config.attachmentRange.Sqr()) {
						RenderGhost(m.transform.localToWorldMatrix);
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

	void Awake() {
		if (TryGetComponent(out grab)) { grab.OnHoldFixedE += OnHoldFixed; grab.OnDroppedE += OnDropped; } 
		if (meshSource) mesh = meshSource.GetComponent<MeshFilter>().sharedMesh;
	}
	void OnDestroy() { if (grab) { grab.OnHoldFixedE -= OnHoldFixed; grab.OnDroppedE -= OnDropped; } }

	protected void RenderGhost(Matrix4x4 matrix) { if(mesh) Graphics.RenderMesh(new RenderParams(GameManager.I.config.outlinePreviewMat), mesh, 0, matrix); }
	protected void SetGhostColor(bool state) { GameManager.I.config.outlinePreviewMat.SetInt("_UseAlt", state ? 1 : 0); }
	private bool CanAttach(Mount m) => !m.attached && mountType.Equals(mountType);

	private void OnDropped() {
		if (detectedMount) { 
			Attach(detectedMount); 
			grab.SetDormant(true);
		}
	}
	protected void Attach(Mount m) {
		if (CanAttach(m)) {
			m.attached = this;
			mount = m;
			transform.SetParent(m.transform);
			transform.position = m.GetAttachPoint(Center);
			Register();
			Receiver.Reassemble();
		}
	}
	protected void Detach() {
		if (mount) {
			Deregister();
			Receiver.Reassemble();
			transform.SetParent(null);
			mount.attached = null;
			mount = null;
		}
	}

	private void Register() {
		print($"Registering {this} to {mount}, receiver is {Receiver}");
		if (!Receiver.parts.Contains(this)) Receiver.parts.Add(this);
		foreach (Mount m in children) {
			m.receiver = Receiver;
			if (m.attached) { m.attached.Register(); }
		}
	}

	private void Deregister() {
		print($"Deregistering {this} from {mount}, receiver is {Receiver}");
		if (mount) {
			Receiver.parts.Remove(this);
			foreach (Mount m in children) {
				if (m.attached) { m.attached.Deregister(); }
				m.receiver = null;
			}
		}
	}
}

