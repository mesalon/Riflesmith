using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Runtime debug drawing for VR using Graphics.DrawMesh.
/// Call methods every frame you want shapes to appear (immediate mode).
/// </summary>
public static class VRGizmos {
	static Mesh cubeMesh;
	static Mesh sphereMesh;
	static Mesh lineMesh;
	static Material material;

	static Mesh CubeMesh => cubeMesh = cubeMesh == null ? CreateCubeMesh() : cubeMesh;
	static Mesh SphereMesh => sphereMesh = sphereMesh == null ? CreateSphereMesh() : sphereMesh;
	static Mesh LineMesh => lineMesh = lineMesh == null ? CreateLineMesh() : lineMesh;
	static Material Material => material = material == null ? CreateMaterial() : material;

	static Material CreateMaterial() {
		Shader shader = Shader.Find("Custom/Gizmo");
        Material mat = new(shader) {
            hideFlags = HideFlags.HideAndDontSave,
            renderQueue = (int)RenderQueue.Transparent
        };
        return mat;
	}

	static Mesh CreateCubeMesh() {
		Mesh mesh = new() { name = "VRGizmos_Cube" };

		Vector3[] verts = {
			// Front
			new(-0.5f, -0.5f, 0.5f), new(0.5f, -0.5f, 0.5f), new(0.5f, 0.5f, 0.5f), new(-0.5f, 0.5f, 0.5f),
			// Back
			new(0.5f, -0.5f, -0.5f), new(-0.5f, -0.5f, -0.5f), new(-0.5f, 0.5f, -0.5f), new(0.5f, 0.5f, -0.5f),
			// Top
			new(-0.5f, 0.5f, 0.5f), new(0.5f, 0.5f, 0.5f), new(0.5f, 0.5f, -0.5f), new(-0.5f, 0.5f, -0.5f),
			// Bottom
			new(-0.5f, -0.5f, -0.5f), new(0.5f, -0.5f, -0.5f), new(0.5f, -0.5f, 0.5f), new(-0.5f, -0.5f, 0.5f),
			// Right
			new(0.5f, -0.5f, 0.5f), new(0.5f, -0.5f, -0.5f), new(0.5f, 0.5f, -0.5f), new(0.5f, 0.5f, 0.5f),
			// Left
			new(-0.5f, -0.5f, -0.5f), new(-0.5f, -0.5f, 0.5f), new(-0.5f, 0.5f, 0.5f), new(-0.5f, 0.5f, -0.5f)
		};

		int[] tris = {
			0,2,1, 0,3,2,       // Front
			4,6,5, 4,7,6,       // Back
			8,10,9, 8,11,10,    // Top
			12,14,13, 12,15,14, // Bottom
			16,18,17, 16,19,18, // Right
			20,22,21, 20,23,22  // Left
		};

		mesh.vertices = verts;
		mesh.triangles = tris;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		return mesh;
	}

	static Mesh CreateSphereMesh(int segments = 16, int rings = 12) {
		Mesh mesh = new() { name = "VRGizmos_Sphere" };

		int vertCount = (rings + 1) * (segments + 1);
		Vector3[] verts = new Vector3[vertCount];
		int[] tris = new int[rings * segments * 6];

		int v = 0;
		for (int ring = 0; ring <= rings; ring++) {
			float phi = Mathf.PI * ring / rings;
			float y = Mathf.Cos(phi) * 0.5f;
			float r = Mathf.Sin(phi) * 0.5f;

			for (int seg = 0; seg <= segments; seg++) {
				float theta = 2f * Mathf.PI * seg / segments;
				verts[v++] = new Vector3(r * Mathf.Cos(theta), y, r * Mathf.Sin(theta));
			}
		}

		int t = 0;
		for (int ring = 0; ring < rings; ring++) {
			for (int seg = 0; seg < segments; seg++) {
				int curr = ring * (segments + 1) + seg;
				int next = curr + segments + 1;

				tris[t++] = curr;
				tris[t++] = next;
				tris[t++] = curr + 1;

				tris[t++] = curr + 1;
				tris[t++] = next;
				tris[t++] = next + 1;
			}
		}

		mesh.vertices = verts;
		mesh.triangles = tris;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		return mesh;
	}

	static Mesh CreateLineMesh() {
		Mesh mesh = new() { name = "VRGizmos_Line" };

		// Unit segment along local +Z, centered at origin so the same
		// midpoint-TRS convention as the cube places the endpoints at start/end.
		Vector3[] verts = {
			new(0f, 0f, -0.5f),
			new(0f, 0f,  0.5f)
		};

		// Line topology: index pairs define segments. No triangles, no normals.
		int[] indices = { 0, 1 };

		mesh.vertices = verts;
		mesh.SetIndices(indices, MeshTopology.Lines, 0);
		mesh.RecalculateBounds();
		return mesh;
	}

	static MaterialPropertyBlock propertyBlock;
	static MaterialPropertyBlock PropertyBlock => propertyBlock ??= new MaterialPropertyBlock();
	static readonly int colorID = Shader.PropertyToID("_Color");

	static void DrawMesh(Mesh mesh, Matrix4x4 matrix, Color color) {
		PropertyBlock.SetColor(colorID, color);
		Graphics.DrawMesh(mesh, matrix, Material, 0, null, 0, PropertyBlock);
	}

	// === Public API ===

	public static void Line(Vector3 start, Vector3 end, Color color) {
		Vector3 dir = end - start;
		float length = dir.magnitude;
		if (length < 0.0001f) return;

		Vector3 center = (start + end) * 0.5f;
		Quaternion rotation = Quaternion.LookRotation(dir);
		Vector3 scale = new(1f, 1f, length);

		Matrix4x4 matrix = Matrix4x4.TRS(center, rotation, scale);
		DrawMesh(LineMesh, matrix, color);
	}

	public static void Ray(Vector3 origin, Vector3 direction, Color color) {
		Line(origin, origin + direction, color);
	}

	public static void Cube(Vector3 center, Vector3 size, Color color) {
		Matrix4x4 matrix = Matrix4x4.TRS(center, Quaternion.identity, size);
		DrawMesh(CubeMesh, matrix, color);
	}

	public static void Cube(Vector3 center, Quaternion rotation, Vector3 size, Color color) {
		Matrix4x4 matrix = Matrix4x4.TRS(center, rotation, size);
		DrawMesh(CubeMesh, matrix, color);
	}

	public static void Sphere(Vector3 center, float radius, Color color) {
		Matrix4x4 matrix = Matrix4x4.TRS(center, Quaternion.identity, Vector3.one * radius * 2f);
		DrawMesh(SphereMesh, matrix, color);
	}

	// Convenience overloads with default colors
	public static void Line(Vector3 start, Vector3 end) => Line(start, end, Color.green);
	public static void Ray(Vector3 origin, Vector3 direction) => Ray(origin, direction, Color.green);
	public static void Cube(Vector3 center, Vector3 size) => Cube(center, size, Color.green);
	public static void Sphere(Vector3 center, float radius) => Sphere(center, radius, Color.green);

	// Axis helper
	public static void Axis(Vector3 position, float size = 0.1f) {
		Ray(position, Vector3.right * size, Color.red);
		Ray(position, Vector3.up * size, Color.green);
		Ray(position, Vector3.forward * size, Color.blue);
	}

	public static void Axis(Vector3 position, Quaternion rotation, float size = 0.1f) {
		Ray(position, rotation * Vector3.right * size, Color.red);
		Ray(position, rotation * Vector3.up * size, Color.green);
		Ray(position, rotation * Vector3.forward * size, Color.blue);
	}
}
