using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyBuilder : MonoBehaviour {
	public List<LoadoutEntry> loadout;
	public Transform gearContainer;
	public bool overrideTier;
	public List<Transform> nonGear;
	public bool debugs;
	[SerializeField] private float tierOverride;

	private void Start() {
		foreach (Transform t in gearContainer) {
			if (!nonGear.Contains(t)) { t.gameObject.SetActive(false); }
		}
		foreach (LoadoutEntry e in loadout) {
			if(debugs) print($"{name} Going through {e.name}");
			Build(e.gearItems);
		}
	}

	public void Build(List<GearItem> items) {
		float tier = 0;
		if (overrideTier) tier = tierOverride;
		//else if (GameManager.I) tier = GameManager.I.enemyTier;
		float GetProb(GearItem entry) => entry.frequency.keys.Length > 0 ? entry.frequency.Evaluate(tier) : 1;
		
		float total = 0;
		foreach (GearItem entry in items) {
			float prob = GetProb(entry); // In absence of a specified curve, default to 1
			total += prob;
			if (debugs) print($"({name}) Probability for {entry.worldObject.name} is {prob}");
		}

		float current = 0;
		float r = Random.Range(0, total);
		foreach (GearItem entry in items) {
			if (r <= (current += GetProb(entry))) {
				entry.worldObject.SetActive(true);
				Material skin = null;
				if (entry.skins.Count > 0) { // todo: Aesthetic cohesion system
					skin = entry.skins.GetRandom();
					entry.smr.material = skin;
				}
				if (debugs) print($"Enabling {entry.worldObject.name} with {(skin ? $"skin {skin}" : "no skin")}");
				if (entry.attachments.Count > 0) { Build(entry.attachments); }
				break;
			}
		}
	}
}

[CustomEditor(typeof(EnemyBuilder))]
public class EnemyBuilderEditor : Editor {
	public override void OnInspectorGUI() {
		base.OnInspectorGUI();
		if (GUILayout.Button("Rebuild Character")) {
			EnemyBuilder b = (EnemyBuilder)target;
			foreach (Transform t in b.gearContainer) {
				if (!b.nonGear.Contains(t)) {
					t.gameObject.SetActive(false);
				}
			}			foreach (LoadoutEntry e in b.loadout) {
				if(b.debugs) Debug.Log($"{name} Going through {e.name}");
				b.Build(e.gearItems);
			}
		}
	}
}

/* Old
public void Build() {
   	for (int i = gearContainer.childCount - 1; i >= 0; i--) {
   		GameObject go = gearContainer.GetChild(i).gameObject;
   		if (Application.isPlaying) { Destroy(go); }
   		else { Undo.DestroyObjectImmediate(go); }
   	}

   	foreach (EnemyLoadoutEntry2 entry in loadout.loadout2) {
   		if(debugs) print($"({name}) Going through {entry.name} . . .");
   		DoRandom(entry.gear);
   	}
   }

   private void DoRandom(List<GearItem2> gear) {
   	float total = 0;
   	List<float> weights = new();
   	foreach (GearItem2 item in gear) {
   		float prob = item.frequency.Evaluate(GameManager.I ? GameManager.I.enemyTier : tier);
   		total += prob;
   		weights.Add(prob);
   		if(debugs) print($"({name}) Prob for {item.name} is {prob}");
   	}

   	float r = Random.Range(0, total);
   	for (int i = 0; i < weights.Count; i++) {
   		if (r <= weights[i]) {
   			if (!gear[i].isnull) { // This gear is not marked as null/do not spawn
   				if (!gear[i].smr) { gear[i].smr = gear[i].GetComponentInChildren<SkinnedMeshRenderer>(); }
   				GearItem2 inst = Instantiate(gear[i], gearContainer);
   				if(debugs) print($"Picking {inst.name}");
   				Attach(inst.smr, inst.smr.bones, rootBone);
   				if (gear[i].attachments.Count > 0) DoRandom(gear[i].attachments);
   			} else if (debugs) { print("Skipping entry"); }
   			break;
   		}
   		r -= weights[i];
   	}
   } 
   
   	void Attach(SkinnedMeshRenderer smr, Transform[] targetBones, Transform rootBone) {
   	//smr.bones = rootBone.GetComponentsInChildren<Transform>();
   	var newBones = new Transform[targetBones.Length];
   	for (int i = 0; i < targetBones.Length; i++) {
   		foreach (var newBone in rootBone.GetComponentsInChildren<Transform>()) {
   			if (newBone.name == targetBones[i].name) { newBones[i] = newBone; }
   		}
   	}
   	smr.bones = newBones;
   	smr.rootBone = rootBone;
   }
   */
