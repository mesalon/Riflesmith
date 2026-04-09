using UnityEngine;
/*
public class RecoilTest : MonoBehaviour {
	[SerializeField] Vector3 impulse;
	[SerializeField] float damping;
	[SerializeField] float frequency;
	[SerializeField] Vector3 target;
	private Vector3 velocity;
	private Vector3 current;

	private Vector3 start;
	private void Start() { start = transform.position; }
	private void Update() {
		if (Input.GetMouseButtonDown(0)) { velocity += impulse; }

		float f = 1f + 2f * Time.deltaTime * damping * frequency;
		float oo = frequency * frequency;
		float ho = Time.deltaTime * oo;
		float hhoo = Time.deltaTime * ho;
		float det = f + hhoo;

		float detInv = 1f / det;
		float detX = f * current + Time.deltaTime * velocity + hhoo * target;
		float detV = velocity + ho * (target - current);

		current = detX * detInv;
		velocity = detV * detInv;
		transform.position = start + new Vector3(0, current, 0);
	}
}
*/
