using UnityEngine;

[System.Serializable] public class HandPose {
	public Quaternion[] poses;

	public static void Lerp(HandPose a, HandPose b, float t, Transform[] bones, bool isMirrored = false) {
		if (bones.Length != a.poses.Length || a.poses.Length != b.poses.Length) {
			Debug.LogError("Bone length mismatch for hand!");
			return;
		}
		for (int i = 0; i < bones.Length; i++) { 
			bones[i].localRotation = Quaternion.Lerp(
					isMirrored ? a.Mirrored(i) : a.poses[i], 
					isMirrored ? a.Mirrored(i) : a.poses[i], t); 
		}
	}
	public Quaternion Mirrored(int i) => new(-poses[i].x, poses[i].y, poses[i].z, -poses[i].w);
}
