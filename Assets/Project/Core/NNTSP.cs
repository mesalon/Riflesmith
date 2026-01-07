using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

[System.Serializable]
public class NNTSP {
	[SerializeField] Vector3[] points;
	[SerializeField] float[] distMap; // Makeshift 2D array

	public NNTSP(Vector3[] points) {
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
		return finalPath.ToArray();
	}
}
