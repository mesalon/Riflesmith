using UnityEngine;

public class ChildOrientTest : MonoBehaviour {
	[SerializeField] Transform target, child;
	void Update() {
		transform.position = target.position - target.rotation * Quaternion.Inverse(child.localRotation) * child.localPosition;
		transform.rotation = target.rotation * Quaternion.Inverse(child.localRotation);
	}
}
