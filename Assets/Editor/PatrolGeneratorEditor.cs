using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PatrolGenerator))]
public class PatrolGeneratorEditor : Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();
		PatrolGenerator generator = (PatrolGenerator)target;
		EditorGUILayout.Space();
		if (GUILayout.Button("Bake Visibility Matrix")) {
			generator.GenerateMatrix();
			EditorUtility.SetDirty(generator);
		}
		if (GUILayout.Button("Bake Patrol Points")) {
			generator.SelectPoints();
			EditorUtility.SetDirty(generator);
		}
	}
}
