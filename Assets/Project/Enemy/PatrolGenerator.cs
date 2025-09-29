using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class PatrolGenerator : MonoBehaviour {
	[SerializeField] float witnessSize;
	[SerializeField] List<Vector3> witnesses;
	[SerializeField] List<Vector3> patrolPoints;
	[SerializeField] private Bounds fit;

	public void Generate() {
		witnesses.Clear();
		patrolPoints.Clear();
		NavMeshTriangulation tri = NavMesh.CalculateTriangulation();
		foreach (Vector3 v in tri.vertices) { fit.Encapsulate(v); }
		HashSet<Vector3> hash = new();
		Vector3 size = fit.max - fit.min;
		if (witnessSize > 0) {
			for (float x = 0; x < size.x; x += witnessSize) {
				for (float y = 0; y < size.y; y += witnessSize) {
					for (float z = 0; z < size.z; z += witnessSize) {
						if (NavMesh.SamplePosition(fit.min + new Vector3(x, y, z), out NavMeshHit hit, 1, ~0)) {
							hash.Add(hit.position);
						}
					}
				}
			}
		}
		witnesses = hash.ToList();
		for (int i = 0; i < tri.indices.Length; i += 3) {
			patrolPoints.Add((tri.vertices[tri.indices[i]] + tri.vertices[tri.indices[i + 1]] + tri.vertices[tri.indices[i + 2]]) / 3);
		}


		HashSet<(Vector3 a, Vector3 b)> boundHash = new();
		List<Vector3> edges = new();
		for (int i = 0; i < tri.indices.Length; i += 3) {
			Vector3 v0 = tri.vertices[tri.indices[i]];
			Vector3 v1 = tri.vertices[tri.indices[i + 1]];
			Vector3 v2 = tri.vertices[tri.indices[i + 2]];

			(Vector3, Vector3) MkEdge(Vector3 a, Vector3 b) => a.x < b.x || (a.x == b.x && a.y < b.y) || (a.x == b.x && a.y == b.y && a.z < b.z) ? (a, b) : (b, a);
			void ProcessEdge((Vector3 a, Vector3 b) edge) {
				if (boundHash.Contains(edge)) { edges.Add((edge.a + edge.b) / 2); }
				boundHash.Add(edge);
			}
			ProcessEdge(MkEdge(v0, v1));
			ProcessEdge(MkEdge(v1, v2));
			ProcessEdge(MkEdge(v2, v0));
		}
	}

	private void OnDrawGizmosSelected() {
		Gizmos.color = Color.purple;
		Gizmos.DrawWireCube(fit.center, fit.size);
		foreach (var point in witnesses) { Gizmos.DrawRay(point, 0.5f * Vector3.up); }
		Gizmos.color = Color.blue;
		foreach (var point in patrolPoints) { Gizmos.DrawRay(point, Vector3.up); }
	}
}
