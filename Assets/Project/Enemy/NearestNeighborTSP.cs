using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "Enemy/TSP")]
public class NearestNeighborTSP : ScriptableObject {
	[SerializeField] int n;
	[SerializeField] Vector3[] points;

	// Flattened from Vector3[,][] pathMap
	[SerializeField] Vector3[] flatPathMap;
	[SerializeField] int[] pathStartIndices;
	[SerializeField] int[] pathLengths;

	// Flattened from float[,] distMap
	[SerializeField] float[] distMap;

	public void Compute(Vector3[] points) {
		n = points.Length;
		this.points = points;
		
		distMap = new float[n * n];
		pathStartIndices = new int[n * n];
		pathLengths = new int[n * n];

		var tempPathList = new List<Vector3>();

		for (int i = 0; i < n; i++) {
			for (int j = 0; j < n; j++) {
				if (i == j) continue;

				int flatIndex = i * n + j;
				var navPath = new UnityEngine.AI.NavMeshPath();

				if (UnityEngine.AI.NavMesh.CalculatePath(points[i], points[j], UnityEngine.AI.NavMesh.AllAreas, navPath) && navPath.status == UnityEngine.AI.NavMeshPathStatus.PathComplete) {
					pathStartIndices[flatIndex] = tempPathList.Count;
					pathLengths[flatIndex] = navPath.corners.Length;
					distMap[flatIndex] = PathLength(navPath.corners);
					tempPathList.AddRange(navPath.corners);
				} else {
					distMap[flatIndex] = float.PositiveInfinity;
					pathStartIndices[flatIndex] = -1;
					pathLengths[flatIndex] = 0;
				}
			}
		}
		flatPathMap = tempPathList.ToArray();
	}

	public Vector3[] GetPath(int start) {
		var finalPath = new List<Vector3> { points[start] };
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

			int flatIndex = currentIndex * n + nearestIdx;
			int pathStart = pathStartIndices[flatIndex];
			int pathLen = pathLengths[flatIndex];
			for (int k = 1; k < pathLen; k++) {
				finalPath.Add(flatPathMap[pathStart + k]);
			}
			
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
