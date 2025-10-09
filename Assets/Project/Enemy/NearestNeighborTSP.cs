using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;
using System.Linq;

public class NearestNeighborTSP : ScriptableObject {
	[SerializeField] Vector3[] points;
	[SerializeField] Vector3[,][] pathMap;
	[SerializeField] float[,] distMap;

	public NearestNeighborTSP(Vector3[] points) {
		int n = points.Length;
		this.points = points;
		pathMap = new Vector3[n, n][];
		distMap = new float[n, n];

		for (int i = 0; i < n; i++) {
			for (int j = 0; j < n; j++) {
				if (i == j) continue;
				var navPath = new NavMeshPath();
				if (NavMesh.CalculatePath(points[i], points[j], NavMesh.AllAreas, navPath) && navPath.status == NavMeshPathStatus.PathComplete) {
					pathMap[i, j] = navPath.corners;
					distMap[i, j] = PathLength(navPath.corners);
				} else {
					distMap[i, j] = float.PositiveInfinity;
				}
			}
		}

	}

	public Vector3[] GetPath(int start) {
		int n = points.Length;
		var finalPath = new List<Vector3> { points[start] };
		var visited = new bool[n];
		int currentIndex = start;
		visited[currentIndex] = true;
		for (int i = 1; i < n; i++) {
			float nearestDist = float.PositiveInfinity;
			int nearestIdx = -1;
			for (int j = 0; j < n; j++) {
				Debug.Log($"n: {visited}, {distMap}");
				if (!visited[j] && distMap[currentIndex, j] < nearestDist) {
					nearestDist = distMap[currentIndex, j];
					nearestIdx = j;
				}
			}
			if (nearestIdx == -1) break;
			finalPath.AddRange(pathMap[currentIndex, nearestIdx].Skip(1));
			currentIndex = nearestIdx;
			visited[currentIndex] = true;
		}
		return finalPath.ToArray();
	}

	private float PathLength(Vector3[] path) {
		if (path.Length < 2) return 0;
		return path.Zip(path.Skip(1), (a, b) => Vector3.Distance(a, b)).Sum();
	}
}
