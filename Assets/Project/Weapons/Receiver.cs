using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

public class Receiver : MonoBehaviour {
	public List<Part> parts = new();

	public void Reassemble() {
		print($"Reassembling receiver with parts: {string.Join(", ", parts)}");
		foreach (Part part in parts) { 
			// todo: this fucking sucks.
			//if (part.config != "") { JsonUtility.FromJsonOverwrite(part.config, this); }
			//part.config = JsonUtility.ToJson(this);
		}
		foreach (Part part in parts) { part.OnAssemble(this); }
	}

	public T Find<T>() {
		foreach (Part part in parts) {
			if (part is T found) { return found; }
		}
		return default;
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(Receiver), true), CanEditMultipleObjects]
	public class ReceiverEditor : Editor {
		public override VisualElement CreateInspectorGUI() {
			VisualElement root = new();
			var t = (Receiver)target;
			InspectorElement.FillDefaultInspector(root, serializedObject, this);
			root.Add(new Button(() => {
						t.Reassemble();
						}) { text = "Reassemble" });
			return root;
		}
	}
#endif
}
