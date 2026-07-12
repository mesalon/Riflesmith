using System.Collections.Generic;
using UnityEngine;

public class PointQuery : MonoBehaviour {
	public static readonly List<PointQuery> overlap = new();
	void OnEnable() => overlap.Add(this);
	void OnDisable() => overlap.Remove(this);
}
