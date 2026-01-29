using UnityEngine;

public class Human : MonoBehaviour {
	public Vector3 Center => (LShoulder.position + RShoulder.position + LHip.position + RHip.position) / 4;
	public Locomotion locomotion;
	public GearManager gearManager;
	public Animator anim;
	public float health = 100;
	public int IFF;
	[SerializeField] Transform LShoulder, RShoulder, LHip, RHip;

	private void Awake() {
		locomotion = new(this);
		gearManager = new(this);
	}

	private void Update() {
		locomotion.Tick();
	}

	public void Damage(float amount) {
		health = Mathf.Max(0, health - amount);
	}
}
