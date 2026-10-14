using System.Collections.Generic;
using UnityEngine;

public class Human : MonoBehaviour {
	public Vector3 Center {
		get {
			Vector3 all = Vector3.zero;
			foreach (Transform p in positionPoints) { all += p.position; }
			return all / positionPoints.Length;
		}
	}
	public int IFF;
	[SerializeField] Transform[] positionPoints;

	public static List<Human> All = new();
	void OnEnable() => All.Add(this);
	void OnDisable() => All.Remove(this);
}
