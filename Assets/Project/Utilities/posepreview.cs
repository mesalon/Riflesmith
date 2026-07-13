using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class posepreview : MonoBehaviour {
	[SerializeField] Transform[] bones;
	[SerializeField] List<HandPoseObject> poses;
	[SerializeField] List<float> weights;
	[SerializeField] float neutralWeight;
	private HandPose neutralPose;

	void Awake() {
		neutralPose = new(bones.Length);
		for (int i = 0; i < bones.Length; i++) {
			neutralPose.poses[i] = new(bones[i].localPosition, bones[i].localRotation);
			neutralPose.mask[i] = true;
		}
	}

	void Update() {
		var col = new List<(HandPose, float)> { (neutralPose, neutralWeight) };
		col.AddRange(poses.Zip(weights, (p, m) => (p.data, m)));
		HandPose.Apply(HandPose.Blend(col.ToArray()), bones);
	}
}
