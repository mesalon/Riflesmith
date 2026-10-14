using UnityEngine;

public class LineUpYouBastard : MonoBehaviour {
	[SerializeField] Transform target;
	[SerializeField] Transform a;

	void Update() {
		transform.rotation = target.transform.rotation * Quaternion.Inverse(a.localRotation);
		transform.position = target.transform.position - transform.rotation * a.localPosition;
	}
}
