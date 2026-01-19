using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using System.Linq;

public static class Ext {
	public static List<LabelRequest> labelQueue = new();
	public static List<DrawRequest> drawQueue = new();
	public static int FixedFrameCount => Mathf.RoundToInt(Time.fixedTime / Time.fixedDeltaTime);
	public static long Timestamp => Stopwatch.GetTimestamp();

	// todo: combine all these into one somehow
	public static bool DidPass(this float current, float amount, float previous) => current > amount && previous <= amount;
	public static bool DidFail(this float current, float amount, float previous) => current < amount && previous >= amount;
	public static bool DidReach(this float current, float amount, float previous) => current >= amount && previous < amount;
	public static bool DidRecede(this float current, float amount, float previous) => current <= amount && previous > amount;

	public static T? TryIndex<T>(this T[] array, int index) {
		return (index >= 0 && index < array.Length) ? array[index] : default;
	}

	public static double LogTime(this long stamp, string msg = "", bool message = true) {
		double ms = (Stopwatch.GetTimestamp() - stamp) * 1000.0 / Stopwatch.Frequency;
		if (message) Debug.Log($"Execution of {(msg == String.Empty ? "method" : msg)} took {ms} ms");
		return ms;
	}

	public static StringBuilder AppendLines(this StringBuilder sb, params object[] lines) {
		foreach (object o in lines) { sb.AppendLine(o?.ToString()); }
		return sb;
	}

	public static string DebugFields(object data) {
		string str = "";
		str += $"--- Data of: {data.GetType().Name} ---";
		BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
		foreach (FieldInfo field in data.GetType().GetFields(flags)) {
			object value = field.GetValue(data);
			str += $"\n{field.Name} ({field.FieldType.Name}): {value ?? "null"}";
		}
		return str;
	}

	public static float GetPathLength(this IEnumerable<Vector3> path) {
		float length = 0;
		using var enumerator = path.GetEnumerator();
		Vector3 previous = enumerator.Current;
		while (enumerator.MoveNext()) {
			Vector3 current = enumerator.Current;
			length += Vector3.Distance(previous, current);
			previous = current;
		}
		return length;
	}

	public static float Remap(this float value, float min, float max, float newMin, float newMax) => Mathf.Lerp(newMin, newMax, Mathf.InverseLerp(min, max, value));

	public static void SetPose(this Transform transform, Vector3 pos, Quaternion rot, bool isLocal = false) {
		if (isLocal) { transform.SetLocalPositionAndRotation(pos, rot); }
		else { transform.SetPositionAndRotation(pos, rot); }
	} 
	public static void SetPose(this Transform transform, Pose pose, bool isLocal = false) {
		if (isLocal) { transform.SetLocalPositionAndRotation(pose.position, pose.rotation); }
		else { transform.SetPositionAndRotation(pose.position, pose.rotation); }
	} 

	public static void SetLayerRecursive(this GameObject gameObject, int layer) {
		foreach (Transform child in gameObject.transform) { child.gameObject.SetLayerRecursive(layer); }
		gameObject.layer = layer;
	}

	public static void IgnoreCollisionsBetween(List<Collider> first, List<Collider> second, bool ignore) {
		foreach (Collider col in first) {
			foreach (Collider other in second) { Physics.IgnoreCollision(col, other, ignore); }
		}
	}

#if UNITY_EDITOR
	public static void AddToPropertyList(this SerializedProperty list, Object value) {
		list.arraySize++;
		list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
	}
#endif

	public static void Label(Vector3 position, string text, float lifespan = 0, GUIStyle style = null, Color color = default) {
		style ??= new() {
			alignment = TextAnchor.MiddleCenter, 
			normal = new() { textColor = color == default ? Color.white : color },
			fontSize = 12,
		};
		labelQueue.Add(new() { position = position, text = text, lifespan = lifespan, style = style, color = color });
	}

	public static float NormalizeAngle(this float angle) => angle % 360 <= 180 ? angle % 360 : angle % 360 - 360;

	public static Vector3 Midpoint(this IEnumerable<Vector3> points) {
		Vector3 all = default;
		int count = 0;
		foreach (Vector3 point in points) {
			all += point;
			count++;
		}
		return count > 0 ? all / count : default;
	}

	public static void PrintAll<T>(this IEnumerable<T> objects) { Debug.Log(string.Join(", ", objects)); }

	public static void DrawAxis(Vector3 position, float radius, Quaternion rotation = default, Color color = default) {
		if (color == default) { color = Color.white; }
		Vector3 right = rotation * Vector3.right;
		Vector3 up = rotation * Vector3.up;
		Vector3 forward = rotation * Vector3.forward;
		Gizmos.DrawRay(position - radius * right, radius * 2 * right);
		Gizmos.DrawRay(position - radius * up, radius * 2 * up);
		Gizmos.DrawRay(position - radius * forward, radius * 2 * forward);
	}

	public static void DrawPath(IEnumerable<Vector3> path, Gradient gradient = null, float offset = 0) {
		Vector3[] pathArr = path.ToArray();
		for (int i = 0; i < pathArr.Length - 1; i++) {
			float t = (float)i / pathArr.Length;
			Color c = gradient != null ? gradient.Evaluate(t) : Color.Lerp(Color.blue, Color.yellow, t);
			Debug.DrawLine(pathArr[i] + Vector3.up * offset, pathArr[i + 1] + Vector3.up * offset, c, i / ((float)pathArr.Length - 1));
		}
	}

	public static void Draw(Action action, float lifespan = 0) { drawQueue.Add(new() { action = action, lifespan = lifespan }); }

	public static void DrawCube(Vector3 position, Quaternion rotation, Vector3 scale, Color color) {
		Draw(() => {
				Gizmos.color = color;
				Gizmos.matrix = Matrix4x4.TRS(position, rotation, scale);
				Gizmos.DrawCube(Vector3.zero, Vector3.one); 
				Gizmos.matrix = Matrix4x4.identity;
				});
	}

	public static void DrawCubeLine(Vector3 start, Vector3 end, Color color, float lifespan = 0, float thickness = 0.05f) {
		Draw(() => {
				Vector3 dir = (end - start).normalized;
				Quaternion rotation = dir != Vector3.zero ? Quaternion.LookRotation(dir, Vector3.up) : Quaternion.identity;
				Gizmos.matrix = Matrix4x4.TRS((start + end) / 2f, rotation, Vector3.one);
				Gizmos.color = color;
				Gizmos.DrawCube(Vector3.zero, new Vector3(thickness, thickness, (end - start).magnitude));
				Gizmos.matrix = Matrix4x4.identity;
				}, lifespan);
	}

	public static void DrawCubeRay(Vector3 start, Vector3 dir, Color color, float lifespan = 0, float thickness = 0.05f) {
		Vector3 end = start + dir;
		DrawCubeLine(start, end, color, lifespan, thickness);
	}

	public static Vector3 FlattenY(this Vector3 vector) => new(vector.x, 0, vector.z);

	public static Color MultiLerp(Color[] colors, float t) {
		int count = colors.Length;
		GradientColorKey[] colorKeys = new GradientColorKey[count];
		GradientAlphaKey[] alphaKeys = new GradientAlphaKey[count];
		for (int i = 0; i < count; i++) {
			float time = (float)i / (count - 1);
			colorKeys[i] = new GradientColorKey(colors[i], time);
			alphaKeys[i] = new GradientAlphaKey(colors[i].a, time);
		}
		Gradient gradient = new Gradient();
		gradient.SetKeys(colorKeys, alphaKeys);
		return gradient.Evaluate(t);
	}

	public static T GetRandom<T>(this List<T> list) {
		if (list == null || list.Count == 0) { return default(T); }
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public static T PopRandom<T>(this List<T> list) {
		if (list == null || list.Count == 0) { return default(T); }
		int randI = UnityEngine.Random.Range(0, list.Count);
		T chosenItem = list[randI];
		list[randI] = list[list.Count - 1];
		list.RemoveAt(list.Count - 1);
		return chosenItem;
	}

	public static IEnumerable<T> GetRandomSubset<T>(this IEnumerable<T> source, int size) {            
		List<T> list = new List<T>();            
		foreach (T item in source) { list.Add(item); }
		if (size >= list.Count) { return new List<T>(list); }            
		List<T> copy = new List<T>(list);            
		int n = copy.Count;            
		// Partial Fisher Yates shuffle            
		for (int i = 0; i < size; i++) {                
			int randIndex = UnityEngine.Random.Range(i, n);                
			T temp = copy[i];                
			copy[i] = copy[randIndex];                
			copy[randIndex] = temp;            
		}            
		return copy.GetRange(0, size);        
	}

	public static void Shuffle<T>(this IList<T> list) {
		int n = list.Count;
		while (n > 1) {
			n--;
			int k = UnityEngine.Random.Range(0, n + 1);
			T value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
	}

	public static T IndexLooped<T>(this T[] array, int i) {
		int r = i % array.Length;
		return array[r < 0 ? r + array.Length : r];
	}

	public static Vector3 AngleTo(this Vector3 current, Vector3 target, float degrees) {
		Vector3 axis = Vector3.Cross(current, target);
		if (axis == Vector3.zero) axis = Vector3.up; 
    return Quaternion.AngleAxis(degrees, axis) * current;
	}

	public static Vector3 GetInertiaTensor(this Bounds shape, Vector3 pivot, float mass) {
		Vector3 baseTensor = new(
				1f / 12f * mass * (shape.size.y * shape.size.y + shape.size.z * shape.size.z),
				1f / 12f * mass * (shape.size.x * shape.size.x + shape.size.z * shape.size.z),
				1f / 12f * mass * (shape.size.x * shape.size.x + shape.size.y * shape.size.y));
		Vector3 r = shape.center - pivot;
		Vector3 rSquared = new Vector3(
				r.y * r.y + r.z * r.z,
				r.x * r.x + r.z * r.z,
				r.x * r.x + r.y * r.y
				);
		return baseTensor + (mass * rSquared);
	}
}

public class LabelRequest {
	public Vector3 position;
	public string text;
	public GUIStyle style;
	public Color color;
	public float lifespan;
}
public class DrawRequest {
	public Action action;
	public float lifespan;
}

[Serializable] public struct FloatRange { public float Min, Max; }
[Serializable] public struct IntRange { public int Min, Max; }

public class NamedRangeAttribute : PropertyAttribute {
	public readonly string MinLabel;
	public readonly string MaxLabel;

	public NamedRangeAttribute(string minLabel, string maxLabel) {
		MinLabel = minLabel;
		MaxLabel = maxLabel;
	}
}

public static class NavMeshExt {
	public static (int a, int b) OrderEdge(this (int a, int b) edge) => edge.a < edge.b ? (edge.a, edge.b) : (edge.b, edge.a);
}

public class Vector3Comparer : IEqualityComparer<Vector3> {
	private readonly float inverseTolerance;

	public Vector3Comparer(float tolerance = 0.001f) {
		inverseTolerance = 1f / tolerance;
	}

	private (int, int, int) GetGridCoords(Vector3 v) {
		int hx = Mathf.RoundToInt(v.x * inverseTolerance);
		int hy = Mathf.RoundToInt(v.y * inverseTolerance);
		int hz = Mathf.RoundToInt(v.z * inverseTolerance);
		return (hx, hy, hz);
	}

	public bool Equals(Vector3 v1, Vector3 v2) {
		return GetGridCoords(v1) == GetGridCoords(v2);
	}

	public int GetHashCode(Vector3 v) {
		var gridCoords = GetGridCoords(v);
		return HashCode.Combine(gridCoords.Item1, gridCoords.Item2, gridCoords.Item3);
	}
}

public class EdgeComparer : IEqualityComparer<(Vector3 a, Vector3 b)> {
	private readonly IEqualityComparer<Vector3> vectorComparer;

	public EdgeComparer(float tolerance = 0.001f) {
		vectorComparer = new Vector3Comparer(tolerance);
	}

	public bool Equals((Vector3, Vector3) edge1, (Vector3, Vector3) edge2) {
		var norm1 = Normalize(edge1);
		var norm2 = Normalize(edge2);
		return vectorComparer.Equals(norm1.a, norm2.a) && 
			vectorComparer.Equals(norm1.b, norm2.b);
	}

	public int GetHashCode((Vector3 a, Vector3 b) edge) {
		var ordered = Normalize(edge);
		int hashA = vectorComparer.GetHashCode(ordered.a);
		int hashB = vectorComparer.GetHashCode(ordered.b);
		return HashCode.Combine(hashA, hashB);
	}

	private (Vector3 a, Vector3 b) Normalize((Vector3 a, Vector3 b) edge) {
		Vector3 a = edge.a;
		Vector3 b = edge.b;
		return a.x < b.x || (a.x == b.x && (a.y < b.y || (a.y == b.y && a.z < b.z))) ?
			(a, b) : (b, a);
	}
}
