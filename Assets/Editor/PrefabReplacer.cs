using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class PrefabReplacer : EditorWindow {
	[SerializeField] List<GameObject> prefabs;
	[SerializeField] SerializedObject sObject;
	[SerializeField] float sus;
	[SerializeField] private VisualTreeAsset tree;

	[MenuItem("Tools/PrefabReplacer")]
	public static void ShowExample() {
		GetWindow<PrefabReplacer>().titleContent = new("PrefabReplacer");
	}

	private void OnEnable() { sObject = new(this); }
	private void OnDisable() { sObject?.Dispose(); }

	public void CreateGUI() {
		tree.CloneTree(rootVisualElement);
		rootVisualElement.Bind(sObject);
		rootVisualElement.Q<Button>("Replace").clicked += () => {
			foreach (Transform t in Selection.transforms) {
				GameObject go = PrefabUtility.InstantiatePrefab(prefabs[Random.Range(0, prefabs.Count)]) as GameObject;
				go.transform.SetPose(t.position, t.rotation);
				DestroyImmediate(t.gameObject);
			}
		};
	}
}