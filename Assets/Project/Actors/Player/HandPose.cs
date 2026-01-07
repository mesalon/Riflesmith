using UnityEngine;

[System.Serializable] public class HandPose {
	public Quaternion[] poses;
	public bool isMirrored;

	public Quaternion this[int i] {
		get => isMirrored ? new(-poses[i].x, poses[i].y, poses[i].z, -poses[i].w) : poses[i];
	}

	public static void Lerp(HandPose a, HandPose b, float t, Quaternion[] result) {
		for (int i = 0; i < result.Length; i++) { result[i] = Quaternion.Lerp(a[i], b[i], t); }
	}
}
