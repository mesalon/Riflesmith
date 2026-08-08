using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

public class RailPart : Part {
	[SerializeField] Transform start, end;
	[SerializeField] RailMount mount;

	private void Attach(RailMount m) {
		if (m.mountType.Equals(mountType)) {
			mount = m;
			mount.attached.Add(this);
			mount.Register();
			if (mount.receiver) mount.receiver.Reassemble();
			transform.SetParent(mount.transform);
			transform.localPosition = m.GetAttachPoint((start.position + end.position) / 2);
		}
	}

	private void Detach() {
		if (mount) {
			mount.Deregister();
			if (mount.receiver) mount.receiver.Reassemble();
			transform.SetParent(null);
			mount.attached.Remove(this);
			mount = null;
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(RailPart), true), CanEditMultipleObjects]
	public class RailMountEditor : Editor {
		public override VisualElement CreateInspectorGUI() {
			VisualElement root = new();
			var t = (RailPart)target;
			InspectorElement.FillDefaultInspector(root, serializedObject, this);
			ObjectField field = new("Drop to attach") { objectType = typeof(RailMount), };
			field.RegisterValueChangedCallback(e => {
					t.Attach((RailMount)e.newValue);
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
