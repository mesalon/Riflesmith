using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class HandPose {
	public Pose[] poses;
	public bool[] mask;

	public HandPose(int length) {
		poses = new Pose[length];
		mask = new bool[length];
	}

	private static void ThrowLengthError() => Debug.LogError("Length mismatch for hand pose! Stinky! This should never happen.");

	public static void Apply(HandPose pose, Transform[] bones, bool mirrored = false) {
		if (bones.Length != pose.poses.Length) { ThrowLengthError(); return; }
		for (int i = 0; i < pose.poses.Length; i++) {
			if (pose.mask[i]) { 
				Vector3 pos = pose.poses[i].position;
				Quaternion rot = pose.poses[i].rotation;
				if (mirrored) {
					pos = -pos;
				}
				bones[i].SetPose(pos, rot, Space.Self); 
			}
		}
	}

	public static HandPose Lerp(HandPose a, HandPose b, float t) {
		HandPose result = new(a.poses.Length);
		for (int i = 0; i < a.poses.Length; i++) {
			result.mask[i] = b.mask[i];
			if (result.mask[i]) {
				float tt = a.mask[i] ? (b.mask[i] ? t : 0) : 1;
				result.poses[i] = new(
						Vector3.Lerp(a.poses[i].position, b.poses[i].position, tt), 
						Quaternion.Lerp(a.poses[i].rotation, b.poses[i].rotation, tt)); 
			}
		}
		return result;
	}

	public static HandPose Blend(params (HandPose p, float w)[] poses) {
		float totalW = 0;
		int length = poses[0].p.poses.Length;
		HandPose result = new(length);
		foreach (var (p, w) in poses) { 
			if (p.poses.Length != length) { ThrowLengthError(); return null; }
			totalW += w; 
		}

		for (int i = 0; i < length; i++) {
			var poss = new List<(Vector3, float)>();
			var rots = new List<(Quaternion, float)>();
			for (int j = 0; j < poses.Length; j++) {
				(HandPose p, float w) = poses[j];
				if (p.mask[i]) {
					poss.Add((p.poses[i].position, w));
					rots.Add((p.poses[i].rotation, w));
					result.mask[i] = true;
				}
			}
			result.poses[i].position = poss.WeighedAverage();
			result.poses[i].rotation = rots.WeighedAverage();
		}
		return result;
	}

	public void Sequence(Transform[] bones, bool mirrored = false, params (HandPose p, float w)[] poses) {
		foreach (var (p, w) in poses) {
			Apply(Lerp(this, p, w), bones, mirrored);
		}
	}
}
