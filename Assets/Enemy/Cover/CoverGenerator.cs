using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;

public class CoverGenerator : MonoBehaviour {
	public static CoverGenerator Instance;
	public List<CoverPoint> cover = new();
	public float bodyWidth = 0.3f;
	public float bodyHeight = 1.75f;

	private void Awake() {
		if (Instance != null) {
			Destroy(this);
			return;
		}
		Instance = this;
	}

	public void Generate() {
		cover.Clear();

		NavMeshTriangulation tri = NavMesh.CalculateTriangulation();
		HashSet<(Vector3, Vector3)> boundHash = new();
		for (int i = 0; i < tri.indices.Length; i += 3) {
			Vector3 v0 = tri.vertices[tri.indices[i]];
			Vector3 v1 = tri.vertices[tri.indices[i + 1]];
			Vector3 v2 = tri.vertices[tri.indices[i + 2]];

			// AI slop that guarantees identical ordering of the tuples, important for deduplication
			(Vector3, Vector3) MkEdge(Vector3 a, Vector3 b) => a.x < b.x || (a.x == b.x && a.y < b.y) || (a.x == b.x && a.y == b.y && a.z < b.z) ? (a, b) : (b, a);
			// Is the element already in the set? It's a duplicate. Fuck both of them!
			void ProcessEdge((Vector3, Vector3) edge) { if(!boundHash.Add(edge)) boundHash.Remove(edge); }
			ProcessEdge(MkEdge(v0, v1));
			ProcessEdge(MkEdge(v1, v2));
			ProcessEdge(MkEdge(v2, v0));
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
		if (cover != null) {
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

