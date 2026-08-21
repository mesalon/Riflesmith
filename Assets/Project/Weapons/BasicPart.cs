using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

public class BasicPart : Part {

#if UNITY_EDITOR
	[CustomEditor(typeof(BasicPart), true), CanEditMultipleObjects]
	public class BasicPartEditor : Editor {
		public sealed override VisualElement CreateInspectorGUI() {
			VisualElement root = new();
			InspectorElement.FillDefaultInspector(root, serializedObject, this);
			BuildInspector(root);
			return root;
		}

		public virtual void BuildInspector(VisualElement root) {
			var t = (BasicPart)target;
			ObjectField field = new("Drop to attach") { objectType = typeof(Receiver), };
			field.RegisterValueChangedCallback(e => {
				Receiver r = (Receiver)e.newValue;
				if (r && !r.parts.Contains(t)) { r.parts.Add(t); }
				field.SetValueWithoutNotify(null);
			});
			root.Add(field);
		}
	}
#endif
}
