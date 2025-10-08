// Todo: nuke code and precompute safety and offense scores for each point
using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;

public class CoverGenerator : MonoBehaviour {
	public static CoverGenerator I;
	public List<CoverPoint> cover = new();
	public float bodyWidth = 0.3f;
	public float bodyHeight = 1.75f;
	[SerializeField] bool debug;

	private void Awake() {
		if (I != null) { Destroy(this); return; }
		I = this;
	}

	public void Generate() {
		cover.Clear();
		NavMeshTriangulation tri = NavMesh.CalculateTriangulation();
		HashSet<(Vector3, Vector3)> boundHash = new HashSet<(Vector3, Vector3)>(new EdgeComparer());
		for (int i = 0; i < tri.indices.Length; i += 3) {
			for (int j = 0; j < 3; j++) { // Is the element already in the set? It's a duplicate. Fuck both of them!
				Vector3 a = tri.vertices[tri.indices[j]];
				Vector3 b = tri.vertices[tri.indices[(j + 1) % 3]];
				if(!boundHash.Add((a, b))) boundHash.Remove((a, b)); 
			}
		}

		List<(Vector3, Vector3)> bounds = boundHash.ToList();
		for (int i = 0; i < bounds.Count; i++) {
			(Vector3 a, Vector3 b) edge = bounds[i];
			Vector3 tangent = (edge.b - edge.a).normalized;
			Vector3 normal = new(tangent.z, 0, -tangent.x);
			float minLength = bodyWidth * 2;
			CoverPoint coverA = new(edge.a + tangent * bodyWidth, normal);
			CoverPoint coverB = new(edge.b - tangent * bodyWidth, normal);
			if ((edge.b - edge.a).sqrMagnitude > minLength * minLength 
					&& Vector3.Distance(coverA.position, coverB.position) > minLength) {
				cover.Add(coverA);
				cover.Add(coverB);
			} 
			else { cover.Add(new((edge.a + edge.b) / 2, normal)); }
		}
	}

	private void OnDrawGizmosSelected() {
		if (debug && cover != null) {
			foreach (var point in cover) {
				Gizmos.DrawRay(point.position, Vector3.up * 0.1f);
				Gizmos.DrawRay(point.position, new Vector3(point.normal.z, 0, -point.normal.x) * 0.1f);
				Gizmos.DrawRay(point.position, -new Vector3(point.normal.z, 0, -point.normal.x) * 0.1f);
			}
		}
	}
}

[Serializable]
public struct CoverPoint {
	public Vector3 position;
	public Vector3 normal;

	public CoverPoint(Vector3 position, Vector3 normal) {
		this.position = position;
		this.normal = normal;
	}
}

