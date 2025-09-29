using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class PatrolGenerator : MonoBehaviour {
	[SerializeField] float witnessSize;
	[SerializeField] List<Vector3> witnesses;
	[SerializeField] private Bounds fit;
	[SerializeField] bool debug;

	public void Generate() {
		witnesses.Clear();
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
	}

	private void OnDrawGizmosSelected() {
		if (!debug) return;
		Gizmos.color = Color.purple;
		Gizmos.DrawWireCube(fit.center, fit.size);
		foreach (var point in witnesses) { Gizmos.DrawRay(point, 0.5f * Vector3.up); }
		Gizmos.color = Color.blue;
	}
}
