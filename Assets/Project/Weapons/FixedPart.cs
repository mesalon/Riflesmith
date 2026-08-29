using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

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
	[SerializeField] FixedMount mount;
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
						print("Rendering");
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

	private void Attach(FixedMount m) {
		if (CanAttach(m)) {
			mount = m;
			mount.attached = this;
			mount.Register();
			if (mount.receiver) mount.receiver.Reassemble();
			KillRb();
			transform.SetParent(mount.transform);
			transform.SetPose(AttachPose, Space.Self);
		}
	}

	private void Detach() {
		if (mount) {
			mount.Deregister();
			if (mount.receiver) mount.receiver.Reassemble();
			transform.SetParent(null);
			mount.attached = null;
			mount = null;
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(FixedPart), true), CanEditMultipleObjects]
	public class FixedPartEditor : Editor {
		public sealed override VisualElement CreateInspectorGUI() {
			VisualElement root = new();
			InspectorElement.FillDefaultInspector(root, serializedObject, this);
			BuildInspector(root);
			return root;
		}

		public virtual void BuildInspector(VisualElement root) {
			var t = (FixedPart)target;
			ObjectField field = new("Drop to attach") { objectType = typeof(FixedMount), };
			field.RegisterValueChangedCallback(e => {
					t.Attach((FixedMount)e.newValue);
					field.SetValueWithoutNotify(null);
					});
			root.Add(field);
			root.Add(new Button(() => {
						t.Detach();
						}) { text = "Detach" });
		}
	}
#endif
}
