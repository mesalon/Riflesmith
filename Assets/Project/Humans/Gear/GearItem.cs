using System.Collections.Generic;
using UnityEngine;

public class GearItem : MonoBehaviour {
	public List<Material> skins;
	[HideInInspector] public SkinnedMeshRenderer smr;
	[HideInInspector] public Transform root;

	public void Init() {
		smr = GetComponentInChildren<SkinnedMeshRenderer>();
		root = transform.Find("root");
	}
}
