using UnityEngine;
using UnityEditor;

public class MeshCleaner : AssetPostprocessor {
	void OnPostprocessModel(GameObject g) {
		CleanNames(g.transform);
	}

	void CleanNames(Transform t) {
		if (t.name.Contains(".0")) { t.name = t.name.Split('.')[0]; }
		foreach (Transform child in t) { CleanNames(child); }
	}
}
