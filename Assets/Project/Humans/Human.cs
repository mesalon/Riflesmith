using UnityEngine;

public class Human : MonoBehaviour {
	public Vector3 Center => (LShoulder.position + RShoulder.position + LHip.position + RHip.position) / 4;
	public Locomotion locomotion;
	public float health = 100;
	public int IFF;
	public Animator anim;
	[SerializeField] Transform LShoulder, RShoulder, LHip, RHip;

	private void Awake() {
		locomotion = new(this);
	}

	private void Update() {
		locomotion.Tick();
	}

	public void Damage(float amount) {
		health = Mathf.Max(0, health - amount);
	}
}
