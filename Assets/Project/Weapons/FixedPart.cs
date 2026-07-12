using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

[DisallowMultipleComponent]
public class FixedPart : Part { // ADD BACK IINTERACTABLE YOU STUPID NIGGER
	protected override Vector3 Center => transform.position;
	[SerializeField] Transform mountPoint;

	void OnHoldFixed() {
		foreach (PointQuery p in PointQuery.overlap) {
			if (CanAttach(mount)) {
				if (dist < GameManager.I.config.attachmentRange.Sqr()) {
					onDrop = m;
				}
			}
		}
	}
#if UNITY_EDITOR
	[CustomEditor(typeof(FixedPart)), CanEditMultipleObjects]
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
