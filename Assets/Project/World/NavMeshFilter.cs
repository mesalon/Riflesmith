using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class NavMeshFilter : MonoBehaviour {
	public List<Vector3> points = new();
	[SerializeField] NavMeshTriangulation mesh;
	[SerializeField] int start;
	[SerializeField] int inspectTri;
	[SerializeField] int inspectVtx;
	[SerializeField] int triA;
	[SerializeField] int triB;
	[SerializeField] List<int>[] adjacencies;

	[ContextMenu("Filter NavMesh To Largest Island")]
	public void FilterNavMesh() {
		points.Clear();
		Debug.Log("Starting NavMesh filtering process...");
		NavMeshTriangulation mesh = NavMesh.CalculateTriangulation();
		this.mesh = mesh;


	}

	private List<int> Bfs(NavMeshTriangulation mesh) {
		int V = adjacencies.Length;
		int s = start;
		List<int> traversed = new List<int>();
		Queue<int> toVisit = new Queue<int>();
		bool[] visited = new bool[V];
		visited[s] = true;
		toVisit.Enqueue(s);

		while (toVisit.Count > 0) {
			int current = toVisit.Dequeue();
			traversed.Add(current);
			foreach (int x in adjacencies[current]) {
				if (!visited[x]) {
					visited[x] = true;
					toVisit.Enqueue(x);
				}
			}
		}
		return traversed;
	}

	Dictionary<int, List<int>> usageMap = new(); // Map of vertices to the triangles that use it
	private void BuildAdjacencies() {
		usageMap.Clear();
		// IS THIS FUCKER LYING TO ME???
		for (int i = 0; i < mesh.indices.Length; i += 3) {
			int triangle = i / 3;
			if (triangle == inspectTri) { print($"Processing triangle {triangle}"); }
			for (int j = 0; j < 3; j++) {
				int vertex = mesh.indices[i + j];
				usageMap.TryAdd(vertex, new List<int>());
				usageMap[vertex].Add(triangle);
				if (triangle == inspectTri) print($"Adding triangle {triangle} to usage by vertex {vertex}");
			}
		}

		for (int i = 0; i < mesh.indices.Length; i += 3) {
			break;
			HashSet<int> neighbors = new();
			neighbors.UnionWith(usageMap[mesh.indices[i + 0]]);
			neighbors.UnionWith(usageMap[mesh.indices[i + 1]]);
			neighbors.UnionWith(usageMap[mesh.indices[i + 2]]);
			neighbors.Remove(i); // A triangle is not adjacent to itself.
			//adjacencies[i / 3] = new List<int>(neighbors);
		}
	}

	private void OnDrawGizmosSelected() {
		Handles.Label(mesh.vertices[mesh.indices[inspectTri * 3]], $"showing triange {inspectTri}, vertex {mesh.indices[inspectTri * 3]}: used by {string.Join(", ", usageMap[mesh.indices[inspectTri * 3]])}");

		Mesh fullMesh = new Mesh {
			vertices = mesh.vertices,
			triangles = mesh.indices
		};
		fullMesh.RecalculateNormals();
		Gizmos.color = Color.darkCyan * 0.5f;
		Gizmos.DrawWireMesh(fullMesh, Vector3.zero, Quaternion.identity);
		Gizmos.color = Color.green;
		DrawTris(usageMap[triA]);
		Gizmos.color = Color.red;
		DrawTris(new() { triA }, 0.25f);
		Gizmos.color = Color.purple;
		DrawTris(usageMap[triB]);
	}

	void DrawTris(List<int> triangles, float offset = 0) {
		foreach (int triangle in triangles) {
			Mesh triangleMesh = new Mesh {
				vertices = new Vector3[] {
					mesh.vertices[mesh.indices[triangle * 3 + 0]] + offset * Vector3.up,
					mesh.vertices[mesh.indices[triangle * 3 + 1]] + offset * Vector3.up,
					mesh.vertices[mesh.indices[triangle * 3 + 2]] + offset * Vector3.up
				},
				triangles = new int[] { 0, 1, 2 }
			};
			triangleMesh.RecalculateNormals();
			Gizmos.DrawWireMesh(triangleMesh, Vector3.zero, Quaternion.identity);
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(NavMeshFilter))]
	public class NavMeshFilterEditor : Editor {
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			NavMeshFilter filter = target as NavMeshFilter;
			if (GUILayout.Button("Filter NavMesh To Largest Island")) { filter.FilterNavMesh(); }
			if (GUILayout.Button("Build adjacencies")) { 
				filter.FilterNavMesh();
				filter.BuildAdjacencies(); 
			}
		}
	}
#endif
}

