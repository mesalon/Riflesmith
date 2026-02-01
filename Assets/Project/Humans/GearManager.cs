using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GearManager : MonoBehaviour {
	[SerializeField] List<GearItem> items;
	[SerializeField] Transform root;
	private Dictionary<string, Transform> boneMap = new();
	private List<Transform> current = new();
	private Human ctx;

	void Awake() {
		ctx = GetComponent<Human>();
	}

	public void Build() {
		foreach (Transform item in current) { 
			if (item != null) { DestroyImmediate(item.gameObject); }
		}
		current.Clear();
		boneMap.Clear();
		boneMap.Add(root.name, root);
		Traverse(root);
		foreach (GearItem entry in items) {
			GearItem item = Instantiate(entry, transform);
			current.Add(item.transform);
			if (item.skins.Count > 0) { item.smr.material = item.skins.GetRandom(); }
			Attach(item.smr);
		}
	}

	void Traverse(Transform root) {
		foreach (Transform bone in root) { 
			boneMap.Add(bone.name, bone);
			Traverse(bone); 
		}
	}

	void Attach(SkinnedMeshRenderer smr) {
		Transform[] newBones = new Transform[smr.bones.Length];
		foreach(Transform bone in smr.bones) { print(bone.name); }
		for (int i = 0; i < smr.bones.Length; i++) {
			if (boneMap.TryGetValue(smr.bones[i].name, out Transform newBone)) {
				newBones[i] = newBone;
			} else {
				print($"Could not match {smr.bones[i].name} to anything.");
			}
		}
		smr.bones = newBones;
		smr.rootBone = root;
		smr.updateWhenOffscreen = true;
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(GearManager))]
public class GearManagerEditor : Editor {
	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		if (GUILayout.Button("Rebuild Character")) {
			GearManager m = (GearManager)target;
			m.Build();
		}
	}
}
#endif
