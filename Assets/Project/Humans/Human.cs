using UnityEngine;

public class Human : MonoBehaviour {
	public Vector3 Center => (LShoulder.position + RShoulder.position + LHip.position + RHip.position) / 4;
	public Locomotion locomotion;
	public GearManager gearManager;
	public float health = 100;
	public int IFF;
	[HideInInspector] public Animator anim;
	[SerializeField] Transform LShoulder, RShoulder, LHip, RHip;

	private void Awake() {
		anim = GetComponent<Animator>();
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
