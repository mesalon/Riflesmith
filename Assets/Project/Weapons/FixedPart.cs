using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

[DisallowMultipleComponent]
public class FixedPart : Part { // ADD BACK IINTERACTABLE YOU STUPID NIGGER
	[SerializeField] Transform mountPoint;
	private FixedMount mount;

	private void Attach(FixedMount m) {
		if (!m.attached && m.mountType.Equals(mountType)) {
			mount = m;
			mount.attached = this;
			mount.Register();
			if (mount.receiver) mount.receiver.Reassemble();
			transform.SetParent(mount.transform);
			Vector3 pos = Vector3.zero;
			Quaternion rot = Quaternion.identity;
			if (mountPoint) {
				rot = Quaternion.Inverse(mountPoint.localRotation);
				pos = rot * -mountPoint.localPosition;
			}
			transform.localPosition = pos;
			transform.localRotation = rot;
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
	public class FixedMountEditor : Editor {
		public override VisualElement CreateInspectorGUI() {
			VisualElement root = new();
			var t = (FixedPart)target;
			InspectorElement.FillDefaultInspector(root, serializedObject, this);
			ObjectField field = new("Drop to attach") { objectType = typeof(FixedMount), };
			field.RegisterValueChangedCallback(e => {
					t.Attach((FixedMount)e.newValue);
					field.SetValueWithoutNotify(null);
					});
			root.Add(field);
			root.Add(new Button(() => {
						t.Detach();
						}) { text = "Detach" });
			return root;
		}
	}
#endif
}
