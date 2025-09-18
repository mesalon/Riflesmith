using UnityEngine;

public class GrabPoseUtility : MonoBehaviour {
    [SerializeField] public Quaternion[] gripPose;

    public void Capture() {
        Transform[] bones = GetComponentsInChildren<Transform>(); // Includes the root transform for some reason
        gripPose = new Quaternion[bones.Length];
        for (int i = 0; i < bones.Length; i++)
            gripPose[i] = bones[i].localRotation;
    }
}