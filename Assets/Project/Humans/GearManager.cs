using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GearManager : MonoBehaviour {
	[SerializeField] List<GearItem> items;
	[SerializeField] Transform root;
	[SerializeField] Transform container;
	private Dictionary<string, Transform> boneMap = new();
	private Human ctx;

	void Awake() {
		ctx = GetComponent<Human>();
	}

	public void Build() {
		for (int i = container.childCount - 1; i >= 0; i--) {
			Undo.DestroyObjectImmediate(container.GetChild(i).gameObject);
		}
		boneMap.Clear();
		boneMap.Add(root.name, root);
		Traverse(root);
		foreach (GearItem entry in items) {
			GearItem item = Instantiate(entry, container);
			item.Init(); // todo: Fuck this for release
			if (item.skins.Count > 0) { item.smr.material = item.skins.GetRandom(); }
			Attach(item);
		}
	}

	void Traverse(Transform root) {
		foreach (Transform bone in root) { 
			boneMap.Add(bone.name, bone);
			Traverse(bone); 
		}
	}

	void Attach(GearItem item) {
		SkinnedMeshRenderer smr = item.smr;
		Transform[] newBones = new Transform[smr.bones.Length];
		for (int i = 0; i < smr.bones.Length; i++) {
			if (boneMap.TryGetValue(smr.bones[i].name, out Transform newBone)) {
				newBones[i] = newBone;
			}
		}
		smr.bones = newBones;
		if (boneMap.TryGetValue(smr.rootBone.name, out Transform newRoot)) {
			Undo.DestroyObjectImmediate(item.root.gameObject);
			smr.rootBone = newRoot;
		}
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
