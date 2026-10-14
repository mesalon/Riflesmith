using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PatrolGenerator : MonoBehaviour {
	public static PatrolGenerator I;
	public List<Vector3> patrolPoints = new();
	private List<Vector3> witnesses = new();

	[SerializeField] NNTSP tsp;
	[SerializeField] private float voxelSize;
	[SerializeField] private float height;
	[SerializeField] private bool debug;

	private void Awake() {
		if (I != null) { Destroy(this); return; }
		I = this;
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
				try {
					GeneratePatrolPoints();
					EditorUtility.SetDirty(generator);
				} finally {
					EditorUtility.ClearProgressBar();
				}
			}
			if (GUILayout.Button("Make TSP")) {
				long t = Ext.Timestamp;
				generator.tsp = new NNTSP(generator.patrolPoints.ToArray());
				EditorUtility.SetDirty(generator);
				EditorUtility.ClearProgressBar();
				Ext.LogTime(t, "TSP");
			}
		}

		public void GeneratePatrolPoints() {
			PatrolGenerator p = (PatrolGenerator)target;
			float voxelSize = p.voxelSize;
			float height = p.height;
			List<Vector3> patrolPoints = p.patrolPoints;
			List<Vector3> witnesses = p.witnesses;
			long t = Ext.Timestamp;
			if (voxelSize > 0 && AstarPath.active.data.recastGraph.isScanned) {
				Clear();
				Bounds bounds = AstarPath.active.data.recastGraph.bounds;
				NearestNodeConstraint constraint = NearestNodeConstraint.Walkable;
				constraint.maxDistanceSqr = voxelSize;
				int total1 = (int)(Mathf.Ceil((bounds.max.x - bounds.min.x) / voxelSize) *
						Mathf.Ceil((bounds.max.y - bounds.min.y) / voxelSize) *
						Mathf.Ceil((bounds.max.z - bounds.min.z) / voxelSize));
				int i = 0;
				for (float x = bounds.min.x; x < bounds.max.x; x += voxelSize) {
					for (float y = bounds.min.y; y < bounds.max.y; y += voxelSize) {
						for (float z = bounds.min.z; z < bounds.max.z; z += voxelSize) {
							i++;
							if (i % 100 == 0) EditorUtility.DisplayProgressBar("Bar", $"Generating witnesses... {i} / {total1}", (float)i / total1);
							NNInfo info = AstarPath.active.GetNearest(new Vector3(x, y, z), constraint);
							if (info.node != null) witnesses.Add(info.position + height * Vector3.up);
						}
					}
				}
			} else {
				Debug.Log("Unable to generate witness points.");
				return;
			}

			var visibilityMatrix = new Dictionary<Vector3, List<Vector3>>(witnesses.Count);
			int total2 = witnesses.Count * witnesses.Count;
			int j = 0;
			foreach (Vector3 from in witnesses) {
				var visible = new List<Vector3>();
				foreach (Vector3 to in witnesses) {
					j++;
					if (j % 100 == 0) EditorUtility.DisplayProgressBar("Patrol Generator", $"Caclulating visibility... {j} / {total2}", (float)j / total2);
					if (!Physics.Linecast(from, to, ~0, QueryTriggerInteraction.Ignore)) { visible.Add(to); }
				}
				visibilityMatrix[from] = visible;
			}

			var unseen = new HashSet<Vector3>(witnesses);
			int total3 = unseen.Count * unseen.Count;
			int l = 0;
			while (unseen.Count > 0) {
				Vector3 bestPatrolPoint = default;
				List<Vector3> bestVisibleSet = null;
				int maxCoverage = 0;

				foreach (var entry in visibilityMatrix) {
					l++;
					if (l % 100 == 0) EditorUtility.DisplayProgressBar("Patrol Generator", $"Calculating coverage... {l} / {total3}", (float)l / total3);
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
			Ext.LogTime(t, "witness generation");
		}

		void Clear() {
			PatrolGenerator p = (PatrolGenerator)target;
			p.patrolPoints.Clear();
			p.witnesses.Clear();
		}
	}
#endif
}
