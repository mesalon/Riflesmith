using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CoverGenerator))]
public class NavmeshBoundsQueryEditor : Editor {
	public override void OnInspectorGUI() {
		DrawDefaultInspector();
		CoverGenerator generator = (CoverGenerator)target;
		EditorGUILayout.Space();
		if (GUILayout.Button("Bake Cover")) {
			generator.Generate();
            EditorUtility.SetDirty(generator);
		}
	}
}
