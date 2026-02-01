using System.Collections.Generic;
using UnityEngine;

public class GearItem : MonoBehaviour {
	public List<Material> skins;
	[HideInInspector] public SkinnedMeshRenderer smr;

	void Awake() {
		smr = GetComponent<SkinnedMeshRenderer>();
	}
}
