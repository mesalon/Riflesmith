using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.UIElements;
#endif

[System.Serializable] public class HandPose {
	public Pose[] poses;
	public bool[] mask;

	public HandPose(int length) {
		poses = new Pose[length];
		mask = new bool[length];
	}

	public static void Apply(Pose[] poses, Transform[] bones) {
		for (int i = 0; i < poses.Length; i++) {
			bones[i].SetPose(poses[i].position, poses[i].rotation, Space.Self);
		}
	}

	public void Apply(Transform[] bones) {
		if (bones.Length != poses.Length) {
			Debug.LogError("Length mismatch for hand pose! Stinky! This should never happen.");
			return;
		}
		for (int i = 0; i < poses.Length; i++) {
			if (mask[i]) { bones[i].SetPose(poses[i].position, poses[i].rotation, Space.Self); }
		}
	}


	public static HandPose Lerp(HandPose a, HandPose b, float t) {
		HandPose result = new(a.poses.Length);
		for (int i = 0; i < a.poses.Length; i++) {
			result.mask[i] = a.mask[i] || b.mask[i];
			if (result.mask[i]) {
				float tt = a.mask[i] ? (b.mask[i] ? t : 0) : 1;
				result.poses[i] = new(
						Vector3.Lerp(a.poses[i].position, b.poses[i].position, tt), 
						Quaternion.Lerp(a.poses[i].rotation, b.poses[i].rotation, tt)); 
			}
		}
		return result;
	}

	public static HandPose Blend(params (HandPose, float)[] poses) {
		return null;
	}


#if UNITY_EDITOR
#endif
}

