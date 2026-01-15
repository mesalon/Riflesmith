using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PatrolGenerator : MonoBehaviour {
	public static PatrolGenerator I;
	public List<Vector3> patrolPoints = new();

	[SerializeField] NNTSP tsp;
	[SerializeField] private float voxelSize;
	[SerializeField] private float height;
	[Space]
	[SerializeField] private bool debug;
	[SerializeField] private List<Vector3> witnesses = new();

	private void Awake() {
		if (I != null) { Destroy(this); return; }
		I = this;
	}

	public void GeneratePatrolPoints() {
		patrolPoints.Clear();
		long witnessTimestamp = Ext.Timestamp;
		witnesses.Clear();
		Bounds bounds = AstarPath.active.data.recastGraph.bounds;
		NearestNodeConstraint constraint = NearestNodeConstraint.Walkable;
		constraint.maxDistanceSqr = voxelSize;
		if (voxelSize > 0) {
			for (float x = bounds.min.x; x < bounds.max.x; x += voxelSize) {
				for (float y = bounds.min.y; y < bounds.max.y; y += voxelSize) {
					for (float z = bounds.min.z; z < bounds.max.z; z += voxelSize) {
						NNInfo info = AstarPath.active.GetNearest(new Vector3(x, y, z), constraint);
						if (info.node != null) witnesses.Add(info.position + height * Vector3.up);
					}
				}
			}
		}
		Ext.LogTime(witnessTimestamp, "witness generation");
		if (witnesses.Count == 0) return;

		long matrixTimestamp = Ext.Timestamp;
		var visibilityMatrix = new Dictionary<Vector3, List<Vector3>>(witnesses.Count);
		foreach (Vector3 from in witnesses) {
			var visible = new List<Vector3>();
			foreach (Vector3 to in witnesses) {
				if (!Physics.Linecast(from, to, ~0, QueryTriggerInteraction.Ignore)) {
					visible.Add(to);
				}
			}
			visibilityMatrix[from] = visible;
		}
		Ext.LogTime(matrixTimestamp, "visibility matrix");

		long selectionTimestamp = Ext.Timestamp;
		var unseen = new HashSet<Vector3>(witnesses);

		while (unseen.Count > 0) {
			Vector3 bestPatrolPoint = default;
			List<Vector3> bestVisibleSet = null;
			int maxCoverage = 0;

			foreach (var entry in visibilityMatrix) {
				int coverage = 0;
				foreach (Vector3 point in entry.Value) {
					if (unseen.Contains(point)) {
						coverage++;
					}
				}

				if (coverage > maxCoverage) {
					maxCoverage = coverage;
					bestPatrolPoint = entry.Key;
					bestVisibleSet = entry.Value;
				}
			}

			if (maxCoverage == 0) {
				Debug.LogWarning($"Could not cover all witness points. {unseen.Count} remain.");
				break;
			}

			patrolPoints.Add(bestPatrolPoint);
			foreach (Vector3 newlySeenPoint in bestVisibleSet) {
				unseen.Remove(newlySeenPoint);
			}
		}
		Ext.LogTime(selectionTimestamp, "patrol selection");
	}

	public Vector3[] GetPatrolPath(Vector3 from) {
		int closest = 0;
		float distance = float.MaxValue;
		for (int i = 0; i < patrolPoints.Count; i++) {
			float d = (patrolPoints[i] - from).sqrMagnitude;
			if (d < distance) { closest = i; distance = d; }
		}
		Vector3[] path = tsp.GetPath(closest);
		return path;
	}

	private void OnDrawGizmos() {
		if (!debug) return;
		Gizmos.color = Color.purple;
		if (witnesses != null) {
			foreach (var point in witnesses) { Ext.DrawAxis(point, 0.25f); }
		}
		Gizmos.color = Color.green;
		if (patrolPoints != null) {
			foreach (Vector3 point in patrolPoints) { Gizmos.DrawSphere(point, 0.25f); }
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(PatrolGenerator))]
	public class PatrolGeneratorEditor : Editor {
		public override void OnInspectorGUI() {
			DrawDefaultInspector();
			PatrolGenerator generator = (PatrolGenerator)target;
			EditorGUILayout.Space();
			if (GUILayout.Button("Generate Patrol Points")) {
				generator.GeneratePatrolPoints();
				EditorUtility.SetDirty(generator);
			}
			if (GUILayout.Button("Make TSP")) {
				long tspTimestamp = Ext.Timestamp;
				generator.tsp = new NNTSP(generator.patrolPoints.ToArray());
				generator.tsp.Compute();
				Ext.LogTime(tspTimestamp, "TSP");
			}
		}
	}
#endif
}
