using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;

public class CoverGenerator : MonoBehaviour {
	public static CoverGenerator I;
	public List<CoverPoint> cover = new();
	public float bodyWidth = 0.3f;
	public float bodyHeight = 1.75f;
	[SerializeField] bool debug;

	private void Awake() {
		if (I != null) {
			Destroy(this);
			return;
		}
		I = this;
	}

	// Todo: nuke code and precompute safety and offense scores for each point
	public void Generate() {
		cover.Clear();
		NavMeshTriangulation mesh = NavMesh.CalculateTriangulation();
		HashSet<(int, int)> boundHash = new();

		for (int i = 0; i < mesh.indices.Length; i += 3) {
			int i0 = mesh.indices[i];
			int i1 = mesh.indices[i + 1];
			int i2 = mesh.indices[i + 2];

			// Edge.
			// Have you ever been on an Edging Streak?
			// Edge.
			// Edge.
			// Do they keep you in a state of Edging? Edge.
			// Edge.
			// When youre not performing your Edging, Do they make you Goon? Edge.
			// Edge.
			// Rizz.
			// Rizz.
			// Whats it like to hold a Gyatt of someone you love? Rizz.
			// Rizz.
			// Do they teach you how to feel? Sigma to Sigma?
			// Rizz.
			// Rizz.
			// Do you long for having your heart Rizzed? Rizz.
			// Rizz.
			// Do you dream about being Rizzed?
			// Rizz.
			// Baby Gronk Rizzed up Livvy Dunne, Rizz.
			// Rizz.
			// Do you feel theres a part of you thats Skibidi?
			// Rizz.
			// Rizz.
			// Skibidi Edge Rizz.
			// Skibidi Edge Rizz.
			// Why dont you say that 3 times? Skibidi Edge Rizz.
			// Skibidi Edge Rizz.
			// Skibidi Edge Rizz.
			void ProcessEdge(int a, int b) {
				(int, int) orderedEdge = a < b ? (a, b) : (b, a);
				// Is the element already in the set? It's a duplicate. Fuck both of them!
				if (!boundHash.Add(orderedEdge)) { boundHash.Remove(orderedEdge); }
			}

			ProcessEdge(i0, i1);
			ProcessEdge(i1, i2);
			ProcessEdge(i2, i0);
		}

		List<(int a, int b)> bounds = boundHash.ToList();
		for (int i = 0; i < bounds.Count; i++) {
			(Vector3 a, Vector3 b) edge = (mesh.vertices[bounds[i].a], mesh.vertices[bounds[i].b]);
			Vector3 tangent = (edge.b - edge.a).normalized;
			Vector3 normal = new(tangent.z, 0, -tangent.x);
			float minLength = bodyWidth * 2;
			CoverPoint coverA = new(edge.a + tangent * bodyWidth, normal);
			CoverPoint coverB = new(edge.b - tangent * bodyWidth, normal);
			if ((edge.b - edge.a).sqrMagnitude > minLength * minLength) {
				cover.Add(coverA);
				cover.Add(coverB);
			} 
			else { cover.Add(new((edge.a + edge.b) / 2, normal)); }
		}
	}

	private void OnDrawGizmosSelected() {
		if (debug && cover != null) {
			foreach (var point in cover) {
				Gizmos.DrawRay(point.position, Vector3.up * 0.1f);
				Gizmos.DrawRay(point.position, new Vector3(point.normal.z, 0, -point.normal.x) * 0.1f);
				Gizmos.DrawRay(point.position, -new Vector3(point.normal.z, 0, -point.normal.x) * 0.1f);
			}
		}
	}
}

[Serializable]
public struct CoverPoint {
	public Vector3 position;
	public Vector3 normal;

	public CoverPoint(Vector3 position, Vector3 normal) {
		this.position = position;
		this.normal = normal;
	}
}

