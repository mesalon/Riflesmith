using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PatrolGenerator))]
public class PatrolGeneratorEditor : Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();
		PatrolGenerator generator = (PatrolGenerator)target;
		EditorGUILayout.Space();
		if (GUILayout.Button("Bake Patrol Routes")) {
			generator.Generate();
			EditorUtility.SetDirty(generator);
		}
	}
}
