using System.Collections.Generic;
using UnityEngine;

public class PointQuery : MonoBehaviour {
	public static readonly List<PointQuery> All = new();
	void OnEnable()  => All.Add(this);
	void OnDisable() => All.Remove(this);
}
