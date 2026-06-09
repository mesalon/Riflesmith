using UnityEngine;

public class matcher : MonoBehaviour {
	[SerializeField] Transform a, b, c;
	[SerializeField] Transform d, e, f;
	private ConfigurableJoint aj, bj, cj;

	void Awake() {
		aj = a.GetComponent<ConfigurableJoint>();
		bj = b.GetComponent<ConfigurableJoint>();
		cj = c.GetComponent<ConfigurableJoint>();
	}

	void Update() {
		Ext.DrawSkeleton(a, Color.blue);
		Ext.DrawSkeleton(d, Color.green);
	}

	void FixedUpdate() {
		aj.targetRotation = Quaternion.Inverse(transform.rotation) * d.rotation;
		bj.targetRotation = e.localRotation;
		cj.targetRotation = f.localRotation;
	}
}
