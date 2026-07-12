using UnityEngine;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

public class RailPart : Part {
	[SerializeField] Transform start, end;
	protected override Vector3 Center => (start.position + end.position) / 2;
	public List<Mount> children = new();
	private RailMount mount;

	private void Attach(RailMount m) {
		if (m.TryAttach(this, out Vector3 pos)) {
			m.attached.Add(this);
			mount = m;
			transform.SetParent(m.transform);
			transform.position = pos;
			Register();
			Receiver.Reassemble();
		}
	}
	protected void Detach() {
		if (mount) {
			Deregister();
			Receiver.Reassemble();
			transform.SetParent(null);
			mount.attached.Remove(this);
			mount = null;
		}
	}

}

#if UNITY_EDITOR
	[CustomEditor(typeof(RailPart)), CanEditMultipleObjects]
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
