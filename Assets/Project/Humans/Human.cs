using UnityEngine;

public class Human : MonoBehaviour {
	public Vector3 Center => (LShoulder.position + RShoulder.position + LHip.position + RHip.position) / 4;
	public float health = 100;
	public int IFF;
	[SerializeField] Transform LShoulder, RShoulder, LHip, RHip;

	private void Awake() {
	}

	private void Update() {
	}

	public void Damage(float amount) {
		health = Mathf.Max(0, health - amount);
	}

}
