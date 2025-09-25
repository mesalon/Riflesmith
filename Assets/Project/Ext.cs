using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public static class Ext {
	public static List<LabelRequest> labelQueue = new();
	public static List<DrawRequest> drawQueue = new();

	// todo: combine all these into one somehow
	public static bool DidPass(this float current, float amount, float previous) => current > amount && previous <= amount;
	public static bool DidFail(this float current, float amount, float previous) => current < amount && previous >= amount;
	public static bool DidReach(this float current, float amount, float previous) => current >= amount && previous < amount;
	public static bool DidRecede(this float current, float amount, float previous) => current <= amount && previous > amount;

	public static double DebugTimestamp(this long stamp, string msg = "", bool message = true) {
		double ms = (Stopwatch.GetTimestamp() - stamp) * 1000.0 / Stopwatch.Frequency;
		if (message) Debug.Log($"Execution of {(msg == String.Empty ? "method" : msg)} took {ms} ms");
		return ms;
	}

	public static StringBuilder AppendLines(this StringBuilder str, params object[] lines) {
		foreach (object o in lines) { str.AppendLine(o?.ToString()); }
		return str;
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

	public static float GetPathLength(this Vector3[] path) {
		float length = 0;
		for (int i = 0; i < path.Length - 1; i++) { length += Vector3.Distance(path[i], path[i + 1]); }
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

	public static void AddToPropertyList(this SerializedProperty list, Object value) {
		list.arraySize++;
		list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
	}

	public static void Label(Vector3 position, string text, float lifespan = 0, GUIStyle style = null, Color color = default) {
		style ??= new() {
			alignment = TextAnchor.MiddleCenter, 
			normal = new() { textColor = color == default ? Color.white : color }
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

	public static void PrintAll<T>(this IEnumerable<T> objects) { Debug.Log(String.Join(", ", objects)); }

	public static void DrawPath(Vector3[] path, Color startCol = default, Color endCol = default, float offset = 0) {
		for (int i = 0; i < path.Length - 1; i++) {
			Debug.DrawLine(path[i] + Vector3.up * offset, path[i + 1] + Vector3.up * offset,
					Color.Lerp(startCol == default ? Color.green : startCol, endCol == default ? Color.red : endCol,
						i / ((float)path.Length - 1)));
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

[System.Serializable] public struct FloatRange { public float Min, Max; }
[System.Serializable] public struct IntRange { public int Min, Max; }

public class NamedRangeAttribute : PropertyAttribute {
	public readonly string MinLabel;
	public readonly string MaxLabel;

	public NamedRangeAttribute(string minLabel, string maxLabel) {
		this.MinLabel = minLabel;
		this.MaxLabel = maxLabel;
	}
}
