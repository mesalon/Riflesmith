using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.AI.Navigation;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NavMeshFilter : MonoBehaviour {
	[SerializeField] int start;
	[SerializeField] List<int> island;
	[SerializeField] bool debug;
	private NavMeshTriangulation mesh;

	private void Filter() {
		NavMesh.RemoveAllNavMeshData();
		List<int> newIndices = new();
		foreach (int triIndex in island) {
			for (int i = 0; i < 3; i++) { newIndices.Add(mesh.indices[triIndex * 3 + i]); }
		}
		Mesh m = new();
		m.vertices = mesh.vertices;
		m.SetTriangles(newIndices, 0);
		m.RecalculateBounds();

		NavMeshBuildSource source = new NavMeshBuildSource {
			shape = NavMeshBuildSourceShape.Mesh,
			sourceObject = m,
			transform = Matrix4x4.identity,
			area = 0
		};

		var sources = new List<NavMeshBuildSource> { source };
		NavMeshBuilder.UpdateNavMeshData(GetComponent<NavMeshSurface>().navMeshData, NavMesh.GetSettingsByID(0), sources, m.bounds);

	}

	private void FloodFill() {
		island.Clear();
		List<int>[] adjacencies = BuildAdjacencies();
		int source = start;
		bool[] visited = new bool[adjacencies.Length];
		visited[source] = true;
		Queue<int> toVisit = new();
		toVisit.Enqueue(source);
		while (toVisit.Count > 0) {
			int count = toVisit.Count;
			for (int i = 0; i < count; i++) {
				int current = toVisit.Dequeue();
				island.Add(current);
				foreach (int x in adjacencies[current]) {
					if (!visited[x]) {
						visited[x] = true;
						toVisit.Enqueue(x);
					}
				}
			}
		}
	}

	private List<int>[] BuildAdjacencies() {
		mesh = NavMesh.CalculateTriangulation();
		Dictionary<Vector3, List<int>> usageMap = new(new Vector3Comparer()); // Map of vertices to the triangles that use it
		for (int i = 0; i < mesh.indices.Length; i += 3) {
			int triangle = i / 3;
			for (int j = 0; j < 3; j++) {
				Vector3 vertex = mesh.vertices[mesh.indices[i + j]];
				usageMap.TryAdd(vertex, new List<int>());
				usageMap[vertex].Add(triangle);
			}
		}

		List<int>[] adjacencies = new List<int>[mesh.indices.Length / 3];
		for (int i = 0; i < mesh.indices.Length; i += 3) {
			int triangle = i / 3;
			HashSet<int> neighbors = new(); // Populate this with every triangle that it is adjacent to.
			for (int j = 0; j < 3; j++) { neighbors.UnionWith(usageMap[mesh.vertices[mesh.indices[i + j]]]); }
			neighbors.Remove(triangle); // A triangle is not adjacent to itself.
			adjacencies[triangle] = new List<int>(neighbors);
		}
		return adjacencies;
	}

	private void OnDrawGizmos() {
		if (debug) {
			if (mesh.areas == null) { mesh = NavMesh.CalculateTriangulation(); }
			Gizmos.color = Color.green;
			if (island.Count > 0) DrawTris(island);
		}
	}

	private void DrawTris(List<int> triangles, float offset = 0) {
		if (triangles == null || triangles.Count == 0) { return; }
		var triPool = new int[triangles.Count * 3];
		var vtxPool = new Vector3[triangles.Count * 3];
		int j = 0;
		foreach (int triangle in triangles) {
			for (int i = 0; i < 3; i++) {
				vtxPool[j] = mesh.vertices[mesh.indices[triangle * 3 + i]] + offset * Vector3.up;
				triPool[j] = j;
				j++;
			}
		}

		Mesh combinedMesh = new Mesh { vertices = vtxPool, triangles = triPool };
		combinedMesh.RecalculateNormals();
		Gizmos.DrawWireMesh(combinedMesh);
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(NavMeshFilter))]
	public class NavMeshFilterEditor : Editor {
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			NavMeshFilter filter = target as NavMeshFilter;
			if (GUILayout.Button("Flood Fill Selected")) { filter.FloodFill(); }
			if (GUILayout.Button("Apply to NavMesh")) { filter.Filter(); }
		}
	}
#endif
}
