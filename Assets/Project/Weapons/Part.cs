using UnityEngine;

public abstract class Part : MonoBehaviour {
	public string mountType;
	public Mount mount;
	public Transform mountPoint;
	private Mount detectedMount;
	private GrabInteractable grab;
	[SerializeField] Transform meshSource;
	private Mesh mesh;

	void Awake() { 
		if (TryGetComponent(out grab)) { grab.OnHoldFixedE += OnHoldFixed; grab.OnDroppedE += OnDropped; } 
		mesh = meshSource.GetComponent<MeshFilter>().sharedMesh;
	}
	void OnDestroy() { if (grab) { grab.OnHoldFixedE -= OnHoldFixed; grab.OnDroppedE -= OnDropped; } }

	public abstract void Reset();
	public abstract void OnAssemble(Receiver receiver);

	public void OnHoldFixed() {
		detectedMount = null;
		if (!mount) {
			foreach (PointQuery p in PointQuery.All) {
				float dist = (p.transform.position - mountPoint.transform.position).sqrMagnitude;
				VRGizmos.Line(p.transform.position, mountPoint.transform.position, Color.blue);
				if (dist < 0.25f.Sqr()) {
					if (p.TryGetComponent(out Mount detected) && mountType.Equals(detected.mountType)) {
						Matrix4x4 matrix = Matrix4x4.TRS(-mountPoint.localPosition, meshSource.localRotation, meshSource.localScale);
						Graphics.RenderMesh(new RenderParams(GameManager.I.config.outlinePreviewMat), mesh, 0, p.transform.localToWorldMatrix * matrix);
						if (dist < 0.15f.Sqr()) {
							GameManager.I.config.outlinePreviewMat.SetInt("_UseAlt", 1);
							// todo: Make it snap visually
							detectedMount = detected;
						} else {
							GameManager.I.config.outlinePreviewMat.SetInt("_UseAlt", 0);
						}
						break;
					}
				}
			}
		}
	}

	public void OnDropped() {
		if (detectedMount) { 
			detectedMount.Attach(this); 
			grab.SetDormant(true);
			transform.SetParent(detectedMount.transform);
			transform.localPosition = -mountPoint.localPosition;
			transform.localRotation = Quaternion.Inverse(mountPoint.localRotation);
		}

		/* Detach code
			 rb = gameObject.AddComponent<Rigidbody>();
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
