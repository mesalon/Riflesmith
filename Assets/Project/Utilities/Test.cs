using UnityEngine;

public class Test : MonoBehaviour {
	[SerializeField] Transform x, y, z;
	void Update() {
		Vector3 a = x.position;
		Vector3 b = y.position;
		Vector3 c = z.position;

		Vector3 ab = b - a;
		float len = ab.magnitude;
		Vector3 dir = ab / len;
		float t = Vector3.Dot(c - a, dir);
		t = Mathf.Clamp(t, 0f, len);
		Vector3 pos = a + dir * t;

		VRGizmos.Ray(a, dir, Color.red);
		VRGizmos.Ray(a, dir * len, Color.blue);
		VRGizmos.Sphere(a, 0.01f);
		VRGizmos.Sphere(pos, 0.01f);
		VRGizmos.Line(a, b);
		VRGizmos.Line(a, c);
		Ext.Label(a, $"{t:f2}");
	}
}
