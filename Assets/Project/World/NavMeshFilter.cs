using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class NavMeshFilter : MonoBehaviour {
	public List<Vector3> points = new();
	[SerializeField] NavMeshTriangulation mesh;

	[ContextMenu("Filter NavMesh To Largest Island")]
	public void FilterNavMesh() {
		points.Clear();
		Debug.Log("Starting NavMesh filtering process...");
		NavMeshTriangulation mesh = NavMesh.CalculateTriangulation();
		this.mesh = mesh;

		if (mesh.vertices.Length == 0) {
			Debug.LogWarning("NavMesh has not been baked or is empty. Aborting.");
			return;
		}

		/*

			 Debug.Log($"Found {allIslands.Count} disconnected NavMesh islands.");
			 if (allIslands.Count <= 1) {
			 Debug.Log("No filtering needed as there is only one island (or none).");
			 return;
			 }

		// --- 3. Find the largest island by surface area ---
		List<int> largestIsland = null;
		float maxArea = 0f;

		foreach (List<int> island in allIslands) {
		points.Add(triangulation.vertices[island[0]]);
		float currentArea = island.Sum(triIndex => CalculateTriangleArea(
		triangulation.vertices[triangulation.indices[triIndex * 3]],
		triangulation.vertices[triangulation.indices[triIndex * 3 + 1]],
		triangulation.vertices[triangulation.indices[triIndex * 3 + 2]]
		));

		if (currentArea > maxArea) {
		maxArea = currentArea;
		largestIsland = island;
		}
		}

		Debug.Log($"Largest island has {largestIsland.Count} triangles and an area of {maxArea}.");

*/
		return;

		/*
		// --- 4. Build a new NavMesh from the largest island ---
		var filteredIndices = new List<int>();
		foreach (int triIndex in largestIsland) {
		filteredIndices.Add(triangulation.indices[triIndex * 3]);
		filteredIndices.Add(triangulation.indices[triIndex * 3 + 1]);
		filteredIndices.Add(triangulation.indices[triIndex * 3 + 2]);
		}

		// Create a new NavMeshData object
		var newNavMeshData = new NavMeshData();

		// Sources will define the geometry of our new NavMesh
		var sources = new List<NavMeshBuildSource> {
		new NavMeshBuildSource {
		shape = NavMeshBuildSourceShape.Mesh,
		sourceObject = CreateMeshFromTriangulation(triangulation.vertices, filteredIndices.ToArray()),
		transform = Matrix4x4.identity, // The vertices are already in world space
		area = 0 // Use default area
		}
		};


		NavMeshBuildSettings settings = NavMesh.GetSettingsByID(0);
		NavMeshData n = NavMeshBuilder.BuildNavMeshData(settings, sources, new Bounds(Vector3.negativeInfinity, Vector3.positiveInfinity), Vector3.zero, Quaternion.identity);

		// --- 5. Replace the old NavMesh ---
		NavMesh.RemoveAllNavMeshData();
		NavMesh.AddNavMeshData(n);
		Debug.Log("NavMesh filtering complete! The scene now uses the filtered NavMesh.");
		*/
	}

	private float CalculateTriangleArea(Vector3 p1, Vector3 p2, Vector3 p3) {
		return Vector3.Cross(p2 - p1, p3 - p1).magnitude * 0.5f;
	}

	private Mesh CreateMeshFromTriangulation(Vector3[] vertices, int[] indices) {
		var mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = indices;
		return mesh;
	}

	private void Update() {
		foreach (int i in mesh.areas) { 
			Vector3 v = mesh.vertices[mesh.indices[i]];
			Ext.Label(v, "ISLAND"); 
			print(v);
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(NavMeshFilter))]
	public class NavMeshFilterEditor : Editor {
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			if (GUILayout.Button("Filter NavMesh To Largest Island")) {
				(target as NavMeshFilter)?.FilterNavMesh();
			}
			EditorGUILayout.HelpBox("1. Bake your NavMesh normally using a NavMeshSurface.\n2. Click the button above to process it, removing all but the largest island.", MessageType.Info);
		}
	}
#endif
}

