using System.Collections.Generic;
using UnityEngine;

public abstract class Part : MonoBehaviour {
	public string mountType = "";
	public DeviceInput input;
	[SerializeField] Transform meshSource;
	public List<Mount> children;
	protected GrabInteractable grab;
	protected Mesh mesh;

	public virtual void OnReset() { }
	public virtual void OnAssemble(Receiver receiver) { }

	/*
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
	*/

	/*
	void Awake() {
		if (TryGetComponent(out grab)) { grab.OnHoldFixedE += OnHoldFixed; grab.OnDroppedE += OnDropped; } 
		if (meshSource) mesh = meshSource.GetComponent<MeshFilter>().sharedMesh;
	}
	void OnDestroy() { if (grab) { grab.OnHoldFixedE -= OnHoldFixed; grab.OnDroppedE -= OnDropped; } }
	*/

	protected void RenderGhost(Matrix4x4 matrix) { if(mesh) Graphics.RenderMesh(new RenderParams(GameManager.I.config.outlinePreviewMat), mesh, 0, matrix); }
	protected void SetGhostColor(bool state) { GameManager.I.config.outlinePreviewMat.SetInt("_UseAlt", state ? 1 : 0); }

	/*
	private void OnDropped() {
		if (detectedMount) { 
			Attach(detectedMount); 
			grab.SetDormant(true);
		}
	}
	*/
}

/*
	private void OnHoldFixed() {
		foreach (PointQuery p in PointQuery.overlap) {
			if (p.TryGetComponent(out FixedMount m)) {
				if (!m.attached && mountType.Equals(m.mountType)) {
					float dist = (transform.position - mount.transform.position).sqrMagnitude;
					if (dist < GameManager.I.config.attachmentRange.Sqr()) {
						RenderGhost(m.transform.localToWorldMatrix);
						if (dist < GameManager.I.config.attachmentMountRange.Sqr()) {
							SetGhostColor(true);
							mountOnDrop = m
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
	*/
