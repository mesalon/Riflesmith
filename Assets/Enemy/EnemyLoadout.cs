using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "EnemyGear", menuName = "Enemy/EnemyGear")]
public class EnemyLoadout : ScriptableObject {
}

[Serializable] public class LoadoutEntry {
	public string name;
	public List<GearItem> gearItems;
}

[Serializable] public class GearItem {
	public AnimationCurve frequency = new();
	public GameObject worldObject;
	public List<Material> skins;
	public List<GearItem> attachments;

	private SkinnedMeshRenderer _smr;
	public SkinnedMeshRenderer smr {
		get {
			if (_smr == null && worldObject != null) {
				_smr = worldObject.GetComponent<SkinnedMeshRenderer>();
				//Debug.Log($"Getting smr of {worldObject.name} for the first time . . . Expensive!!");
			}
			return _smr;
		}
		set => _smr = value;
	}
}