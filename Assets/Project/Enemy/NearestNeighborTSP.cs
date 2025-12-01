using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

[System.Serializable]
public class NearestNeighborTSP {
	[SerializeField] Vector3[] points;
	// Makeshift 2D array
	[SerializeField] float[] distMap;

	public NearestNeighborTSP(Vector3[] points) {
		this.points = points;
	}

	public void Compute() {
		int n = points.Length;

		distMap = new float[n * n];

		var tempPathList = new List<Vector3>();

		for (int i = 0; i < n; i++) {
			for (int j = 0; j < n; j++) {
				if (i == j) continue;
				int flatIndex = i * n + j;
				var path = ABPath.Construct(points[i], points[j], null);
				AstarPath.StartPath(path);
				path.BlockUntilCalculated();

				if (path.error) { distMap[flatIndex] = float.PositiveInfinity; }
				distMap[flatIndex] = path.GetTotalLength();
			}
		}
	}

	public Vector3[] GetPath(int start) {
		long t = Ext.Timestamp;
		var finalPath = new List<Vector3> { points[start] };
		int n = points.Length;
		var visited = new bool[n];
		int currentIndex = start;
		visited[currentIndex] = true;

		for (int i = 1; i < n; i++) {
			float nearestDist = float.PositiveInfinity;
			int nearestIdx = -1;
			for (int j = 0; j < n; j++) {
				if (!visited[j] && distMap[currentIndex * n + j] < nearestDist) {
					nearestDist = distMap[currentIndex * n + j];
					nearestIdx = j;
				}
			}
			if (nearestIdx == -1) break;
			currentIndex = nearestIdx;
			visited[currentIndex] = true;
			finalPath.Add(points[currentIndex]);
		}
		Ext.LogTime(t);
		return finalPath.ToArray();
	}
}
