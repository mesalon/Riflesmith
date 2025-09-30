using Random = UnityEngine.Random;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class PatrolGenerator : MonoBehaviour {
	[SerializeField] UnityEngine.Rendering.SerializedDictionary<Vector3, HashSet<Vector3>> visMatrix = new();
	[SerializeField] List<Vector3> final = new();
	[SerializeField] List<Vector3> witnesses = new();
	[SerializeField] float voxelSize;
	[SerializeField] bool debug;
	[SerializeField, Range(0, 1)] float percentShown;
	[SerializeField, Range(0, 1)] float percentShownFinal;
	[SerializeField] float height;
	private Bounds fit;

	public void GenerateMatrix() {
		witnesses.Clear();
		NavMeshTriangulation tri = NavMesh.CalculateTriangulation();
		foreach (Vector3 v in tri.vertices) { fit.Encapsulate(v); }
		HashSet<Vector3> hash = new();
		Vector3 size = fit.max - fit.min;
		if (voxelSize > 0) {
			for (float x = 0; x < size.x; x += voxelSize) {
				for (float y = 0; y < size.y; y += voxelSize) {
					for (float z = 0; z < size.z; z += voxelSize) {
						if (NavMesh.SamplePosition(fit.min + new Vector3(x, y, z), out NavMeshHit hit, 1, ~0)) {
							hash.Add(hit.position + height * Vector3.up);
						}
					}
				}
			}
		}
		witnesses = hash.ToList();

		visMatrix.Clear();
		long matrix = Ext.Timestamp;
		foreach (Vector3 from in witnesses) {
			HashSet<Vector3> visible = new();
			foreach (Vector3 to in witnesses) { 
				if (!Physics.Linecast(from, to)) {
					visible.Add(to); 
				}
			}
			visMatrix.Add(from, visible);
		}
		Ext.LogTime(matrix, "matrix generation");
	}

	public void SelectPoints() {
		final.Clear();
		long t = Ext.Timestamp;
		HashSet<Vector3> unseen = witnesses.ToHashSet();
		foreach (var _ in visMatrix) {
			if (unseen.Count == 0) break;
			KeyValuePair<Vector3, HashSet<Vector3>> heighestKvp = default;
			int heighest = 0;
			foreach (var kvp in visMatrix) {
				int stillVisible = 0;
				foreach (Vector3 point in kvp.Value) { if (unseen.Contains(point)) stillVisible++; }
				if (stillVisible > heighest) {
					heighest = stillVisible;
					heighestKvp = kvp;
				}
			}
			foreach (Vector3 point in heighestKvp.Value) { 
				unseen.Remove(point); 
			}
			print($"Trying to rm from <{heighestKvp.Key}, {heighestKvp.Value.Count}>, unseen left: {unseen.Count}\nRemoving {String.Join("\n", heighestKvp.Value)}");
			final.Add(heighestKvp.Key);
		}
		Ext.LogTime(t, "point selection");
	}

	private void OnDrawGizmosSelected() {
		if (!debug) return;
		Gizmos.color = Color.purple;
		Gizmos.DrawWireCube(fit.center, fit.size);
		foreach (var point in witnesses) { Ext.DrawAxis(point, 0.5f); }
		foreach (var kvp in visMatrix) {
			Random.InitState(kvp.Key.GetHashCode());
			if (Random.value < percentShown) {
				Gizmos.color = Random.ColorHSV();
				foreach (Vector3 v in kvp.Value) {
					Gizmos.DrawLine(kvp.Key, v);
				}
			}
		}
		foreach (Vector3 point in final) {
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(point, 1);
		}
	}
}

