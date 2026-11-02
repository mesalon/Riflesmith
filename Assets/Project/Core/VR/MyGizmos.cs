using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using TMPro;
using UnityEngine.TextCore;

public static class MyGizmos {
	private static List<Gizmo> gizmos;
	private static List<Gizmo> labels;
	private static Shader shader;
	private static Mesh quad;
	private static Mesh cube;
	private static Mesh uvsphere;
	private static Mesh icosphere;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	public static void Init() {
		RenderPipelineManager.beginContextRendering -= OnBeforeRender;
		RenderPipelineManager.beginContextRendering += OnBeforeRender;
		RenderPipelineManager.beginCameraRendering -= OnCameraRender;
		RenderPipelineManager.beginCameraRendering += OnCameraRender;
		gizmos = new();
		labels = new();
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
		int size = 50;
		Vector3[] uvVertices = new Vector3[size * size];
		int[] uvTriangles = new int[(size-1) * (size-1) * 6];
		int v = 0;

		for (int y = 0; y < size; y++) {
			float ringAngle = Mathf.PI * y / (size-1);
			float ringHeight = Mathf.Cos(ringAngle);
			float ringSize = Mathf.Sin(ringAngle);
			for (int x = 0; x < size; x++) {
				float segmentAngle = 2 * Mathf.PI / (size-1) * x;
				uvVertices[v] = new(Mathf.Cos(segmentAngle) * ringSize, ringHeight, Mathf.Sin(segmentAngle) * ringSize);
				if (x < size - 1 && y < size - 1) {
					int quad = x + (y*(size-1));
					int t = quad*6;
					uvTriangles[t+0] = v;
					uvTriangles[t+1] = v+size+1;
					uvTriangles[t+2] = v+size;
					uvTriangles[t+3] = v;
					uvTriangles[t+4] = v+1;
					uvTriangles[t+5] = v+size+1;
				}
				v += 1;
			}
		}
		uvsphere = new() { vertices = uvVertices, triangles = uvTriangles, };

		float r = (1 + Mathf.Sqrt(5)) / 2;
		List<Vector3> icoVertices = new() {
			new Vector3(-1,  r,  0).normalized, new Vector3( 1,  r,  0).normalized, new Vector3(-1, -r,  0).normalized, new Vector3( 1, -r,  0).normalized,
			new Vector3( 0, -1,  r).normalized, new Vector3( 0,  1,  r).normalized, new Vector3( 0, -1, -r).normalized, new Vector3( 0,  1, -r).normalized,
			new Vector3( r,  0, -1).normalized, new Vector3( r,  0,  1).normalized, new Vector3(-r,  0, -1).normalized, new Vector3(-r,  0,  1).normalized,
		};
		List<int> icoTriangles = new() {
			0,11,5,  0,5,1,   0,1,7,   0,7,10,  0,10,11,
			1,5,9,   5,11,4,  11,10,2, 10,7,6,  7,1,8,
			3,9,4,   3,4,2,   3,2,6,   3,6,8,   3,8,9,
			4,9,5,   2,4,11,  6,2,10,  8,6,7,   9,8,1,
		};
		Dictionary<(int, int), int> midpoints = new();
		int FindMidpoint(int i, int j) {
			(int, int) edge = i < j ? (i, j) : (j, i);
			if (midpoints.TryGetValue(edge, out int midpoint)) {
				return midpoint;
			} else {
				int idx = icoVertices.Count;
				icoVertices.Add(((icoVertices[i] + icoVertices[j]) / 2).normalized);
				midpoints.Add(edge, idx);
				return idx;
			}
		}
		for (int subs = 0; subs < 3; subs++) {
			List<int> newTriangles = new(icoTriangles.Count * 4);
			for (int i = 0; i < icoTriangles.Count; i += 3) {
				int a = icoTriangles[i];
				int b = icoTriangles[i+1];
				int c = icoTriangles[i+2];
				int ab = FindMidpoint(a, b);
				int bc = FindMidpoint(b, c);
				int ca = FindMidpoint(c, a);
				newTriangles.AddRange(new[] { 
						a, ab, ca,
						ab, b, bc,
						ca, bc, c,
						ab, bc, ca,
						});
			}
			icoTriangles = newTriangles;
		}
		icosphere = new() { vertices = icoVertices.ToArray(), triangles = icoTriangles.ToArray(), };
	}

	private static void OnCameraRender(ScriptableRenderContext context, Camera camera) {
		for (int i = labels.Count - 1; i >= 0; i--) {
			Gizmo label = labels[i];
			Vector3 screenPos = camera.WorldToScreenPoint(label.position);
			Vector3 worldPos = camera.ScreenToWorldPoint(screenPos);
			float unitsPerPixel = Vector3.Distance(camera.ScreenToWorldPoint(screenPos + Vector3.right), worldPos);
			Matrix4x4 matrix = Matrix4x4.TRS(worldPos, camera.transform.rotation, new Vector3(300, 300, 1) * unitsPerPixel);
			Graphics.DrawMesh(label.mesh, matrix, label.mat, 0, camera);
			if (Time.unscaledTime >= label.expiry) { labels.RemoveAt(i); }
		}
	}

	static void OnBeforeRender(ScriptableRenderContext ctx, List<Camera> cams) {
		for (int i = gizmos.Count - 1; i >= 0; i--) {
			Gizmo gizmo = gizmos[i];
			Debug.Log($"Gizmos: {gizmos.Count}, Labels: {labels.Count}");
			Graphics.DrawMesh(gizmo.mesh, Matrix4x4.TRS(gizmo.position, gizmo.rotation, gizmo.scale), gizmo.mat, 0);
			if (Time.unscaledTime >= gizmo.expiry) { gizmos.RemoveAt(i); }
		}
	}

	static void Draw(Mesh mesh, Vector3 position, Quaternion rotation, Vector3 scale, Color color, float lifespan) {
		Material mat = new(shader);
		mat.SetColor(Shader.PropertyToID("_Color"), color);
		gizmos.Add(new Gizmo() { 
				mesh = mesh,
				mat = mat,
				position = position,
				rotation = rotation,
				scale = scale,
				expiry = Time.unscaledTime + lifespan
				});
	}
	public static void DrawQuad(Vector3 position, Quaternion rotation, Vector3 scale, Color color = default, float lifespan = 0) =>
		Draw(quad, position, rotation, scale, Ext.DefaultWhite(color), lifespan);
	public static void DrawCube(Vector3 position, Quaternion rotation, Vector3 scale, Color color = default, float lifespan = 0) =>
		Draw(cube, position, rotation, scale, Ext.DefaultWhite(color), lifespan);
	public static void DrawSphere(Vector3 position, Vector3 scale, Color color = default, float lifespan = 0) =>
		Draw(icosphere, position, Quaternion.identity, scale, Ext.DefaultWhite(color), lifespan);

	public static void Label(Vector3 position, string s, float lifespan = 0) {
		TMP_FontAsset font = TMP_Settings.defaultFontAsset;
		font.TryAddCharacters(s, true);

		var vertices = new Vector3[s.Length * 4];
		var triangles = new int[s.Length * 6];
		var uv = new Vector2[s.Length * 4];
		Vector3 pos = Vector3.zero;
		for (int i = 0; i < s.Length; i++) {
			font.characterLookupTable.TryGetValue(s[i], out TMP_Character ch);

			GlyphMetrics gl = ch.glyph.metrics;
			float scale = 1024;
			vertices[4 * i + 0] = pos + new Vector3(gl.horizontalBearingX, gl.horizontalBearingY, 0) / scale;
			vertices[4 * i + 1] = pos + new Vector3(gl.horizontalBearingX + gl.width, gl.horizontalBearingY, 0) / scale;
			vertices[4 * i + 2] = pos + new Vector3(gl.horizontalBearingX + gl.width, gl.horizontalBearingY - gl.height, 0) / scale;
			vertices[4 * i + 3] = pos + new Vector3(gl.horizontalBearingX, gl.horizontalBearingY - gl.height, 0) / scale;

			float width = font.atlasTexture.width;
			float height = font.atlasTexture.height;
			GlyphRect rect = ch.glyph.glyphRect;
			uv[4 * i + 0] = new(rect.x / width, (rect.y + rect.height) / height);
			uv[4 * i + 1] = new((rect.x + rect.width) / width, (rect.y + rect.height) / height);
			uv[4 * i + 2] = new((rect.x + rect.width) / width, rect.y / height);
			uv[4 * i + 3] = new(rect.x / width, rect.y / height);

			triangles[6 * i + 0] = 4 * i + 0;
			triangles[6 * i + 1] = 4 * i + 1;
			triangles[6 * i + 2] = 4 * i + 2;

			triangles[6 * i + 3] = 4 * i + 0;
			triangles[6 * i + 4] = 4 * i + 2;
			triangles[6 * i + 5] = 4 * i + 3;

			pos += new Vector3(gl.horizontalAdvance / scale, 0, 0);
		}
		Mesh mesh = new() { vertices = vertices, triangles = triangles, uv = uv };
		Material mat = new(shader);
		mat.SetColor(Shader.PropertyToID("_Color"), Color.white);
		labels.Add(new Gizmo() { 
				mesh = mesh,
				mat = font.material,
				position = position,
				expiry = Time.unscaledTime + lifespan
				});
	}
}

public struct Gizmo {
	public Mesh mesh;
	public Material mat;
	public Vector3 position;
	public Quaternion rotation;
	public Vector3 scale;
	public float expiry;
}
