using UnityEngine;

public class SnapToFront : MonoBehaviour {
	void Start() {
		Quaternion yawRotation = Quaternion.Euler(0, VRPlayer.Input.head.rotation.eulerAngles.y, 0);
		transform.position = yawRotation * transform.position;
		transform.rotation = yawRotation;
	}
}
