using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public static class MyGizmos {
	private static List<Gizmo> gizmos;
	private static Shader shader;
	private static Mesh quad;
	private static Mesh cube;
	private static Mesh sphere;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	public static void Init() {
		RenderPipelineManager.beginContextRendering -= OnBeforeRender;
		RenderPipelineManager.beginContextRendering += OnBeforeRender;
		gizmos = new();
		shader = Shader.Find("Custom/MyGizmo");
		quad = new() {
			vertices = new Vector3[] {
				new(-0.5f, -0.5f, -0.5f),
				new(-0.5f, 0.5f, -0.5f),
				new(0.5f, 0.5f, -0.5f),
				new(0.5f, -0.5f, -0.5f),
			},
			triangles = new int[] {
				0, 1, 2,
				0, 2, 3,
			}
		};
		cube = new() {
			vertices = new Vector3[] {
				new(-0.5f, -0.5f, -0.5f), new(-0.5f, 0.5f, -0.5f), new(0.5f, 0.5f, -0.5f), new(0.5f, -0.5f, -0.5f), 
				new(-0.5f, -0.5f, 0.5f), new(-0.5f, 0.5f, 0.5f), new(0.5f, 0.5f, 0.5f), new(0.5f, -0.5f, 0.5f), 
			},
			triangles = new int[] {
				0, 1, 2,  0, 2, 3,
				7, 6, 5,  7, 5, 4,
				0, 4, 5,  0, 5, 1,
				3, 2, 6,  3, 6, 7,
				1, 5, 6,  1, 6, 2,
				0, 3, 7,  0, 7, 4
			},
		};
		int size = 3;
		Vector3[] vertices = new Vector3[size * size];
		int[] triangles = new int[(size-1) * (size-1) * 6];
		for (int x = 0; x < size; x++) {
			for (int y = 0; y < size; y++) {
				vertices[x+y] = new(x, y, 0);
				int t = x * 6;
				Debug.Log($"{x},{y}, t:{t}, vl:{vertices.Length}, tl: {triangles.Length}, v:{vertices[y]}");
				if (x < size - 1 && y < size - 1) {
					triangles[t+0] = x+y;
					triangles[t+1] = x+y+size;
					triangles[t+2] = x+y+size+1;
					triangles[t+3] = x+y;
					triangles[t+4] = x+y+size+1;
					triangles[t+5] = x+y+1;
				}
			}
		}
		Debug.Log(string.Join("\n", vertices));
		string s = "";
		for(int i = 0; i < triangles.Length; i += 6) {
			s += $"{triangles[i]}, {triangles[i+1]}, {triangles[i+2]},  {triangles[i+3]}, {triangles[i+4]}, {triangles[i+5]}\n";
		}
		Debug.Log(s);
		sphere = new() { vertices = vertices, triangles = triangles, };
	}

	static void OnBeforeRender(ScriptableRenderContext ctx, List<Camera> cams) {
		for (int i = gizmos.Count - 1; i >= 0; i--) {
			Gizmo gizmo = gizmos[i];
			Graphics.DrawMesh(gizmo.mesh, gizmo.matrix, gizmo.mat, 0);
			if (Time.unscaledTime >= gizmo.expiry) {
				gizmos.RemoveAt(i);
			}
		}
	}

	static void Draw(Mesh mesh, Matrix4x4 matrix, Color color, float lifespan) {
		Material mat = new(shader);
		mat.SetColor(Shader.PropertyToID("_Color"), color);
		gizmos.Add(new Gizmo() { 
				mesh = mesh,
				mat = mat,
				matrix = matrix,
				expiry = Time.unscaledTime + lifespan
				});
	}
	public static void DrawQuad(Vector3 position, Quaternion rotation, Vector3 scale, Color color = default, float lifespan = 0) =>
		Draw(quad, Matrix4x4.TRS(position, rotation, scale), Ext.DefaultWhite(color), lifespan);
	public static void DrawCube(Vector3 position, Quaternion rotation, Vector3 scale, Color color = default, float lifespan = 0) =>
		Draw(cube, Matrix4x4.TRS(position, rotation, scale), Ext.DefaultWhite(color), lifespan);
	public static void DrawSphere(Vector3 position, Vector3 scale, Color color = default, float lifespan = 0) =>
		Draw(sphere, Matrix4x4.TRS(position, Quaternion.identity, scale), Ext.DefaultWhite(color), lifespan);

	public static void Label(Vector3 position, string s, float lifespan = 0) { return; }
	public static void Label(Vector3 position, object s, float lifespan = 0) { return; }
}

public struct Gizmo {
	public Mesh mesh;
	public Material mat;
	public Matrix4x4 matrix;
	public float expiry;
}
