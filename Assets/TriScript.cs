using UnityEngine;
using UnityEngine.AI;

public class TriScript : MonoBehaviour {
	NavMeshTriangulation mesh;
	[SerializeField] int i;

	void Awake() {
		mesh = NavMesh.CalculateTriangulation();
	}

	void Update() {
		int j = i * 3;
		Ext.Label(mesh.vertices[mesh.indices[j]], "V0");
		Ext.Label(mesh.vertices[mesh.indices[j + 1]], "V1");
		Ext.Label(mesh.vertices[mesh.indices[j + 2]], "V2");
		print($"Verts: {mesh.vertices.Length}, Areas: {mesh.areas.Length}, Indicies: {mesh.indices.Length}");
	}
}
